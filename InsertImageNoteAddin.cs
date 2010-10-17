using System;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;
using System.IO;
using System.Text;
using Tomboy.InsertImage.Action;
//using AM = Mono.Addins.AddinManager;

namespace Tomboy.InsertImage
{
	public class InsertImageNoteAddin : NoteAddin
	{
		Gtk.ImageMenuItem insertLocalImageMenuItem;
		Gtk.ImageMenuItem insertWebImageMenuItem;
		List<ImageInfo> imageInfoList = new List<ImageInfo> ();

		const string SAVE_HEAD = "\n\n\n[Tomboy.InsertImage]\nThe following content is used by " +
			"tomboy's InsertImage plugin to save the contents of inserted images.\n";
		const string SAVE_TAIL = "\n\n[/Tomboy.InsertImage]\n";
		//List<ImageInfo> deletedImages = new List<ImageInfo> ();

		static InsertImageNoteAddin ()
		{
			if (NoteTagTable.Instance.Lookup ("image") == null) {
				NoteTagTable.Instance.Add (new ImageTag ());
			}
		}


		public override void Initialize ()
		{
			if (!Note.TagTable.IsDynamicTagRegistered ("imagebox"))
				Note.TagTable.RegisterDynamicTag ("imagebox", typeof (ImageBoxTag));
		}

		public override void OnNoteOpened ()
		{
			insertLocalImageMenuItem = new Gtk.ImageMenuItem (
				Catalog.GetString ("Insert Local Image"));
			insertLocalImageMenuItem.Image = new Gtk.Image (Gtk.Stock.Harddisk, Gtk.IconSize.Menu);
			insertLocalImageMenuItem.Activated += OnInsertLocalImage;
			insertLocalImageMenuItem.AddAccelerator ("activate", Window.AccelGroup,
				(uint)Gdk.Key.l, Gdk.ModifierType.ControlMask | Gdk.ModifierType.ShiftMask,
				Gtk.AccelFlags.Visible);
			insertLocalImageMenuItem.Show ();
			AddPluginMenuItem (insertLocalImageMenuItem);

			insertWebImageMenuItem = new Gtk.ImageMenuItem (
				Catalog.GetString ("Insert Web Image"));
			insertWebImageMenuItem.Image = new Gtk.Image (Gtk.Stock.Network, Gtk.IconSize.Menu);
			insertWebImageMenuItem.Activated += OnInsertWebImage;
			insertWebImageMenuItem.AddAccelerator ("activate", Window.AccelGroup,
				(uint)Gdk.Key.w, Gdk.ModifierType.ControlMask | Gdk.ModifierType.ShiftMask,
				Gtk.AccelFlags.Visible);
			insertWebImageMenuItem.Show ();
			AddPluginMenuItem (insertWebImageMenuItem);

			LoadImageBoxes ();

			Note.Saved += OnNoteSaved;
			Buffer.DeleteRange += new DeleteRangeHandler (Buffer_DeleteRange);
		}

		[GLib.ConnectBefore]
		void Buffer_DeleteRange (object o, DeleteRangeArgs args)
		{
			// TODO can Tomboy allow me to access frozen_cnt?
			//if (Buffer.Undoer.frozen_cnt == 0) ...
			var iter = args.Start;
			//DeleteImageAction action = null;
			var imagesToDel = new List<ImageInfo> ();
			while (iter.Offset < args.End.Offset) {
				foreach (var mark in iter.Marks) {
					foreach (var imageInfo in imageInfoList) {
						if (mark == imageInfo.Mark) {
							//An embeded image is deleted.
							// TODO implement Undo/Redo delete image action.
							//action = new DeleteImageAction (this, imageInfo, imageInfoList);
							//Buffer.Undoer.AddUndoAction (action);
							imagesToDel.Add (imageInfo);
						}
					}
				}
				if (!iter.ForwardChar ())
					break;
			}
			foreach (var info in imagesToDel)
				imageInfoList.Remove (info);
		}

		public override void Shutdown ()
		{
			Note.Saved -= OnNoteSaved;
			if (insertLocalImageMenuItem != null)
				insertLocalImageMenuItem.Activated -= OnInsertLocalImage;
			if (insertWebImageMenuItem != null)
				insertWebImageMenuItem.Activated -= OnInsertWebImage;
		}

		void OnNoteSaved (Note note)
		{
			if (imageInfoList.Count > 0) {
				var fileContent = File.ReadAllText (Note.FilePath);
				var sb = new StringBuilder (4096);
				int contentEndIndex = fileContent.IndexOf ("</note-content>");
				sb.Append (fileContent.Substring (0, contentEndIndex));
				sb.AppendFormat ("<image>{0};", SAVE_HEAD);
				imageInfoList.Sort (new ImageInfoComparerByPosition ());
				foreach (var imageInfo in imageInfoList) {
					Gdk.Size displaySize = imageInfo.Widget.ImageSize;
					imageInfo.DisplayWidth = displaySize.Width;
					imageInfo.DisplayHeight = displaySize.Height;
					sb.AppendFormat ("{0}:", imageInfo.Position);
					sb.Append (imageInfo.SaveAsString());
					sb.Append (";");
				}
				sb.AppendFormat ("{0}</image>", SAVE_TAIL);
				sb.Append (fileContent.Substring (contentEndIndex));
				File.WriteAllText (Note.FilePath, sb.ToString());
			}
		}

		private void LoadImageBoxes ()
		{
			var imageTag = Buffer.TagTable.Lookup("image");
			TextIter pos = Buffer.StartIter;
			pos.ForwardLine ();
			bool foundTag;
			TextIter imageBegin, imageEnd;
			Buffer.Undoer.FreezeUndo ();
			while (true) {
				foundTag = pos.ForwardToTagToggle (imageTag);
				if (!foundTag)
					break;
				imageBegin = pos;
				foundTag = pos.ForwardToTagToggle (imageTag);
				if (!foundTag)
					// TODO Report error: can't find </image>
					break;
				imageEnd = pos;
				string imageElementValue = Buffer.GetSlice (imageBegin, imageEnd, true);
				Buffer.Delete (ref imageBegin, ref imageEnd);
				pos = imageBegin;

				// TODO, current saveInfo reading is extremely inefficient.
				foreach (var saveInfo in imageElementValue.Split (new char [] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
					if (saveInfo.Trim () == SAVE_HEAD.Trim ())
						continue;
					if (saveInfo.Trim () == SAVE_TAIL.Trim ())
						break;
					int colonIndex = saveInfo.IndexOf (":");
					if (colonIndex == -1)
						throw new FormatException (Catalog.GetString("Invalid <image> format"));
					int offset = int.Parse (saveInfo.Substring (0, colonIndex));
					ImageInfo info = ImageInfo.FromSavedString (saveInfo.Substring(colonIndex + 1));
					InsertImage (Buffer.GetIterAtOffset(offset), info, false);
				}
			}
			Buffer.Undoer.ThawUndo ();
		}

		void OnInsertLocalImage (object sender, EventArgs args)
		{
			InsertImage (LocalImageChooser.Instance);
		}

		void OnInsertWebImage (object sender, EventArgs args)
		{
			InsertImage (WebImageChooser.Instance);
		}

		private void InsertImage (IImageInfoChooser chooser)
		{
			ImageInfo imageInfo = null;
			try {
				imageInfo = chooser.ChooseImageInfo (Note.Window);
			} catch {
				// TODO: Report the open file error.
				imageInfo = null;
			}
			if (imageInfo == null)
				return;

			TextIter currentIter = Buffer.GetIterAtOffset (Buffer.CursorPosition);
			InsertImage (currentIter, imageInfo, true);
		}

		public ImageBoxTag InsertImage (TextIter iter, ImageInfo imageInfo, bool supportUndo)
		{
			Gdk.Pixbuf pixbuf = null;
			try {
				pixbuf = new Gdk.Pixbuf (imageInfo.FileContent);
			} catch {
				pixbuf = null;
			}
			if (pixbuf == null) {
				// TODO: Report the open image error.
				return null;
			}

			var imageWidget = new ImageWidget (pixbuf);
			imageWidget.ResizeImage (imageInfo.DisplayWidth, imageInfo.DisplayHeight);
			imageWidget.ShowAll ();


			imageInfo.Mark = Buffer.CreateMark (null, iter, true);

			ImageBoxTag tag = (ImageBoxTag)Note.TagTable.CreateDynamicTag ("imagebox");
			tag.ImageInfo = imageInfo;
			imageInfo.Widget = imageWidget;
			tag.Widget = imageWidget;
			imageWidget.Resized += imageWidget_Resized;

			TextTag [] tags = { tag };
			if (supportUndo)
				Buffer.Undoer.FreezeUndo ();
			Buffer.InsertWithTags (ref iter, String.Empty, tags);

			//imageWidget.Destroyed += (o, e) =>
			//{
			//    if (!imageWidget.InsertUndone) {
			//        imageInfoList.Remove (imageInfo);
			//    }
			//};

			if (supportUndo) {
				Buffer.Undoer.ThawUndo ();
				var action = new InsertImageAction (this, tag, imageInfoList);
				Buffer.Undoer.AddUndoAction (action);
			}
			imageInfoList.Add (imageInfo);

			return tag;
		}

		void imageWidget_Resized (object sender, ResizeEventArgs e)
		{
			ImageWidget widget = (ImageWidget)sender;
			var action = new ResizeImageAction (widget, e.OldWidth, e.OldHeight, e.NewWidth, e.NewHeight);
			Buffer.Undoer.AddUndoAction (action);
		}
	}
}

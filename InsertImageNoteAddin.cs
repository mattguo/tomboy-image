using System;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;
using System.IO;
using System.Text;
using Tomboy.InsertImage.Action;
using System.Reflection;
//using AM = Mono.Addins.AddinManager;

namespace Tomboy.InsertImage
{
	public class InsertImageNoteAddin : NoteAddin
	{
		Gtk.ImageMenuItem insertLocalImageMenuItem;
		Gtk.ImageMenuItem insertWebImageMenuItem;
		List<ImageInfo> imageInfoList = new List<ImageInfo> ();

		const string SAVE_HEAD = "[Tomboy.InsertImage]";
		const string SAVE_TAIL = "[/Tomboy.InsertImage]";

		// For debug "Add Help Note" only.
		//static InsertImageNoteAddin ()
		//{
		//    InsertImagePreferences.HelpNoteAdded = false;
		//}

		public override void Initialize ()
		{
			if (!InsertImagePreferences.HelpNoteAdded) {
				InsertImagePreferences.HelpNoteAdded = true;
				try {
					AddHelpNote ();
				}
				catch (Exception ex) {
					Message.Error ("Can't add help note{0}{1}", Environment.NewLine, ex);
				}
			}
			//} else if (newHelpNoteId != null && Note.Id == newHelpNoteId) {
			//    Note.Buffer.Undoer.ClearUndoHistory ();
			//}
		}

		public static void AddHelpNote ()
		{
			Note helpNote = Tomboy.DefaultNoteManager.Create ("Using Tomboy.InsertImage");
			var stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("Tomboy.InsertImage.HelpNoteContent.txt");
			helpNote.XmlContent = new StreamReader (stream).ReadToEnd ();
			helpNote.Buffer.Undoer.ClearUndoHistory ();
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
			// TODO dirty hacks to get private field
			var frozen_cnt = ReflectionUtil.GetFieldValue<uint> (
				Buffer.Undoer, "frozen_cnt", BindingFlags.NonPublic | BindingFlags.Instance);
			if (frozen_cnt == 0) {
				var iter = args.Start;
				var imagesToDel = new List<ImageInfo> ();
				while (iter.Offset < args.End.Offset) {
					var imageInfo = FindImageInfoByAnchor (iter.ChildAnchor);
					if (imageInfo != null) {
						//var action = new DeleteImageAction (this, imageInfo, imageInfoList, args.Start.Offset);
						//Buffer.Undoer.AddUndoAction (action);
						imagesToDel.Add (imageInfo);
					}
					if (!iter.ForwardChar ())
						break;
				}
				if (imagesToDel.Count > 0) {
					// TODO dirty hacks to retrieve Tomboy's private field value
					var undoStack = ReflectionUtil.GetFieldValue<Stack<EditAction>> (
						Buffer.Undoer, "undo_stack", BindingFlags.NonPublic | BindingFlags.Instance);
					EditAction lastAction = null;
					EraseAction lastEraseAction = null;
					if (undoStack != null)
						lastAction = undoStack.Pop ();
					lastEraseAction = lastAction as EraseAction;
					System.Diagnostics.Debug.Assert (lastAction != null, lastAction != null ? lastAction.GetType ().FullName : "<null>");
					foreach (var info in imagesToDel) {
						info.DisplayWidth = info.Widget.ImageSize.Width;
						info.DisplayHeight = info.Widget.ImageSize.Height;
						imageInfoList.Remove (info);
					}
					var action = new DeleteImageAction (this, lastEraseAction, imagesToDel, imageInfoList);
					Buffer.Undoer.AddUndoAction (action);
				}
			}
		}

		private ImageInfo FindImageInfoByAnchor (TextChildAnchor anchor)
		{
			if (anchor == null)
				return null;
			foreach (var info in imageInfoList) {
				if (info.Anchor == anchor)
					return info;
			}
			return null;
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
				sb.AppendFormat ("{0};", SAVE_HEAD);
				imageInfoList.Sort (new ImageInfoComparerByPosition ());
				foreach (var imageInfo in imageInfoList) {
					Gdk.Size displaySize = imageInfo.Widget.ImageSize;
					imageInfo.DisplayWidth = displaySize.Width;
					imageInfo.DisplayHeight = displaySize.Height;
					sb.AppendFormat ("{0}:", imageInfo.Position);
					sb.Append (imageInfo.SaveAsString ());
					sb.Append (";");
				}
				sb.Append (SAVE_TAIL);
				sb.Append (fileContent.Substring (contentEndIndex));
				File.WriteAllText (Note.FilePath, sb.ToString ());
			}
		}

		private void LoadImageBoxes ()
		{
			TextIter start = Buffer.StartIter;
			start.ForwardLine ();
			TextIter end = Buffer.EndIter;
			TextIter saveStart, saveEnd, tmpIter;
			bool foundSaveInfo = start.ForwardSearch (SAVE_HEAD, TextSearchFlags.TextOnly, out saveStart, out tmpIter, end);
			if (foundSaveInfo) {
				foundSaveInfo = saveStart.ForwardSearch (SAVE_TAIL, TextSearchFlags.TextOnly, out tmpIter, out saveEnd, end);
				if (foundSaveInfo) {
					Buffer.Undoer.FreezeUndo ();
					string imageElementValue = Buffer.GetSlice (saveStart, saveEnd, true);
					Buffer.Delete (ref saveStart, ref saveEnd);
					// TODO, current saveInfo reading is extremely inefficient.
					foreach (var saveInfo in imageElementValue.Split (new char [] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
						if (saveInfo.Trim () == SAVE_HEAD.Trim ())
							continue;
						if (saveInfo.Trim () == SAVE_TAIL.Trim ())
							break;
						int colonIndex = saveInfo.IndexOf (":");
						if (colonIndex == -1)
							throw new FormatException (Catalog.GetString ("Invalid <image> format"));
						int offset = int.Parse (saveInfo.Substring (0, colonIndex));
						ImageInfo info = ImageInfo.FromSavedString (saveInfo.Substring (colonIndex + 1), true);
						InsertImage (Buffer.GetIterAtOffset (offset), info, false);
					}
					Buffer.Undoer.ThawUndo ();
				}
			}
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
			}
			catch {
				// TODO: Report the open file error.
				imageInfo = null;
			}
			if (imageInfo == null)
				return;

			TextIter currentIter = Buffer.GetIterAtOffset (Buffer.CursorPosition);
			InsertImage (currentIter, imageInfo, true);
		}

		public void InsertImage (TextIter iter, ImageInfo imageInfo, bool supportUndo)
		{
			Gdk.Pixbuf pixbuf = null;
			try {
				pixbuf = new Gdk.Pixbuf (imageInfo.FileContent);
			}
			catch {
				pixbuf = null;
			}
			if (pixbuf == null) {
				// TODO: Report the open image error.
				return;
			}

			if (imageInfo.DisplayWidth == 0) {
				imageInfo.DisplayWidth = pixbuf.Width;
				imageInfo.DisplayHeight = pixbuf.Height;
			}

			var imageWidget = new ImageWidget (pixbuf);
			imageWidget.ResizeImage (imageInfo.DisplayWidth, imageInfo.DisplayHeight);
			imageWidget.ShowAll ();
			InitImageWidgetContextMenu (imageWidget, imageInfo);
			imageWidget.Resized += imageWidget_Resized;

			if (supportUndo)
				Buffer.Undoer.FreezeUndo ();
			var anchorStart = iter;
			var anchor = Buffer.CreateChildAnchor (ref iter);

			var tag = new NoteTag ("dummy");
			tag.CanUndo = false;
			Buffer.ApplyTag (tag, anchorStart, iter);

			Window.Editor.AddChildAtAnchor (imageWidget, anchor);
			imageInfo.SetInBufferInfo (Buffer, anchor, imageWidget);

			//imageWidget.Destroyed += (o, e) =>
			//{
			//    if (!imageWidget.InsertUndone) {
			//        imageInfoList.Remove (imageInfo);
			//    }
			//};

			if (supportUndo) {
				Buffer.Undoer.ThawUndo ();
				var action = new InsertImageAction (this, imageInfo, imageInfoList);
				Buffer.Undoer.AddUndoAction (action);
			}
			imageInfoList.Add (imageInfo);
		}

		private void InitImageWidgetContextMenu (ImageWidget imageWidget, ImageInfo imageInfo)
		{
			Gtk.ImageMenuItem saveAs = new Gtk.ImageMenuItem (Catalog.GetString ("Save as..."));
			saveAs.Image = new Gtk.Image (Gtk.Stock.SaveAs, Gtk.IconSize.Menu);
			saveAs.Activated += (o, e) => SaveImage (Note.Window, imageInfo);
			imageWidget.ContextMenu.Append (saveAs);

			Gtk.ImageMenuItem prop = new Gtk.ImageMenuItem (Catalog.GetString ("Properties"));
			prop.Image = new Gtk.Image (Gtk.Stock.Properties, Gtk.IconSize.Menu);
			prop.Activated += (o, e) => ShowImageProperties (Note.Window, imageInfo);
			imageWidget.ContextMenu.Append (prop);

			imageWidget.ContextMenu.ShowAll ();
		}

		private void SaveImage (Window parent, ImageInfo imageInfo)
		{
			var fc = new FileChooserDialog (
				string.Format (Catalog.GetString ("Save {0} in"), Path.GetFileName (imageInfo.FilePath)),
				parent,
				FileChooserAction.SelectFolder,
				Catalog.GetString ("Cancel"), ResponseType.Cancel,
				Catalog.GetString ("Select"), ResponseType.Accept);
			if (fc.Run () == (int)ResponseType.Accept) {
				string imagePath = null;
				try {
					imagePath = imageInfo.SaveAs (fc.Filename);
					Message.Info (Catalog.GetString ("Saved \"{0}\""), imagePath);
				}
				catch (Exception ex) {
					Message.Error (Catalog.GetString("Failed to save image, {0}"), ex);
				}
			}
			fc.Destroy ();
		}

		private void ShowImageProperties (Window parent, ImageInfo imageInfo)
		{
			// TODO
			Message.Info ("Not implemented yet");
		}

		private void imageWidget_Resized (object sender, ResizeEventArgs e)
		{
			ImageWidget widget = (ImageWidget)sender;
			ImageInfo info = imageInfoList.Find (ii => ii.Widget == widget);
			if (info != null) {
				var action = new ResizeImageAction (info, e.OldWidth, e.OldHeight, e.NewWidth, e.NewHeight);
				Buffer.Undoer.AddUndoAction (action);
			}
		}
	}
}

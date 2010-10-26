using System;
using System.Collections.Generic;
using Gtk;
using System.Diagnostics;

namespace Tomboy.InsertImage.Action
{
	public class InsertImageAction : EditAction
	{
		private InsertImageNoteAddin addin;
		private ImageInfo imageInfo;
		private List<ImageInfo> imageInfoList;
		private int imagePosition = -1;

		public InsertImageAction (InsertImageNoteAddin addin, ImageInfo imageInfo, List<ImageInfo> imageInfoList)
		{
			this.addin = addin;
			this.imageInfo = imageInfo;
			this.imageInfoList = imageInfoList;
		}

		#region EditAction Members

		public void Undo (Gtk.TextBuffer buffer)
		{
			imagePosition = imageInfo.Position;
			imageInfoList.Remove (imageInfo);
			imageInfo.DisplayWidth = imageInfo.Widget.ImageSize.Width;
			imageInfo.DisplayHeight = imageInfo.Widget.ImageSize.Height;

			TextIter imageBoxBegin = buffer.GetIterAtOffset (imagePosition);
			TextIter imageBoxEnd = imageBoxBegin;
			imageBoxEnd.ForwardChar ();
			buffer.Delete (ref imageBoxBegin, ref imageBoxEnd);
			buffer.MoveMark (buffer.InsertMark, imageBoxBegin);
			buffer.MoveMark (buffer.SelectionBound, imageBoxBegin);
		}

		public void Redo (Gtk.TextBuffer buffer)
		{
			Debug.Assert (imagePosition != -1);
			var iter = buffer.GetIterAtOffset (imagePosition);
			addin.InsertImage (iter, imageInfo, false);
			buffer.MoveMark (buffer.InsertMark, iter);
			buffer.MoveMark (buffer.SelectionBound, iter);
			imagePosition = -1;
		}

		public void Merge (EditAction action)
		{
			throw new Exception ("InsertImageAction cannot be merged");
		}

		public bool CanMerge (EditAction action)
		{
			return false;
		}

		public void Destroy ()
		{
		}

		#endregion
	}
}

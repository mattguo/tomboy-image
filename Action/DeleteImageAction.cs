using System;
using System.Collections.Generic;
using Gtk;
using System.Diagnostics;

namespace Tomboy.InsertImage.Action
{
	public class DeleteImageAction : EditAction
	{
		private InsertImageNoteAddin addin;
		private ImageInfo imageInfo;
		private List<ImageInfo> imageInfoList;
		private int imagePosition;
		private EditAction innerAction;

		public DeleteImageAction (InsertImageNoteAddin addin, ImageInfo imageInfo,
			List<ImageInfo> imageInfoList, int deletePosition)
		{
			this.addin = addin;
			this.imageInfo = imageInfo;
			this.imageInfoList = imageInfoList;
			// TODO I can't really handle the undo/redo of batch deletion for now.
			// Recover image at the deletion_start_offset is the best I can do for now.
			this.imagePosition = deletePosition;
		}

		#region EditAction Members

		public void Undo (Gtk.TextBuffer buffer)
		{
			if (innerAction != null)
				innerAction.Undo (buffer);
			var iter = buffer.GetIterAtOffset (imagePosition);
			addin.InsertImage (iter, imageInfo, false);
			buffer.MoveMark (buffer.InsertMark, iter);
			buffer.MoveMark (buffer.SelectionBound, iter);
		}

		public void Redo (Gtk.TextBuffer buffer)
		{
			imageInfoList.Remove (imageInfo);
			int pos = imageInfo.Position;
			Debug.Assert (pos == imagePosition, "DeleteImageAction.Redo, check image position" );
			TextIter imageBoxBegin = buffer.GetIterAtOffset (pos);
			TextIter imageBoxEnd = imageBoxBegin;
			imageBoxEnd.ForwardChar ();
			buffer.Delete (ref imageBoxBegin, ref imageBoxEnd);
			buffer.MoveMark (buffer.InsertMark, imageBoxBegin);
			buffer.MoveMark (buffer.SelectionBound, imageBoxBegin);
		}

		public void Merge (EditAction action)
		{
			throw new Exception ("DeleteImageAction cannot be merged");
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

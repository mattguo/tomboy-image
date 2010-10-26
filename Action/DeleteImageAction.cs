using System;
using System.Collections.Generic;
using Gtk;
using System.Diagnostics;

namespace Tomboy.InsertImage.Action
{
	public class DeleteImageAction : EditAction
	{
		private InsertImageNoteAddin addin;
		private EraseAction innerAction;
		private Dictionary<int, ImageInfo> deletedImages = null;
		private List<ImageInfo> imageInfoList;

		public DeleteImageAction (InsertImageNoteAddin addin, EraseAction innerAction, List<ImageInfo> deletedImages,
			List<ImageInfo> imageInfoList)
		{
			this.addin = addin;
			this.innerAction = innerAction;
			this.deletedImages = new Dictionary<int, ImageInfo> ();
			foreach (var imageInfo in deletedImages)
				this.deletedImages.Add (imageInfo.Position, imageInfo);
			this.imageInfoList = imageInfoList;
		}

		#region EditAction Members

		public void Undo (Gtk.TextBuffer buffer)
		{
			if (innerAction != null)
				innerAction.Undo (buffer);
			foreach (var pair in deletedImages) {
				var iter = buffer.GetIterAtOffset (pair.Key);
				addin.InsertImage (iter, pair.Value, false);
			}
		}

		public void Redo (Gtk.TextBuffer buffer)
		{
			//foreach (var imageInfo in deletedImages
			if (innerAction != null)
				innerAction.Redo (buffer);
			//imageInfoList.Remove (imageInfo);
			//int pos = imageInfo.Position;
			//Debug.Assert (pos == imagePosition, "DeleteImageAction.Redo, check image position" );
			//TextIter imageBoxBegin = buffer.GetIterAtOffset (pos);
			//TextIter imageBoxEnd = imageBoxBegin;
			//imageBoxEnd.ForwardChar ();
			//buffer.Delete (ref imageBoxBegin, ref imageBoxEnd);
			//buffer.MoveMark (buffer.InsertMark, imageBoxBegin);
			//buffer.MoveMark (buffer.SelectionBound, imageBoxBegin);
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

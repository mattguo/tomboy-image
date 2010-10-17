using System;
using System.Collections.Generic;
using Gtk;
using System.Diagnostics;

namespace Tomboy.InsertImage.Action
{
	public class InsertImageAction : EditAction
	{
		private InsertImageNoteAddin addin;
		private ImageBoxTag imageBoxTag;
		private List<ImageInfo> imageInfoList;
		private int imagePosition = -1;

		public InsertImageAction (InsertImageNoteAddin addin, ImageBoxTag imageBoxTag, List<ImageInfo> imageInfoList)
		{
			this.addin = addin;
			this.imageBoxTag = imageBoxTag;
			this.imageInfoList = imageInfoList;
		}

		#region EditAction Members

		public void Undo (Gtk.TextBuffer buffer)
		{
			imagePosition = imageBoxTag.ImageInfo.Position;
			imageInfoList.Remove (imageBoxTag.ImageInfo);
			
			TextIter imageBoxBegin = buffer.GetIterAtMark (imageBoxTag.ImageInfo.Mark);
			TextIter imageBoxEnd = imageBoxBegin;
			bool ret = imageBoxEnd.ForwardChar ();
			buffer.Delete (ref imageBoxBegin, ref imageBoxEnd);
			buffer.MoveMark (buffer.InsertMark, imageBoxBegin);
			buffer.MoveMark (buffer.SelectionBound, imageBoxBegin);
		}

		public void Redo (Gtk.TextBuffer buffer)
		{
			Debug.Assert (imagePosition != -1);
			var iter = buffer.GetIterAtOffset (imagePosition);
			imageBoxTag = addin.InsertImage (iter, imageBoxTag.ImageInfo, false);
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

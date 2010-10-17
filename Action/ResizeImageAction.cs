using System;
using System.Collections.Generic;
using Gtk;

namespace Tomboy.InsertImage.Action
{
	public class ResizeImageAction : EditAction
	{
		private ImageWidget image;
		private int oldWidth;
		private int oldHeight;
		private int newWidth;
		private int newHeight;

		public ResizeImageAction (ImageWidget image, int oldWidth, int oldHeight, int newWidth, int newHeight)
		{
			this.image = image;
			this.oldWidth = oldWidth;
			this.oldHeight = oldHeight;
			this.newWidth = newWidth;
			this.newHeight = newHeight;
		}

		#region EditAction Members

		public void Undo (Gtk.TextBuffer buffer)
		{
			image.ResizeImage (oldWidth, oldHeight);
		}

		public void Redo (Gtk.TextBuffer buffer)
		{
			image.ResizeImage (newWidth, newHeight);
		}

		public void Merge (EditAction action)
		{
			throw new Exception ("ResizeImageAction cannot be merged");
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

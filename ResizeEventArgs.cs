using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tomboy.InsertImage
{
	public class ResizeEventArgs : EventArgs
	{
		public ResizeEventArgs (int oldWidth,
			int oldHeight,
			int newWidth,
			int newHeight)
		{
			OldWidth = oldWidth;
			OldHeight = oldHeight;
			NewWidth = newWidth;
			NewHeight = newHeight;
		}

		public int OldWidth { get; set; }
		public int OldHeight { get; set; }
		public int NewWidth { get; set; }
		public int NewHeight { get; set; }
	}
}

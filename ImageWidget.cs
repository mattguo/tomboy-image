using System;
using System.Collections;
using Gtk;
using Gdk;
namespace Tomboy.InsertImage
{
	public class ImageWidget : EventBox
	{
		Gtk.Image child;
		int width, height;
		int difX, difY;
		bool resizingX;
		bool resizingY;
		int oldChildWidth;
		int oldChildHeight;
		Pixbuf originalPixbuf = null;

		Cursor cursorX = new Cursor (CursorType.RightSide);
		Cursor cursorY = new Cursor (CursorType.BottomSide);
		Cursor cursorXY = new Cursor (CursorType.BottomRightCorner);
		Cursor cursorNormal = new Cursor (CursorType.Arrow);

		Gdk.Size imageSize;

		const int selectionBorder = 8;

		internal bool AllowResize = true;

		public ImageWidget (Pixbuf pixbuf)
		{
			resizingX = resizingY = false;
			this.CanFocus = true;
			this.Events = EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.PointerMotionMask;
			this.child = new Gtk.Image();
			this.child.Pixbuf = pixbuf;
			originalPixbuf = pixbuf;
			Add (child);
			imageSize = child.Allocation.Size;
		}

		public Gdk.Size ImageSize
		{
			get
			{
				return imageSize;
			}
		}

		public event EventHandler<ResizeEventArgs> Resized;

		public void ResumeImageSize ()
		{
			child.Pixbuf = originalPixbuf;
			child.SetSizeRequest (originalPixbuf.Width, originalPixbuf.Height);
			imageSize.Width = originalPixbuf.Width;
			imageSize.Height = originalPixbuf.Height;
		}

		public void ResizeImage (int width, int height)
		{
			if (width <= 0 || height <= 0)
				return;
			if (width == originalPixbuf.Width && height == originalPixbuf.Height)
				ResumeImageSize ();
			else
				ResizeImage (width, height, InterpType.Bilinear);
		}

		public void ResizeImage (int width, int height, InterpType interType)
		{
			child.Pixbuf = originalPixbuf.ScaleSimple (width, height, interType);
			child.SetSizeRequest (width, height);
			imageSize.Width = width;
			imageSize.Height = height;
			//QueueDraw ();
		}

		private void OnResized (int oldWidth, int oldHeight, int newWidth, int newHeight)
		{
			if (Resized != null) {
				ResizeEventArgs arg = new ResizeEventArgs (oldWidth, oldHeight, newWidth, newHeight);
				Resized (this, arg);
			}
		}

		protected override void OnDestroyed ()
		{
			if (cursorX != null) {
				cursorX.Dispose ();
				cursorXY.Dispose ();
				cursorY.Dispose ();
				cursorX = cursorXY = cursorY = null;
			}
			base.OnDestroyed ();
		}

		protected override bool OnMotionNotifyEvent (Gdk.EventMotion ev)
		{
			if (resizingX || resizingY) {
				int minWidth = selectionBorder * 2;
				int minHeight = selectionBorder * 2;
				int newWidth, newHeight;
				if (resizingX) {
					newWidth = (int) (ev.X + difX);
					if (newWidth < minWidth)
						newWidth = minWidth;
					
				} else newWidth = child.Allocation.Width;

				if (resizingY) {
					newHeight = (int) (ev.Y + difY);
					if (newHeight < minHeight)
						newHeight = minHeight;
					child.HeightRequest = newHeight;
				} else newHeight = child.Allocation.Height;

				ResizeImage (newWidth, newHeight, InterpType.Nearest);
			} else if (AllowResize) {
				if (GetAreaResizeXY ().Contains ((int) ev.X, (int) ev.Y))
					GdkWindow.Cursor = cursorXY;
				else if (GetAreaResizeX ().Contains ((int) ev.X, (int) ev.Y))
					GdkWindow.Cursor = cursorX;
				else if (GetAreaResizeY ().Contains ((int) ev.X, (int) ev.Y))
					GdkWindow.Cursor = cursorY;
				else
					GdkWindow.Cursor = cursorNormal;
			}

			return base.OnMotionNotifyEvent (ev);
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton ev)
		{
			Gdk.Rectangle rectArea = child.Allocation;

			if (AllowResize) {
				if (ev.Button == 1) {
					// Left button click
					Rectangle rect = GetAreaResizeXY ();
					if (rect.Contains ((int)ev.X, (int)ev.Y)) {
						resizingX = resizingY = true;
						difX = (int)(rectArea.Width - ev.X);
						difY = (int)(rectArea.Height - ev.Y);
						GdkWindow.Cursor = cursorXY;
					} else if (GetAreaResizeY ().Contains ((int)ev.X, (int)ev.Y)) {
						resizingY = true;
						difY = (int)(rectArea.Height - ev.Y);
						width = rectArea.Width;
						GdkWindow.Cursor = cursorY;
					} else if (GetAreaResizeX ().Contains ((int)ev.X, (int)ev.Y)) {
						resizingX = true;
						difX = (int)(rectArea.Width - ev.X);
						height = rectArea.Height;
						GdkWindow.Cursor = cursorX;
					}
					if (resizingX || resizingY) {
						oldChildWidth = child.Allocation.Width;
						oldChildHeight = child.Allocation.Height;
					}
				}
			}


			return base.OnButtonPressEvent (ev);
		}

		Rectangle GetAreaResizeY ()
		{
			Gdk.Rectangle rect = child.Allocation;
			return new Gdk.Rectangle (rect.X, rect.Y + rect.Height - selectionBorder, rect.Width - selectionBorder, selectionBorder);
		}

		Rectangle GetAreaResizeX ()
		{
			Gdk.Rectangle rect = child.Allocation;
			return new Gdk.Rectangle (rect.X + rect.Width - selectionBorder, rect.Y, selectionBorder, rect.Height - selectionBorder);
		}

		Rectangle GetAreaResizeXY ()
		{
			Gdk.Rectangle rect = child.Allocation;
			return new Gdk.Rectangle (rect.X + rect.Width - selectionBorder, rect.Y + rect.Height - selectionBorder, selectionBorder, selectionBorder);
		}

		protected override bool OnButtonReleaseEvent (Gdk.EventButton ev)
		{
			if (AllowResize) {
				if (ev.Button == 1 && (resizingX || resizingY)) {
					resizingX = resizingY = false;
					GdkWindow.Cursor = cursorNormal;
					int newWidth = child.Allocation.Width;
					int newHeight = child.Allocation.Height;
					ResizeImage (newWidth, newHeight);
					OnResized (oldChildWidth, oldChildHeight, newWidth, newHeight);
				} else if (ev.Button != 1) {
					int oldWidth = child.Allocation.Width;
					int oldHeight = child.Allocation.Height;
					ResumeImageSize ();
					OnResized (oldWidth, oldHeight, originalPixbuf.Width, originalPixbuf.Height);
				}
			}
			return base.OnButtonReleaseEvent (ev);
		}
	}
}

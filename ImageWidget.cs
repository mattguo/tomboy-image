using System;
using System.Collections;
using Gtk;
using Gdk;
using Mono.Unix;

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

		Gtk.Menu contextMenu = null;

		Gdk.Size imageSize;

		internal bool AllowResize = true;

		public const int SelectionBorder = 8;
		public const int MinWidth = 12;
		public const int MinHeight = 12;


		public ImageWidget (Pixbuf pixbuf)
		{
			resizingX = resizingY = false;
			this.CanFocus = true;
			this.Events = EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.PointerMotionMask;
			this.child = new Gtk.Image ();
			this.child.Pixbuf = pixbuf;
			originalPixbuf = pixbuf;
			Add (child);
			imageSize = child.Allocation.Size;
		}

		public Gtk.Menu ContextMenu
		{
			get
			{
				if (contextMenu == null) {
					contextMenu = new Menu ();
					var accel_group = new Gtk.AccelGroup ();
					contextMenu.AccelGroup = accel_group;

					Gtk.ImageMenuItem resumeSize = new Gtk.ImageMenuItem (
						Catalog.GetString ("Resume Size"));
					resumeSize.Image = new Gtk.Image (Gtk.Stock.Zoom100, Gtk.IconSize.Menu);
					resumeSize.Activated += (o, e) => ResumeImageSize ();
					//resumeSize.AddAccelerator ("activate",
					//                       accel_group,
					//                       (uint) Gdk.Key.r,
					//                       Gdk.ModifierType.ControlMask,
					//                       Gtk.AccelFlags.Visible);
					contextMenu.Append (resumeSize);
				}
				return contextMenu;
			}
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
			int oldWidth = child.Allocation.Width;
			int oldHeight = child.Allocation.Height;
			ResizeImage (originalPixbuf.Width, originalPixbuf.Height);
			OnResized (oldWidth, oldHeight, originalPixbuf.Width, originalPixbuf.Height);
		}

		public void ResizeImage (int width, int height)
		{
			ResizeImage (width, height, InterpType.Bilinear);
		}

		public void ResizeImage (int width, int height, InterpType interType)
		{
			if (width <= 0 || height <= 0)
				return;
			if (width < MinWidth)
				width = MinWidth;
			if (height < MinHeight)
				height = MinHeight;
			if (width == originalPixbuf.Width && height == originalPixbuf.Height)
				child.Pixbuf = originalPixbuf;
			else
				child.Pixbuf = originalPixbuf.ScaleSimple (width, height, interType);
			child.SetSizeRequest (width, height);
			imageSize.Width = width;
			imageSize.Height = height;
			//QueueDraw ();
		}

		private void OnResized (int oldWidth, int oldHeight, int newWidth, int newHeight)
		{
			if (Resized != null && ((oldWidth != newWidth) || (oldHeight != newHeight))) {
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
				int newWidth = resizingX ? (int)(ev.X + difX) : child.Allocation.Width;
				int newHeight = resizingY ? (int)(ev.Y + difY) : child.Allocation.Height;
				ResizeImage (newWidth, newHeight, InterpType.Nearest);
			} else if (AllowResize) {
				if (GetAreaResizeXY ().Contains ((int)ev.X, (int)ev.Y))
					GdkWindow.Cursor = cursorXY;
				else if (GetAreaResizeX ().Contains ((int)ev.X, (int)ev.Y))
					GdkWindow.Cursor = cursorX;
				else if (GetAreaResizeY ().Contains ((int)ev.X, (int)ev.Y))
					GdkWindow.Cursor = cursorY;
				else
					GdkWindow.Cursor = cursorNormal;
			}

			return base.OnMotionNotifyEvent (ev);
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton ev)
		{
			Gdk.Rectangle rectArea = child.Allocation;
			if (ev.Button == 1) {
				if (AllowResize) {
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
			return new Gdk.Rectangle (rect.X, rect.Y + rect.Height - SelectionBorder, rect.Width - SelectionBorder, SelectionBorder);
		}

		Rectangle GetAreaResizeX ()
		{
			Gdk.Rectangle rect = child.Allocation;
			return new Gdk.Rectangle (rect.X + rect.Width - SelectionBorder, rect.Y, SelectionBorder, rect.Height - SelectionBorder);
		}

		Rectangle GetAreaResizeXY ()
		{
			Gdk.Rectangle rect = child.Allocation;
			return new Gdk.Rectangle (rect.X + rect.Width - SelectionBorder, rect.Y + rect.Height - SelectionBorder, SelectionBorder, SelectionBorder);
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
				} else if (ev.Button == 3) {
					ContextMenu.Popup ();
				}
				//} else if (ev.Button != 1) {
				//    int oldWidth = child.Allocation.Width;
				//    int oldHeight = child.Allocation.Height;
				//    ResumeImageSize ();
				//    OnResized (oldWidth, oldHeight, originalPixbuf.Width, originalPixbuf.Height);
				//}
			}
			return base.OnButtonReleaseEvent (ev);
		}
	}
}

using System;
using Mono.Unix;

namespace Tomboy.InsertImage
{
	// This class is copied form the UIA-Explorer project:
	// git://gitorious.org/uia-explorer/uia-explorer.git
	public static class Message
	{
		public static void Error (string format, params object [] args)
		{
			RunModalDialog ("Error", format, args);
		}

		public static void Warn (string format, params object [] args)
		{
			RunModalDialog ("Warning", format, args);
		}

		public static void Info (string format, params object [] args)
		{
			RunModalDialog ("Info", format, args);
		}

		private static void RunModalDialog (string title, string format, params object [] args)
		{
			Gtk.Dialog dlg = new Gtk.Dialog ("Tomboy.InsertImage - " + Catalog.GetString(title), null,
											 Gtk.DialogFlags.Modal | Gtk.DialogFlags.DestroyWithParent);
			var text = new Gtk.TextView ();
			text.WrapMode = Gtk.WrapMode.Word;
			text.Editable = false;
			if (args.Length > 0)
				format = string.Format (format, args);
			text.Buffer.Text = format;
			var scroll = new Gtk.ScrolledWindow ();
			scroll.Add (text);
			dlg.AddButton (Catalog.GetString("Close"), Gtk.ResponseType.Close);
			dlg.VBox.PackStart (scroll, true, true, 0);
			dlg.SetSizeRequest (300, 240);
			scroll.ShowAll ();
			dlg.Run ();
			dlg.Destroy ();
		}
	}
}

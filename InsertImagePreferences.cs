//
// InsertTimestampPreferences.cs: Preferences dialog for InsertTimestamp addin.
// Allows configuration of timestamp format.
//
using System;
using System.Collections.Generic;
using Mono.Unix;
using Tomboy;
using Gtk;

namespace Tomboy.InsertImage
{
	public class InsertImagePreferences
	{
		public const string HelpNoteAddedKey = "/apps/tomboy/insert_image/help_note_added";
		public const string WarnQualityLossKey = "/apps/tomboy/insert_image/warn_quality_loss";
		public const string AutoCompressOnClosedKey = "/apps/tomboy/insert_image/auto_compress_on_closed";

		public static bool HelpNoteAdded
		{
			get
			{
				object obj = Preferences.Get (HelpNoteAddedKey);
				return obj != null ? (bool)obj : false;
			}
			set { Preferences.Set (HelpNoteAddedKey, value); }
		}

		public static bool WarnQualityLoss
		{
			get
			{
				object obj = Preferences.Get (WarnQualityLossKey);
				return obj != null ? (bool)obj : true;
			}
			set { Preferences.Set (WarnQualityLossKey, value); }
		}

		public static bool AutoCompressOnClosed
		{
			get
			{
				object obj = Preferences.Get (AutoCompressOnClosedKey);
				return obj != null ? (bool)obj : false;
			}
			set { Preferences.Set (AutoCompressOnClosedKey, value); }
		}
	}

	public class InsertImagePreferencesWidget : Gtk.VBox {

		public InsertImagePreferencesWidget ()
			: base (false, 12)
		{
			var btnWarnQualityLoss = new CheckButton (Catalog.GetString ("Warn user the effect of quality loss when compressing images"));
			btnWarnQualityLoss.Active = InsertImagePreferences.WarnQualityLoss;
			btnWarnQualityLoss.Toggled += (o, e) => InsertImagePreferences.WarnQualityLoss = btnWarnQualityLoss.Active;
			PackStart (btnWarnQualityLoss);
			var btnAutoCompressOnClosed = new CheckButton (Catalog.GetString ("Auto compress images when window is closed"));
			btnAutoCompressOnClosed.Active = InsertImagePreferences.AutoCompressOnClosed;
			btnAutoCompressOnClosed.Toggled += (o, e) => InsertImagePreferences.AutoCompressOnClosed = btnAutoCompressOnClosed.Active;
			PackStart (btnAutoCompressOnClosed);
			ShowAll ();
		}
	}
}

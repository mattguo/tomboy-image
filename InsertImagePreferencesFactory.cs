//
// InsertTimestampPreferencesFactory.cs: Creates a widget that will be used in
// the addin's preferences dialog.
//

using System;

using Tomboy;

namespace Tomboy.InsertImage
{
	public class InsertImagePreferencesFactory : AddinPreferenceFactory
	{
		public override Gtk.Widget CreatePreferenceWidget ()
		{
			return new InsertImagePreferencesWidget ();
		}
	}
}

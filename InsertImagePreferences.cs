//
// InsertTimestampPreferences.cs: Preferences dialog for InsertTimestamp addin.
// Allows configuration of timestamp format.
//

using System;
using System.Collections.Generic;

using Mono.Unix;

using Tomboy;

namespace Tomboy.InsertImage
{
	public class InsertImagePreferences : Gtk.VBox {

		public InsertImagePreferences ()
			: base (false, 12)
		{	
			Gtk.Label label = new Gtk.Label ("Under construction... :(");
			PackEnd (label);
			ShowAll ();
		}
	}
}

using System;
using System.Collections.Generic;
using Tomboy;

namespace Tomboy.InsertImage
{
	public class ImageTag : NoteTag
	{
		public ImageTag ()
			: base ("image")
		{
			CanSpellCheck = false;
			this.PaletteForeground = ContrastPaletteColor.Grey;
			this.Invisible = true;
		}
	}
}

using System;
using System.Collections.Generic;
using Tomboy;

namespace Tomboy.InsertImage
{
	public class ImageBoxTag : DynamicNoteTag
	{
		public ImageBoxTag ()
			: base ()
		{
		}
		
		public override void Initialize (string element_name)
		{
			base.Initialize (element_name);
			this.CanSerialize = false;
			this.CanSpellCheck = false;
			//this.CanGrow = false;
			//this.CanSplit = false;
			//this.Editable = false;
		}
		
		public ImageInfo ImageInfo { get; set; }
	}
}

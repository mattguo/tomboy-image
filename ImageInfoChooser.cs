using System;
using System.Collections.Generic;
using Gtk;
using Mono.Unix;

namespace Tomboy.InsertImage
{
	public interface IImageInfoChooser {
		ImageInfo ChooseImageInfo (Window parent);
	}

	public class LocalImageChooser : IImageInfoChooser
	{
		#region Singleton

		private static LocalImageChooser instance = new LocalImageChooser ();
		public static LocalImageChooser Instance { get { return instance; } }

		private LocalImageChooser ()
		{
		}

		#endregion

		private FileChooserDialog fc = null;
		private RadioButton embedOption = null;


		#region IImageInfoChooser Members

		public ImageInfo ChooseImageInfo (Window parent)
		{
			if (fc == null)
				InitFileChooserDialog ();
			fc.Reparent (parent);

			ImageInfo info = null;

			var ret = fc.Run ();
			fc.Hide ();
			if (ret == (int)ResponseType.Accept) {
				string imagePath = fc.Filename;
				info = ImageInfo.FromLocalFile (imagePath, !embedOption.Active);
			}

			return info;
		}

		#endregion

		private void InitFileChooserDialog ()
		{
			if (fc != null)
				fc.Destroy ();
			fc = new FileChooserDialog (
				Catalog.GetString ("Choose the file to open"), null,
				FileChooserAction.Open,
				Catalog.GetString ("Cancel"), ResponseType.Cancel,
				Catalog.GetString ("Open"), ResponseType.Accept);

			FileFilter filter = new FileFilter ();
			filter.Name = Catalog.GetString ("All Images");
			filter.AddPattern ("*.[pP][nN][gG]");
			filter.AddPattern ("*.[jJ][pP][gG]");
			filter.AddPattern ("*.[jJ][pP][eE][gG]");
			filter.AddPattern ("*.[jJ][pP][eE]");
			filter.AddPattern ("*.[jJ][fF][iI][fF]");
			filter.AddPattern ("*.[gG][iI][fF]");
			filter.AddPattern ("*.[bB][mM][pP]");
			filter.AddPattern ("*.[tT][iI][fF][fF]");
			filter.AddPattern ("*.[tT][iI][fF]");
			fc.AddFilter (filter);

			FileFilter pngFilter = new FileFilter ();
			pngFilter.Name = "PNG (*.PNG)";
			pngFilter.AddPattern ("*.[pP][nN][gG]");
			fc.AddFilter (pngFilter);

			FileFilter jpegFilter = new FileFilter ();
			jpegFilter.Name = "JPEG (*.JPG;*.JPEG;*.JPE;*.JFIF)";
			jpegFilter.AddPattern ("*.[jJ][pP][gG]");
			jpegFilter.AddPattern ("*.[jJ][pP][eE][gG]");
			jpegFilter.AddPattern ("*.[jJ][pP][eE]");
			jpegFilter.AddPattern ("*.[jJ][fF][iI][fF]");
			fc.AddFilter (jpegFilter);

			FileFilter gifFilter = new FileFilter ();
			gifFilter.Name = "GIF (*.GIF)";
			gifFilter.AddPattern ("*.[gG][iI][fF]");
			fc.AddFilter (gifFilter);

			FileFilter bmpFilter = new FileFilter ();
			bmpFilter.Name = "BMP (*.BMP)";
			bmpFilter.AddPattern ("*.[bB][mM][pP]");
			fc.AddFilter (bmpFilter);

			FileFilter tiffFilter = new FileFilter ();
			tiffFilter.Name = "TIFF (*.TIFF;*.TIF)";
			tiffFilter.AddPattern ("*.[tT][iI][fF][fF]");
			tiffFilter.AddPattern ("*.[tT][iI][fF]");
			fc.AddFilter (tiffFilter);

			FileFilter all = new FileFilter ();
			all.Name = Catalog.GetString ("All Files");
			all.AddPattern ("*");
			fc.AddFilter (all);

			var align = new Alignment (1.0f, 0.5f, 0f, 0f);
			align.RightPadding = 15;
			fc.VBox.PackStart (align, false, true, 5);

			var radioBox = new VBox ();
			embedOption = new RadioButton (null, Catalog.GetString ("Embed the content of the selected image"));
			radioBox.PackStart (embedOption, false, false, 0);

			RadioButton linkOption = new RadioButton (embedOption, Catalog.GetString ("Insert a link to the selected image"));
			radioBox.PackStart (linkOption, false, false, 0);

			embedOption.Active = true;

			align.Add (radioBox);

			fc.VBox.ShowAll ();
		}
	}

	public class WebImageChooser : IImageInfoChooser
	{
		#region Singleton

		private static WebImageChooser instance = new WebImageChooser ();
		public static WebImageChooser Instance { get { return instance; } }
		
		private WebImageChooser ()
		{
		}

		#endregion

		private Dialog dlg = null;
		private Entry txtAddress = null;
		private RadioButton embedOption = null;

		#region IImageInfoChooser Members

		public ImageInfo ChooseImageInfo (Window parent)
		{
			if (dlg == null)
				InitDialog ();
			dlg.Reparent (parent);

			ImageInfo info = null;

			var ret = dlg.Run ();
			dlg.Hide ();
			if (ret == (int)ResponseType.Accept) {
				string imageAddress = txtAddress.Text;
				if (!imageAddress.Contains ("://"))
					imageAddress = "http://" + imageAddress;
				info = ImageInfo.FromWebFile (imageAddress, !embedOption.Active);
			}

			return info;
		}

		#endregion

		private void InitDialog ()
		{
			if (dlg != null)
				dlg.Destroy ();
			dlg = new Dialog ();

			var align = new Alignment (0.0f, 0.5f, 0f, 0f);
			dlg.VBox.PackStart (align, false, true, 5);
			var label = new Label (Catalog.GetString ("Image URL:"));
			align.Add (label);

			txtAddress = new Entry ();
			dlg.VBox.PackStart (txtAddress, false, true, 0);

			align = new Alignment (1.0f, 0.5f, 0f, 0f);
			align.RightPadding = 15;
			dlg.VBox.PackStart (align, false, true, 5);

			var radioBox = new VBox ();
			embedOption = new RadioButton (null, Catalog.GetString ("Embed the content of the selected image"));
			radioBox.PackStart (embedOption, false, false, 0);
			RadioButton linkOption = new RadioButton (embedOption, Catalog.GetString ("Insert a link to the selected image"));
			radioBox.PackStart (linkOption, false, false, 0);
			embedOption.Active = true;
			align.Add (radioBox);

			dlg.AddButton (Catalog.GetString ("Cancel"), Gtk.ResponseType.Cancel);
			dlg.AddButton (Catalog.GetString ("Open"), Gtk.ResponseType.Accept);
			dlg.WidthRequest = 360;
			dlg.VBox.ShowAll ();
		}
	}
}

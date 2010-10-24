using System;
using System.IO;
using Gtk;
using Mono.Unix;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

namespace Tomboy.InsertImage
{
	public class ImageInfo
	{
		public static ImageInfo FromLocalFile (string path, bool useExternalLink)
		{
			ImageInfo info = new ImageInfo ();
			info.FilePath = path;
			info.UseExternalLink = useExternalLink;
			info.IsLocalFile = true;
			info.FileContent = File.ReadAllBytes (path);
			return info;
		}

		public static ImageInfo FromWebFile (string address, bool useExternalLink)
		{
			ImageInfo info = new ImageInfo ();
			info.FilePath = address;
			info.UseExternalLink = useExternalLink;
			info.IsLocalFile = false;
			info.LoadFromWeb (address);
			return info;
		}

		internal static ImageInfo FromSavedString (string savedInfo, bool ignoreContentError)
		{
			var fields = savedInfo.Split (new char [] { ',' });
			if (fields.Length != 6)
				throw new FormatException (Catalog.GetString("Invalid ImageInfo Format."));
			ImageInfo info = new ImageInfo ();
			try {
				info.DisplayWidth = int.Parse (fields [0]);
				info.DisplayHeight = int.Parse (fields [1]);
				info.UseExternalLink = bool.Parse (fields [2]);
				info.IsLocalFile = bool.Parse (fields [3]);
				byte [] imagePathBytes = Convert.FromBase64String (fields [4]);
				info.FilePath = Encoding.UTF8.GetString (imagePathBytes);
				if (!info.UseExternalLink)
					info.FileContent = Convert.FromBase64String (fields [5]);
				else if (info.IsLocalFile) {
					if (!File.Exists (info.FilePath))
						throw new FileNotFoundException ();
					info.FileContent = File.ReadAllBytes (info.FilePath);
				} else
					info.LoadFromWeb (info.FilePath);
			} catch (FileNotFoundException) {
				if (ignoreContentError)
					SetMissingImage (info);
				else
					throw;
			} catch (IOException) {
				if (ignoreContentError)
					SetMissingImage (info);
				else
					throw;
			} catch (WebException) {
				if (ignoreContentError)
					SetMissingImage (info);
				else
					throw;
			} catch {
				throw new FormatException (Catalog.GetString ("Invalid ImageInfo Format."));
			}
			return info;
		}

		private static void SetMissingImage (ImageInfo info)
		{
			var imgStream =
				typeof (ImageInfo).Assembly.GetManifestResourceStream ("Tomboy.InsertImage.missing-image.png");
			byte[] bytes = new Byte[imgStream.Length];
			imgStream.Read(bytes, 0, (int)imgStream.Length);
			info.FileContent = bytes;
			imgStream.Close ();
		}

		private void LoadFromWeb (string address)
		{
			WebClient wc = new WebClient ();
			FileContent = wc.DownloadData (address);
		}

		public ImageInfo ()
		{
		}

		public string FilePath { get; set; }
		public byte [] FileContent { get; set; }

		public bool UseExternalLink { get; set; }
		public bool IsLocalFile { get; set; }

		public int DisplayWidth { get; set; }
		public int DisplayHeight { get; set; }

		public TextMark Mark { get; set; }

		public ImageWidget Widget { get; set; }

		public int Position
		{
			get
			{
				return Mark.Buffer.GetIterAtMark (Mark).Offset;
			}
		}

		public string SaveAsString ()
		{
			byte[] filePathBytes = Encoding.UTF8.GetBytes (FilePath);
			return string.Format ("{0},{1},{2},{3},{4},{5}",
				DisplayWidth, DisplayHeight,
				UseExternalLink, IsLocalFile,
				Convert.ToBase64String (filePathBytes),
				UseExternalLink ? "" : Convert.ToBase64String (FileContent));
		}
	}

	public class ImageInfoComparerByPosition : IComparer<ImageInfo>
	{
		public int Compare (ImageInfo x, ImageInfo y)
		{
			return x.Position - y.Position;
		}
	}
}
![screen shot](http://mattguo.com/tomboy-image/shot1.png)
#Features
* Insert image
	* Import image content, or
	* Just link external image
* Delete image
* Resize image
* "Save as..." stored image
* Undo/Redo image insertion/deletion/resizing

#Installation
Download InsertImage.dll from [here](http://mattguo.com/tomboy-image/InsertImage.dll),
Then copy the dll to Tomboy's addins directory.

Q: Where is Tomboy's addins directory?
A: [http://live.gnome.org/Tomboy/Directories](http://live.gnome.org/Tomboy/Directories)

* On Linux
	    mv InsertImage.dll ~/.config/tomboy/addins
* On Windows
	    move InsertImage.dll "%APPDATA%\Tomboy\config\addins"
* On Mac
	    mv InsertImage.dll ~/Library/Preferences/Tomboy/addins

Then restart Tomboy.

#Build Source Code
git clone this repo and build it with Visual Studio 2008 or MonoDevelop 2.X.

#Known Issues
* Can't sync embedded images to Web store


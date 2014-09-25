## Features
* Insert image
	* Import image content, or
	* Just link external image
* Delete image
* Resize image
* "Save as..." stored image
* Undo/Redo image insertion/deletion/resizing

## Installation
Download InsertImage.dll from the Releases folder.

* Close the Tomboy application if it is running.
* Copy the dll to Tomboy's addins directory.
    If the folder does not exist, create it in the appropriate location as stipulated below.

## Tomboy Addin Directory Locations

A: [http://live.gnome.org/Tomboy/Directories](http://live.gnome.org/Tomboy/Directories)

* Linux
	    mv InsertImage.dll ~/.config/tomboy/addins
* Windows
	    move InsertImage.dll "%APPDATA%\Tomboy\config\addins"
* OSX
	    mv InsertImage.dll ~/Library/Preferences/Tomboy/addins

## How to Use
I'm only familiar with how to use this on OSX but I imagine its similar for Windows and Linux.

* Open Tomboy and begin a new note
* Open the Right-Click contextual menu
* Select `Capture Selection From Screen`
* You will then be prompted to select an area of the screen, similar to if you were taking a screenshot.  The screen area you select will be inserted into your note as an image.
* Select the new menu option `Import Image` which should now exist
* This will open up a new window requesting you to select an external device to import an image from (e.g. camera, scanner)

Note: You can not copy, drag/drop, or insert image files.  Inserting via this screenshot method or from an external device are the only ways to add an image but the screenshot method is really all I need so I'm satisfied.

#Known Issues
* Can't sync embedded images to Web store


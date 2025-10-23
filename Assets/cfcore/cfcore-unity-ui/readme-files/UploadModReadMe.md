
# cfcore UploadMod

## Initializing

In order to to define what the popup should do and initialize it with the correct data we should 
send "UploadModOptions" to the "Initialize" method.
More info about what each parameter means you can find in "UploadModOptions.cs"

If the mod is intended to be updated , it's info will be the default values in the popup.
The thumbnail can be set via options or by taking a screenshot through the popup.


## Uploading a new file

In order to upload a new file please make sure that:

-Your mod is acrchived to a *.zip file
-The archive is not empty
-The archive is not the same as any archive that was uploaded beforehand.


## Events

This popup expose the following events :

- ModCreated / ModUpdated 
   This event invoked with the modID whenever the popup finished creating/updating the mod.
   Saving the mod ID after the mod created is important for easier finding if the user need to modify
   the mod later.

- FileProgress
   This event invoked while a file is uploaded . It reflects the progress of the uploaded file.
   This event might be helpfull if there is a need to visualize the progress.

- FileUploaded
   This event invoked when the file was uploaded succesfully.

- ModCreationFailed / FileUploadFailed
   These events called when there was an error with uploading the data for the mod and uploading a file for the mod.
   These events are for error handling.


## Input Fields

All the fields must be filled in order to se

- Mod name
  This is the name of the mod as it will be shown while browsing.

- Mod Summary
  A short summary of the mod.

- Thumbnail
  In order to upload a new mod there is a need to add thumbnail.
  This can be done by sending a texture in options or by pressing "Take Screenshot" button.
  There is a default implementation of taking screenshot, but if needed it can be easily replaced by implementing the interface

- Class / Category 
  These dropdowns will determine the class and the main category of the mod.


## More info

You can find another README files in :
.../Assets/cfcore : A more thorough explanation about how to setup your game using CFCore 

For more info about the API please visit https://core.curseforge.com/
For more developer actions and moderation please visit https://console.curseforge.com/#/
You can also create and browse mods from our main site https://www.curseforge.com/YOURGAMENAME/   


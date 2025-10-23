# Eternal Color Theme

This README is for game developers who use the CurseForge UI provided with the
Eternal SDK for Unity, and wish to know how to customize its colors.

Below you can read how to create and use a Color Theme in order to customize the
provided UI.

## Setup

The Color Theme is defined by an `EternalThemeColorObject` asset.

You have two ways to create an `EternalThemeColorObject` asset:

1. Open the Assets menu and go to `'Create' -> 'Eternal' -> 'Theme Color'`.
2. Right click in the Project window and go to
   `'Create' -> 'Eternal' -> 'Theme Color'`.

Go to the `Settings` asset (`Edit Setting` under `Eternal` menu), and drag the
`ThemeColorObject` asset you created to the `Color Theme` property.

For more information about the `Settings` asset, please read
[SettingsReadMe.md](./SettingsReadMe.md).

## Usage

To change an element's color, change the color of the corresponding property of
the `ThemeColorObject` asset.
Once you run the game in the editor you will see the updated colors of the UI
elements.

Once you are satisfied with the changes, you can apply them by pressing the
`Overwrite UI with theme settings` button in the `Settings` asset which will
apply the colors to the UI elements' assets.

_Note: Pressing "Apply" will clear the 'Color Theme' field in the settings
and make it ready for new changes._

_Note: You can reset all the colors to the default theme by pressing
the 'Reset UI Colors' button in the settings. You can find and edit the default
themes in the folder:_ `Assets/cfcore-unity-ui/UI/Theme/Default Themes/`

## Applying theme to custom elements

If you want to color custom UI elements you created using the colors theme, do
the following:

1. Add the `EternalColorSetter` script to the UI GameObject which contains a
   sprite or a font.
2. Add to the array `EternalUIColorables` one or two elements (depends if you
   have a sprite, font or both)
3. Choose what type of coloration this element will have.

_Note: You can see the explanation of each of colorable, in the tooltip while
hovering over it in the Theme Color scriptable object (or in
`EternalThemeColor.cs`)_

## More info

You can find more info about settings in [SettingsReadMe.md](./SettingsReadMe.md).

You can find more README files [here](../readme-files).

For more info about the API please visit [Eternal](https://eternal.overwolf.com/).

For more developer actions and moderation please visit the [Eternal Console](https://console.curseforge.com/#/).

And, of course, you can also create and browse mods on [CurseForge](https//www.curseforge.com).

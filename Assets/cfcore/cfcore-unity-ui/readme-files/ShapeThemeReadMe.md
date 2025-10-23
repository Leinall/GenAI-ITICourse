# Eternal Shape Theme

This README is for game developers who use the CurseForge UI provided with the
Eternal SDK for Unity, and wish to know how to customize its shapes.

Below you can read how to create and use a Shape Theme in order to customize the
provided UI.

## Setup

The Shape Theme is defined by an `EternalThemeShapeObject` asset.

You have two ways to create an `EternalThemeShapeObject` asset:

1. Open the Assets menu and go to `'Create' -> 'Eternal' -> 'Theme Shape'`.
2. Right click in the Project window and go to
   `'Create' -> 'Eternal' -> 'Theme Shape'`.

Once you have the `ShapeTheme` asset, drag it to the `Shape Theme` field on the
settings asset.

For more information about the settings asset, please read
[SettingsReadMe.md](./SettingsReadMe.md).

## Usage

In order to alter an element, drag a sprite to the corresponding field on the
`ThemeShape` asset you created. Once you run the app you can see the updated
sprites of the UI elements.

Once you are satisfied with the changes, you can apply them by pressing the
`Overwrite UI with theme settings` button in the settings asset, which will
apply the sprites to the UI elements to all the Prefabs.

## Extending the UI

If you want to extend the UI, and change the shape of your elements according to
the theme:

1. Add `EternalShapeSetter` script to the UI GameObject which contains a sprite.
2. Choose the type of the shape this element is.

> You can see the explanation of each of shape in the tooltip while
> hovering over it in the Theme Shape scriptable object (or in
> `EternalThemeColor.cs`)

> Pressing Apply, will empty the `Shape Theme` field in the settings and make it
> ready for new changes.

> You can reset all the sprites to the default theme by pressing the `Reset UI
Shapes` button in the settings. You can find and edit the default themes in
> the folder `.../UI/Theme/Default Themes/`.

## More info

You can find more info about settings in [SettingsReadMe.md](./SettingsReadMe.md).

You can find more README files [here](../readme-files).

For more info about the API please visit [Eternal](https://eternal.overwolf.com/).

For more developer actions and moderation please visit the [Eternal Console](https://console.curseforge.com/#/).

And, of course, you can also create and browse mods on [CurseForge](https//www.curseforge.com).

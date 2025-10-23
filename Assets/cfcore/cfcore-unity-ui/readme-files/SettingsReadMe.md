# Eternal Settings

This README is for game developers who use the Eternal SDK for Unity and wish to
learn how to configure the Eternal UI using the `Settings` asset.

## Setup

You can generate / edit a settings asset by choosing the menu item `Eternal` ->
`Edit Settings`.

If the settings asset doesn't exist, it will be created in the `Resources` folder
and will be focused on.

If the settings asset does exist, it will be focused on.

## Usage

You can change the setting asset default path by changing the static string
`FilePath`.

Once the setting asset exists, the settings will be automaticaly loaded in
`READ_APIManager.cs` on startup. The settings of the game will be overriden by
the settings file.

## Contents

### Static fields

| Field      | Description                                               |
| ---------- | --------------------------------------------------------- |
| `FilePath` | Path to the settings file relative to 'Resources' folder. |

### Public fields

| Field                 | Description                                                                                              |
| --------------------- | -------------------------------------------------------------------------------------------------------- |
| `Game Id`             | Your Eternal Game ID.                                                                                    |
| `API Key`             | Your Eternal API key.                                                                                    |
| 'Hide SignIn'         | If checked, the sign in through the UI will be turned off                                                |
| 'Hide MyCreation'     | If checked, the my creation section in sidebar will be be hidden                                         |
| `Eula Language`       | The language in which the EULA will be displayed.                                                        |
| `Directory Mode`      | The directory structure of the mods.                                                                     |
| `Use Mod Description` | If checked, mod descriptions will be shown on mod pages. <br> Else, summaries will be displayed instead. |
| `Color Theme`         | Scriptable object that controls the colors of the UI elements. [More info](./ColorThemeReadMe.md)        |
| `Shape Theme`         | Scriptable object that controls the shape of the UI elements. [More info](./ShapeThemeReadMe.md)         |
| `Scale`               | Determines if the Eternal UI will be displayed in full-screen or not.                                    |
| `Theme Controller`    | Controls for applying / resetting themes.                                                                |

> You can find your `Game Id` and `API Key` on the Status page of your game on
> the [Eternal Console](https://console.curseforge.com).

> `Directory Mode` has two options:
>
> - `CF Core Structure`: Default directory structure, i.e.
>   `../ModsFolder/GameID/ModID_FileID/..`
> - `Single`: Directory structure without the Game ID, i.e.
>   `../ModsFolder/ModID_FileID/..`

## More info

You can find more about themes in the theme READMEs
'ColorThemeReadme.md' and 'ShapeThemeReadme.md'

You can find more README files in :
.../Assets/cfcore/cfcore-unity-ui/readme-files/

You can find more README files [here](../readme-files).

For more info about the API please visit [Eternal](https://eternal.overwolf.com/).

For more developer actions and moderation please visit the [Eternal Console](https://console.curseforge.com/#/).

And, of course, you can also create and browse mods on [CurseForge](https//www.curseforge.com).

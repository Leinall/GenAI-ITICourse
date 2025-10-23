# cfcore-unity-ui

This readme file will try to explain how to get started with adding our UI
component into your game.

## Sample Scene

The sample scene is a demo that includes everything you need to browse and
download the mods for a game.

## Setup your game

- Add the ApiManager prefab to your scene.
- Fill in the Game Id and the Api Key of your game:
  - Create account in https://console.curseforge.com/
  - Add a game to your account
  - Use the Game Id  and the Api key of your game to fill in the ApiManager prefab in the READ_APIManager script the coresponding fields

For more information please read https://docs.curseforge.com/#getting-started

## Usage

The Api is ready to use.

- You may create your own UI and access it through READ_APIManager e.g use
READ_APIManager.Instance.SearchMods in order to find mods in accordance to the
pagination and search parameters

For more information please read https://docs.curseforge.com/#accessing-the-service

- This package includes prefabs that implement the most common functionality
that you will in your game. You are free to use them at your discretion.
  - CFCoreModsBrowser : A multi purpuse Prefab that allow the following functions:
    - Browse mods
    - Download mods
    - Uninstall mods
    - Like/Unlike mods
  - Popup-UploadMod : A Popup that allow the user to upload new mods and update
 existing ones. This package includes SampleCFCoreUploadMod which is an example of usage of this popup and a few use cases.You can find it in the
 "SampleUploadMods" folder.

For more information please read the README file in the "Readme" folder.

## More info

You can find another README files in :
.../Assets/cfcore/cfcore-unity-ui/readme-files/UploadModReadMe.md : A more thorough explanation about how to upload mods

For more info about the API please visit https://core.curseforge.com/
For more developer actions and moderation please visit https://console.curseforge.com/#/
You can also create and browse mods from our main site https://www.curseforge.com/YOURGAMENAME/

Join our Discord server

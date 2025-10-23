
# cfcore-csharp-unity

This readme file will help you getting started on how to work with the Eternal
SDK from your game (should you choose not to use our Unity UI).

> **NOTE:** you are using IL2CPP as your Scripting Backend when building your
> game, you might need to add a link.xml to your project's asset folder.
> We added a link.xm_ file to this project which you should take and rename to
> link.xml

## Setup
Unzip cfcore-unity-x.y.z.zip into your project's Assets folder.

NOTE: In newer versions of Unity (2020) you do not need to bundle the Json.Net.
In this case, you may see console errors. To fix this, just remove the
vendor/Json.net folder and relaunch the project.

## Usage

- Initialization:
```
private const int kGameId = [YOUR_GAME_ID];
private const string kApiKey = [YOUR_GAME_API_KEY];
private const uint kMaxConcurrentInstallations = 3;

...

_cfcoreService = BootstrapUnity.Create(new Settings() {
  GameId = kGameId,
  ApiKey = kApiKey,
  ModsDirectory = GetModsDirectory(),
  UserDataDirectory = GetUserDataDirectory(),
  MaxConcurrentInstallations = kMaxConcurrentInstallations
});

_cfcoreService.Initialize(
  () => Debug.Log("Successfully initialized"),
  (error) => Debug.LogError($"Failed to initialize: {error}"));

...

private string GetModsDirectory() {
#if UNITY_EDITOR
    return Path.Combine(Directory.GetCurrentDirectory(), "cfcore/mods");
#else
  return Path.Combine(Application.dataPath, "mods");
#endif
}

private string GetUserDataDirectory() {
#if UNITY_EDITOR
    return Path.Combine(Directory.GetCurrentDirectory(), "cfcore/user_data");
#else
  return Path.Combine(Application.persistentDataPath, "cfcore/userData");
#endif
}
```

- Authentication:
-- Auth token from email OTP:
```
_cfcoreService.CFCoreAuthentication.SendSecurityCode(
  "email@sample.com", // User input
  () => Debug.Log("Successfully sent email - awaiting user code input"),
  (error) => Debug.LogError($"Failed: {error.Description}"));

...

_cfcoreService.CFCoreAuthentication.GenerateAuthToken(
  "email@sample.com", // User input
  821138, // User input
  () => Debug.Log("Succesfully authenticated user"),
  (err) => Debug.LogError(err.Description));
```

- Creating a mod and uploading a file:
```
_cfcoreService.CFCoreApi.CreateMod(new CreateModRequestDto() {
  ClassId = 5133,
  Name = "Test 1",
  Summary = "Some summary here",
  Description = "Some description here",
  DescriptionType = MarkupType.PlainText,
  PrimaryGameCategoryId = 5134,
  IsExperimental = true
},
(response) => Debug.Log(response.Data.Id.ToString()), // use this for uploading
(error) => Debug.LogError(error.Description));

...

_cfcoreService.CFCoreApi.CreateFile(
  modId,
  new FileInfo(@"D:\game\uploads\mod.zip"),
  new UploadModFileRequestDto() {
    GameVersionIds = new List<int>() { -1 },
    Filename = "test.zip",
    DisplayName = "test",
    ChangelogType = ChangelogMarkupType.Text,
    Changelog = "This is the changelog",
    ReleaseType = FileReleaseType.Release
  },
  (response) => {
    Debug.Log($"FileId: {response.Data.FileId}");
  },
  (progress) => Debug.Log(progress.Progress),
  (error) => Debug.LogError(error.Description));
```

# TODOs

- Install - Disk space issues
- Install - dependencies
- Re-validate authentication token upon initialization
- Install mod progress should include copying, deleting and not just downloading
- Global delegate interface
- Logger interface
- Auto token renewal
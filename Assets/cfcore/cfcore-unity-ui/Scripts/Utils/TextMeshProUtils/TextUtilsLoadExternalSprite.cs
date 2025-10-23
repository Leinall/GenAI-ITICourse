using B83.Image.GIF;
using DaVikingCode.AssetPacker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Overwolf.CFCore.UnityUI.Utils {

  public class TextUtilsLoadExternalSprite : MonoBehaviour {
    // Start is called before the first frame update

    public TMP_Text TMP_TextGameComponenet;

    public Texture2D spriteAssetTexture; // This texture will be overiden

    private string localFilePath;
    private string TempFileLocation = "Temp/AssetPacker/TMP_test";
    private string GifLocation = "Temp/AssetPacker/Gif2";
    private List<string> ImageLinks;

    private readonly string[] kPrefixSeparator = new string[] { "<pic=" };
    private const char kSufixSeperator = '>';
    private const string spriteFileNamePrefix = "tmpsprite";
    private AssetPacker assetPacker;
    private string TextToShow;

    // amount of pictures we finished loaded (successfully and not) so ther numbernig will be consistent
    private int numberOfProcessedPictures;
    private Dictionary<string, int> URLSpritePositionDictionary;

    private void Start() {
      localFilePath = Application.persistentDataPath + '/' + TempFileLocation;
    }

    /// <summary>
    /// Extracts the links to the images and put them in the ImageLinks list.
    /// </summary>
    /// <param name="text"></param>
    /// <returns>Input string with TMP tags instead </returns>
    private void ExtractImageLinksFromText(string text) {

      URLSpritePositionDictionary = new Dictionary<string, int>();
      TextToShow = text;
      // we add this as a way to make sure we don't include anything before the first seperation
      var  Temptext = kSufixSeperator + text;

      // split the text to chunks and we know for sure that the first chunk will not contain a valid link
      string[] firstSeperation  = Temptext.Split(kPrefixSeparator, StringSplitOptions.RemoveEmptyEntries);

      int imageIndex = 0;
      foreach (string str in firstSeperation ) {
        var ans = str.Split(kSufixSeperator);
        if (ans.Length >0 && ans[0].Length>0)
          {
        /* if (ans[0].Contains(".gif"))
              {
            GifLinks.Add(ans[0]);
            continue;
          }*/
          ImageLinks.Add(ans[0]);
          URLSpritePositionDictionary.Add(ans[0], imageIndex);
          imageIndex++;
        }
      }
    }

    public string ChangeTextImageFormating(string text) {

      foreach (var entry in URLSpritePositionDictionary) {

        Sprite spr = assetPacker.GetSprite($"{spriteFileNamePrefix}{entry.Value}");
        if (spr == null) {
          continue;
        }

        string[] nameSeperator = new string[] { $"{kPrefixSeparator[0]}{entry.Key}{kSufixSeperator}" };

        string[] seperation = text.Split(nameSeperator, StringSplitOptions.RemoveEmptyEntries);

        // if there is no size override we need to use size of the picture
        if (seperation.Length>1 &&!seperation[1].StartsWith("</size>")) {
          text = text.Replace($"{kPrefixSeparator[0]}{entry.Key}{kSufixSeperator}",
                    $"<size={spr.rect.height}><sprite={entry.Value}></size>");
        } else {
          text= text.Replace($"{kPrefixSeparator[0]}{entry.Key}{kSufixSeperator}",
                    $"<sprite={entry.Value}>");
        }
      }
        return text;
    }

    private void LoadSprites() {
      if (ImageLinks.Count == 0) {
        TMP_TextGameComponenet.text = TextToShow;
        return;
      }

      foreach (string url in ImageLinks) {
        StartCoroutine(DownloadTexture(url, (Tex) => {
          CreateTMPSpriteFromTexture(Tex, URLSpritePositionDictionary[url]);
        }, CreateAssetPack));
      }
    }

    public void ProccessText(string text) {
      ImageLinks = new List<string>();
      //GifLinks = new List<string>();
      assetPacker = GetComponent<AssetPacker>();
      assetPacker.ClearData();
      numberOfProcessedPictures = 0;

      ExtractImageLinksFromText(text);
      LoadSprites();
    }


    IEnumerator DownloadTexture(string URL, Action<Texture> OnSuccess, Action OnFailure) {
      UnityWebRequest request = UnityWebRequestTexture.GetTexture(URL);
      yield return request.SendWebRequest();

      numberOfProcessedPictures++;

#if UNITY_2020_1_OR_NEWER
      if (request.result == UnityWebRequest.Result.ProtocolError ||
          request.result == UnityWebRequest.Result.ConnectionError) {
#else
      if (request.isHttpError || request.isNetworkError) {
#endif
        Debug.LogError("Failed to download an image \n" + request.error);
        OnFailure();
      } else {
        OnSuccess(((DownloadHandlerTexture)request.downloadHandler).texture);
      }
      request.Dispose();
    }

    void CreateTMPSpriteFromTexture(Texture texture,int spriteID) {
      Sprite spriteSaved = Sprite.Create((Texture2D)texture,
              new Rect(0, 0, texture.width, texture.height),
              new Vector2(0, 1));

      Directory.CreateDirectory(localFilePath);

      byte[] spriteByteArray = spriteSaved.texture.EncodeToPNG();

      File.WriteAllBytes(localFilePath + $"/{spriteFileNamePrefix}{spriteID}.png", spriteByteArray);

      CreateAssetPack();
    }

    void CreateAssetPack() {
      // we don't do anything until we finished processing all the urls
      if (numberOfProcessedPictures < ImageLinks.Count)
        return;

      string[] files = Directory.GetFiles(localFilePath, "*.png");
      assetPacker = transform.GetComponent<AssetPacker>();

      assetPacker.OnProcessCompleted.AddListener(()=> { ApplySpriteToTMPText(assetPacker.modifiedTexture); });

      assetPacker.AddTexturesToPack(files);

      files = Directory.GetFiles(Application.persistentDataPath + '/' + GifLocation, "*.png");
      numberOfProcessedPictures += files.Length;
      assetPacker.AddTexturesToPack(files);

      assetPacker.Process();
    }

    void ApplySpriteToTMPText(Texture textureToApply) {

      if (Directory.Exists(localFilePath)) {
        Directory.Delete(localFilePath, true);
      }
      Color32[] colors = ((Texture2D)textureToApply).GetPixels32();

      spriteAssetTexture.SetPixels32(colors);

      spriteAssetTexture.Apply();

      for (int i = 0; i < numberOfProcessedPictures; i++) {

        Sprite spr = assetPacker.GetSprite($"{spriteFileNamePrefix}{i}");
        if (spr == null) {
          /*    spr = assetPacker.GetSprite($"gifsprite{i}");
            if (spr == null)  */
          {
            continue;
          }
        }

          TMP_TextGameComponenet.spriteAsset.spriteGlyphTable[i].glyphRect
         = new UnityEngine.TextCore.GlyphRect((int)spr.rect.x, (int)spr.rect.y,
         (int)spr.rect.width, (int)spr.rect.height);

        TMP_TextGameComponenet.spriteAsset.spriteGlyphTable[i].metrics =
          new UnityEngine.TextCore.GlyphMetrics(
            (int)spr.rect.width,
            (int)spr.rect.height,
            0,
             (int)spr.rect.height, (int)spr.rect.width);
      }
      spriteAssetTexture.Apply();
      TMP_TextGameComponenet.text = ChangeTextImageFormating(TextToShow);
    }
  }
}
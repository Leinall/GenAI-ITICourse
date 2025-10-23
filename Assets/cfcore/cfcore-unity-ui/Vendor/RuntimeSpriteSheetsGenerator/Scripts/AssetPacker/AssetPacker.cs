using DaVikingCode.RectanglePacking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.Networking;

namespace DaVikingCode.AssetPacker {

	public class AssetPacker : MonoBehaviour {

		public UnityEvent OnProcessCompleted;
		public float pixelsPerUnit = 100.0f;

		public bool useCache = true;
		public int cacheVersion = 1;
		public bool deletePreviousCacheVersion = true;
		[SerializeField]
		public Texture2D modifiedTexture;

		protected Dictionary<string, Sprite> mSprites = new Dictionary<string, Sprite>();
		protected List<TextureToPack> itemsToRaster = new List<TextureToPack>();

		protected bool allow4096Textures = false;

		private static AssetPacker _instance;
		public static AssetPacker Instance {
			get {
				if (_instance == null)
					_instance = new AssetPacker();
				return _instance;
			}
		}

    private void Start() {
			_instance = this;

		}

		public void ClearData() {
		 mSprites = new Dictionary<string, Sprite>();
		 itemsToRaster = new List<TextureToPack>();
			Debug.Log("data cleared");
		}

    public void AddTextureToPack(string file, string customID = null) {

			itemsToRaster.Add(new TextureToPack(file, customID != null ? customID : Path.GetFileNameWithoutExtension(file)));
		}

		public void AddTexturesToPack(string[] files) {

			foreach (string file in files)
				AddTextureToPack(file);
		}

		public void Process( bool allow4096Textures = false) {

			this.allow4096Textures = allow4096Textures;
				StartCoroutine(createPack());
		}


		protected IEnumerator createPack() {

			List<Texture2D> textures = new List<Texture2D>();
			List<string> images = new List<string>();

			foreach (TextureToPack itemToRaster in itemsToRaster) {

				UnityWebRequest request = UnityWebRequestTexture.GetTexture("file:///" + itemToRaster.file);
				yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
				if (request.result == UnityWebRequest.Result.ProtocolError ||
						request.result == UnityWebRequest.Result.ConnectionError) {
#else
				if (request.isHttpError || request.isNetworkError) {
#endif
					Debug.LogError("asset packer downloading failed!\n" + request.error);
				} else {
					textures.Add(((DownloadHandlerTexture)request.downloadHandler).texture);
					images.Add(itemToRaster.id);
				}
				request.Dispose();
			}

			int textureSize = allow4096Textures ? 4096 : 2048;

			List<Rect> rectangles = new List<Rect>();
			for (int i = 0; i < textures.Count; i++)
				if (textures[i].width > textureSize || textures[i].height > textureSize)
					throw new Exception("A texture size is bigger than the sprite sheet size!");
				else
					rectangles.Add(new Rect(0, 0, textures[i].width, textures[i].height));

			const int padding = 1;

			while (rectangles.Count > 0) {
				modifiedTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
				Color32[] fillColor = modifiedTexture.GetPixels32();

				RectanglePacker packer = new RectanglePacker(modifiedTexture.width, modifiedTexture.height, padding);

				for (int i = 0; i < rectangles.Count; i++)
					packer.insertRectangle((int)rectangles[i].width, (int)rectangles[i].height, i);

				packer.packRectangles();

				if (packer.rectangleCount > 0) {

					modifiedTexture.SetPixels32(fillColor);
					modifiedTexture.Apply();
					IntegerRectangle rect = new IntegerRectangle();
					List<TextureAsset> textureAssets = new List<TextureAsset>();

					List<Rect> garbageRect = new List<Rect>();
					List<Texture2D> garabeTextures = new List<Texture2D>();
					List<string> garbageImages = new List<string>();

					for (int j = 0; j < packer.rectangleCount; j++) {

						rect = packer.getRectangle(j, rect);

						int index = packer.getRectangleId(j);

						modifiedTexture.SetPixels32(rect.x, rect.y, rect.width, rect.height, textures[index].GetPixels32());
						modifiedTexture.Apply();

						TextureAsset textureAsset = new TextureAsset();
						textureAsset.x = rect.x;
						textureAsset.y = rect.y;
						textureAsset.width = rect.width;
						textureAsset.height = rect.height;
						textureAsset.name = images[index];

						textureAssets.Add(textureAsset);

						garbageRect.Add(rectangles[index]);
						garabeTextures.Add(textures[index]);
						garbageImages.Add(images[index]);
					}

					foreach (Rect garbage in garbageRect)
						rectangles.Remove(garbage);

					foreach (Texture2D garbage in garabeTextures)
						textures.Remove(garbage);

					foreach (string garbage in garbageImages)
						images.Remove(garbage);

					foreach (TextureAsset textureAsset in textureAssets)
						mSprites.Add(textureAsset.name, Sprite.Create(modifiedTexture, new Rect(textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height), Vector2.zero, pixelsPerUnit, 0, SpriteMeshType.FullRect));
				}
				Directory.CreateDirectory(Application.persistentDataPath + "/Temp/AssetPacker/1");
				File.WriteAllBytes(Application.persistentDataPath + "/Temp/AssetPacker/1/TempFile" + 0 + ".png", modifiedTexture.EncodeToPNG());
			}

			OnProcessCompleted?.Invoke();
		}

		public void Dispose() {

			if (Directory.Exists(Application.persistentDataPath + "/Temp/AssetPacker")) {
				Directory.Delete(Application.persistentDataPath + "/Temp/AssetPacker",true);
			}

			foreach (var asset in mSprites)
				Destroy(asset.Value.texture);

			mSprites.Clear();
		}

		void Destroy() {
			Dispose();
		}

		public Sprite GetSprite(string id) {

			Sprite sprite = null;

			mSprites.TryGetValue(id, out sprite);

			return sprite;
		}

		public Sprite[] GetSprites(string prefix) {

			List<string> spriteNames = new List<string>();
			foreach (var asset in mSprites)
				if (asset.Key.StartsWith(prefix))
					spriteNames.Add(asset.Key);

			spriteNames.Sort(StringComparer.Ordinal);

			List<Sprite> sprites = new List<Sprite>();
			Sprite sprite;
			for (int i = 0; i < spriteNames.Count; ++i) {

				mSprites.TryGetValue(spriteNames[i], out sprite);

				sprites.Add(sprite);
			}

			return sprites.ToArray();
		}
	}
}
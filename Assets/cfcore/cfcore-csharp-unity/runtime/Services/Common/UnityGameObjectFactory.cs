using System.Collections.Generic;
using UnityEngine;

namespace Overwolf.CFCore.Services.Common {
  // ---------------------------------------------------------------------------
  class UnityGameObjectFactory {
    private static Dictionary<string, GameObject> NameToGameObjectDictonary;
    // -------------------------------------------------------------------------
    // In order to instantiate an instance of a |MonoBehaviour| derived class.
    //
    // This function also assures we reuse existing game objects so that they
    // don't add more than once to the context
    public static T Create<T>() where T : Component {
      GameObject gameObject = GetGameObject<T>();
      T instance = GetComponent<T>(gameObject);

      GameObject.DontDestroyOnLoad(gameObject);

      return instance;
    }

    // -------------------------------------------------------------------------
    private static GameObject GetGameObject<T>() where T : Component {
      if (NameToGameObjectDictonary == null) {
        NameToGameObjectDictonary = new Dictionary<string, GameObject>();
      }

      string objName = $"CFCore-{typeof(T)}";

      GameObject gameObject;
      bool isDictionaryContainsValue =
        NameToGameObjectDictonary.TryGetValue(objName, out gameObject);
      if (isDictionaryContainsValue && gameObject != null)
        return gameObject;

      gameObject = GameObject.Find(objName);

      if (gameObject == null) {
        gameObject = new GameObject(objName);
      };

      if (isDictionaryContainsValue) {
        NameToGameObjectDictonary[objName] = gameObject;
      } else {
        NameToGameObjectDictonary.Add(objName, gameObject);
      }

      return gameObject;
    }

    // -------------------------------------------------------------------------
    private static T GetComponent<T>(GameObject gameObj) where T : Component {
      var instance = gameObj.GetComponent<T>();
      if (instance == null) {
        instance = gameObj.AddComponent<T>();
      }

      return instance;
    }
  }
}

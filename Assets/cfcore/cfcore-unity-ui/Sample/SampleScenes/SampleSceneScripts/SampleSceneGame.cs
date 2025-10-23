using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Overwolf.CFCore.UnityUI.Managers;

namespace Overwolf.CFCore.UnityUI.SampleGame {
  public class SampleSceneGame : MonoBehaviour {

   public void Start() {
      // This is only needed if "Initialize API at startup" is disabled in the settings
      READ_APIManager.Instance.Init();
    }

  }
}

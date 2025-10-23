using Overwolf.CFCore.Base.Library.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Overwolf.CFCore.UnityUI.UIItems {
  public static class InstalledModsSorter {

    public static List<InstalledMod> SortByLatestInstalled(List<InstalledMod> inputList) {
      inputList.Sort((a, b) => DateTime.Compare(a.DateInstalled, b.DateInstalled));
      return inputList;
    }

    public static List<InstalledMod> SortByModName(List<InstalledMod> inputList) {
      inputList.Sort((a, b) => String.Compare(a.Details.Name, b.Details.Name));
      return inputList;
    }

    public static List<InstalledMod> SortByAuthorName(List<InstalledMod> inputList) {
      inputList.Sort((a, b) => String.Compare(a.Details.Authors[0].Name, b.Details.Authors[0].Name));
      return inputList;
    }
  }
}

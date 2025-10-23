using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Overwolf.CFCore.Base.Api.Common;
using Overwolf.CFCore.Base.Api.Models.Filters;
using Overwolf.CFCore.Base.Api.Models.Enums;
using Overwolf.CFCore.UnityUI.UIItems;
using System.Collections.Generic;
using UnityEngine.Events;
using Overwolf.CFCore.CFCContext;

namespace Overwolf.CFCore.UnityUI.Managers {

  [System.Serializable]
  public enum NavigationSection { None, BrowseMods, InstalledMods, MyCreations }

  [System.Serializable]
  public class NavigationButton {
    public Button button;
    public NavigationSection navigationSection;
    public GameObject Panel;
    public UnityEvent OnClickEvent;
  }

  public class NavigationManager : MonoBehaviour {
    private static NavigationManager _instance;
    [SerializeField] int FirstActive = 0;

    public static NavigationManager Instance {
      get {
        if (_instance == null)
          _instance = new NavigationManager();
        return _instance;
      }
    }

    public NavigationSection SelectedNavigationSection = NavigationSection.None;

    private Button lastButtonSelected;

#pragma warning disable 0649 // allocated in Prefab
    [Header("Navigation Bar InputField & Buttons")]
    public TMP_InputField InputField_Search;
    public GameObject Btn_ClearSearchString;

    [SerializeField] List<NavigationButton> NavigationButtons;

#pragma warning restore 0649

    // -------------------------------------------------------------------------
    private void Awake() {
      _instance = this;
    }

    // -------------------------------------------------------------------------
    IEnumerator Start() {
      Context.Instance.AddListener(
        CFCContextConstants.CurrentFirstModInPage, OnModsChange);
      yield return new WaitUntil(() => READ_APIManager.Instance.IsReadyStart);

      InitializeButtons();
      NavigationButtons[FirstActive].button.onClick.Invoke();
    }

    // -------------------------------------------------------------------------
    private void OnDestroy() {
      Context.Instance.RemoveListener(
        CFCContextConstants.CurrentFirstModInPage, OnModsChange);
    }

    // -------------------------------------------------------------------------
    private void InitializeButtons() {
      lastButtonSelected = NavigationButtons[0].button;
      foreach (var navButton in NavigationButtons) {
        navButton.button.onClick.AddListener(() => OnNavButtonClicked(navButton));
      }
    }

    // -------------------------------------------------------------------------
    private void OnNavButtonClicked(NavigationButton clickedButton) {
      lastButtonSelected = clickedButton.button;

      // Will be changed by looking at context menu and closing the popup
      var modDetailViewer = FindObjectOfType<ModDetailViewer>();
      if (modDetailViewer != null)
        Destroy(modDetailViewer.gameObject);

      CFCContext.Context.Instance.SetContext(CFCContext.CFCContextConstants.IsUINavigationMode, true);

      int index = NavigationButtons.IndexOf(clickedButton);
      for (int i = 0; i < NavigationButtons.Count; i++) {
        if (i == index) {
          SelectedNavigationSection = clickedButton.navigationSection;
          NavigationButtons[i].Panel.SetActive(true);
          NavigationButtons[i].button.transform.GetComponent<Sidebar_HoverActiveEffect>().SetActive(true);
        } else {
          NavigationButtons[i].Panel.SetActive(false);
          NavigationButtons[i].button.transform.GetComponent<Sidebar_HoverActiveEffect>().SetActive(false);
        }
      }
      clickedButton.OnClickEvent?.Invoke();
      Context.Instance.SetContext(CFCContextConstants.LastSelectedClass,0);
    }

    #region Navigation Bar

    // -------------------------------------------------------------------------
    public void SearchModsWithString() {
      switch (SelectedNavigationSection) {
        case NavigationSection.BrowseMods: {
            SearchModsFilter searchModsFilter = new SearchModsFilter();
            searchModsFilter.ClassId = UIManager.Instance.BrowseMods_SelectedClassTab.category.Id;
            searchModsFilter.CategoryId = UIManager.Instance.BrowseMods_SelectedClassTab.categoryFilter;
            searchModsFilter.SortField = UIManager.Instance.BrowseMods_SelectedClassTab.searchSortField;
            if (searchModsFilter.SortField == ModsSearchSortField.Name || searchModsFilter.SortField == ModsSearchSortField.Author)
              searchModsFilter.SortOrder = SortOrder.asc;
            else
              searchModsFilter.SortOrder = SortOrder.desc;
            searchModsFilter.SearchFilter = InputField_Search.text;

            ApiRequestPagination pagination = new ApiRequestPagination();
            pagination.Index = 0;
            pagination.PageSize = UIManager.Instance.ModsPerPage;

            UIManager.Instance.BrowseMods(searchModsFilter, pagination);
          }
          break;
        case NavigationSection.InstalledMods: {
            UIManager.Instance.SearchInstalledMods(InputField_Search.text);
          }
          break;
        case NavigationSection.MyCreations: {
            UIManager.Instance.SearchMyCreation(InputField_Search.text);
          }
          break;
      }
    }

    // -------------------------------------------------------------------------
    public void NavigateToBrowseMods() {
      var button = NavigationButtons.Find(
        btn => btn.navigationSection == NavigationSection.BrowseMods);
      button.OnClickEvent?.Invoke();
    }

    // -------------------------------------------------------------------------
    public void SearchModEdit() {
      Btn_ClearSearchString.SetActive(!string.IsNullOrEmpty(InputField_Search.text));
    }

    // -------------------------------------------------------------------------
    public void ClearSearchString() {
      InputField_Search.text = "";
      Btn_ClearSearchString.SetActive(false);
      SearchModsWithString();
    }
    #endregion

    // -------------------------------------------------------------------------
    private void OnModsChange(GameObject firstModInPage) {
      Selectable selectable = firstModInPage == null ? null
        : firstModInPage.GetComponent<Selectable>();

      foreach (NavigationButton navButton in NavigationButtons) {
        var tempNav = navButton.button.navigation;
        tempNav.selectOnRight = selectable;
        navButton.button.navigation = tempNav;
      }
    }

    // -------------------------------------------------------------------------
    public Button GetFirstButton() {
      return NavigationButtons[FirstActive].button;
    }

    // -------------------------------------------------------------------------
    public Button GetLastButtonSelected() {
      return lastButtonSelected;
    }
  }
}

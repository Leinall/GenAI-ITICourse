using Overwolf.CFCore.Base.Api.Common;
using Overwolf.CFCore.Base.Api.Models;
using Overwolf.CFCore.Base.Api.Models.Enums;
using Overwolf.CFCore.Base.Api.Models.Filters;
using Overwolf.CFCore.Base.Api.Models.Users;
using Overwolf.CFCore.Base.Common;
using Overwolf.CFCore.Base.Creation.Models;
using Overwolf.CFCore.Base.Library.Models;
using Overwolf.CFCore.UnityUI.Gamepad;
using Overwolf.CFCore.UnityUI.InputController;
using Overwolf.CFCore.UnityUI.Popups;
using Overwolf.CFCore.UnityUI.Themes;
using Overwolf.CFCore.UnityUI.UIItems;
using Overwolf.CFCore.UnityUI.View;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Overwolf.CFCore.UnityUI.Managers {
  public class UIManager : MonoBehaviour, ICancelHandler {
    private static UIManager _instance;

    public static UIManager Instance {
      get {
        if (_instance == null)
          _instance = new UIManager();
        return _instance;
      }
    }

    private uint modsPerPage = 14;
    [SerializeField]
    private GamepadPromptsScript gamepadPrompts;

    public UnityEvent OnQuit = new();

#pragma warning disable 0649 // allocated in Prefab
    [Header("Profile")]
    [SerializeField] GameObject Btn_Signin;
    [SerializeField] GameObject Item_Profile;
    [SerializeField] GameObject Btn_SignOut;
    [SerializeField] TextMeshProUGUI Txt_ProfileName;
    [SerializeField] RawImage Img_ProfileTexture;

    [Header("Browse Mods Panel ")]
    public Transform Transform_BrowsePanel;
    [SerializeField] GameObject Item_BrowseModsTitle;
    [SerializeField] GameObject Item_BrowseMods_SearchedResultTitle;
    [SerializeField] TextMeshProUGUI Txt_BrowseMods_SearchedResultDetails;
    [SerializeField] Transform Transform_BrowseMods_ClassTab;

    [SerializeField] Transform Transform_BrowseMods_ModList;

    private  List<ModTileScript> currentModItems;
    private List<Selectable> currentModItemsSelectables;

    public TMP_Dropdown DropDown_BrowseModsSortField;
    private Selectable Selectable_BrowseModsSortField;
    public TMP_Dropdown DropDown_BrowseModsCategoryFilter;
    private Selectable Selectable_BrowseModsCategoryFilter;

    public List<GameObject> ClassItemsGameObjects;

    [SerializeField] GameObject Item_BrowseMods_Pagination;
    [SerializeField] Button Btn_BrowseMods_PrevPagination;
    [SerializeField] Button Btn_BrowseMods_NextPagination;
    public TextMeshProUGUI Txt_BrowseMods_PageCount;
    bool isPaginationBusy = false;

    [SerializeField] GameObject Panel_BrowseMods_NoSearchResult;
    [SerializeField] TextMeshProUGUI Txt_BrowseMods_NoResultForKeyword;
    [SerializeField] TextMeshProUGUI Txt_BrowseMods_NoResultSuggestion;

    public ClassTabItem BrowseMods_SelectedClassTab { get; set; }

    [Header("Installed Mods Panel")]
    public Transform Transform_InstalledPanel;
    [SerializeField] Transform Transform_InstalledMods_ClassTab;
    [SerializeField] Transform Transform_InstalledMods_ModList;

    public TMP_Dropdown DropDown_InstalledMods_SortField;
    private Selectable Selectable_InstalledMods_SortField;

    [SerializeField] GameObject Item_InstalledMods_Pagination;
    [SerializeField] Button Btn_InstalledMods_PrevPagination;
    [SerializeField] Button Btn_InstalledMods_NextPagination;
    public TextMeshProUGUI Txt_InstalledMods_PageCount;

    [SerializeField] GameObject Panel_InstalledMods_EmptyView;

    public ClassTabItem InstalledMods_SelectedClassTab { get; set; }
    public uint ModsPerPage { get => modsPerPage; }

    [Header("MyCreation Mods Panel")]
    public Transform Transform_MyCreations;
    [SerializeField]
    public GameObject GameObject_MyCreationButton;

    [Header("Popup Prefabs")]
    [SerializeField] GameObject PopupSignInPrefab;

    [SerializeField] GameObject PopupClassTabItemPrefab;
    [SerializeField] GameObject PopupModTilePrefab;
    [SerializeField] GameObject PopupAlertPrefab;

    [Header("Error GameObjects")]
    [SerializeField] GameObject ConectionErrorObject;
#pragma warning restore 0649

    // mods that are installing so we can't delete them otherwise we lose info about their state
    private Dictionary<Mod, ModTileScript> stashedInstallingMods;

    private string searchValue = "";
    List<InstalledMod> filteredInstalledMods = new List<InstalledMod>();

    private void Awake() {
      _instance = this;

    }

    private void Start() {
      if (GlobalLoader.Instance != null) {
        GlobalLoader.Instance.SetActive(true);
      }

      if (OnQuit == null) {
        OnQuit = new UnityEvent();
      }

      UpdateTileCount();

      stashedInstallingMods = new Dictionary<Mod, ModTileScript>();
      var theme = EternalUITheme.Instance;
      if (theme != null) {
        if (theme.ColorTheme != null)
          theme.ApplyColorTheme();
        if (theme.ShapeTheme != null)
          theme.ApplyShapeTheme();
        // modsPerPage= theme.ApplyScale();
      }

      var settings =
       READ_APIManager.Settings;

      if (settings != null) {
        GameObject_MyCreationButton?.SetActive(!settings.HideSignIn && !settings.HideMyCreation);
        Btn_Signin.SetActive(!settings.HideSignIn);
      }

      SetSignInOutButtonsNavigation(READ_APIManager.Instance.IsSignedIn);
    }

    public void UpdateTileCount() {
      modsPerPage = (uint)ModTilesLimitGetter.GetModTileLimit(Screen.width / (float)Screen.height);
    }

    private void OnEnable() {
      READ_APIManager.LoginSuccessEvent += LoggedIn;
      READ_APIManager.LogoutSuccessEvent += LoggedOut;
      READ_APIManager.CurrentUserChangedEvent += UpdateUserInfotmation;
      READ_APIManager.ErrorEvent += ManageError;
    }

    private void OnDisable() {
      READ_APIManager.LoginSuccessEvent -= LoggedIn;
      READ_APIManager.LogoutSuccessEvent -= LoggedOut;
      READ_APIManager.CurrentUserChangedEvent -= UpdateUserInfotmation;
      READ_APIManager.ErrorEvent -= ManageError;
    }

    public void OnCancel(BaseEventData eventData) {
      Quit();
    }

    #region SignIn
    public void SignIn() {
      GameObject SigninModal = Instantiate(PopupSignInPrefab, transform);
      if (!SigninModal.TryGetComponent<Popup>(out var popup)) {
        return;
      }

      popup.SetPopupOrigin(EventSystem.current.currentSelectedGameObject);
    }

    public void SignOut() {
      READ_APIManager.Instance.Request_Logout();
    }

    void LoggedIn() {
      READ_APIManager.Instance.GetUserThumbUpMods(UpdateAllItems);
      Btn_Signin.SetActive(false);
      if (NavigationManager.Instance.SelectedNavigationSection == NavigationSection.MyCreations) {
        List_MyCreations();
      }

      SetSignInOutButtonsNavigation(true);
      Item_Profile.GetComponent<Selectable>().Select();
    }

    void UpdateUserInfotmation(Me currentUser) {
      if (currentUser == null) return;

      Txt_ProfileName.text = currentUser.Username;
      StartCoroutine(LoadUserProfileTexture(currentUser));
    }

    IEnumerator LoadUserProfileTexture(Me currentUser) {
      if (currentUser == null || currentUser.AvatarUrl == null) {
        Item_Profile.SetActive(true);
        Debug.LogError("Profile texture is null");
        yield return null;
      }

      string avatarURL = AvatarUrlFixer(currentUser.AvatarUrl);

      UnityWebRequest request =
                       UnityWebRequestTexture.GetTexture(avatarURL);
      yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
      if (request.result == UnityWebRequest.Result.ProtocolError ||
          request.result == UnityWebRequest.Result.ConnectionError) {
#else
      if (request.isHttpError || request.isNetworkError) {
#endif
        Debug.LogError("Profile texture loading failed!\n" + request.error);
      } else {
        Img_ProfileTexture.texture =
                      ((DownloadHandlerTexture)request.downloadHandler).texture;
      }
      Item_Profile.SetActive(true);
      request.Dispose();
    }

    public void UpdateAllItems() {
      if (currentModItems == null) {
        currentModItems =new List<ModTileScript>( Transform_BrowseMods_ModList.GetComponentsInChildren<ModTileScript>()) ;
      }

      foreach (ModTileScript item in currentModItems) {
        item.UpdateStatus();
      }
    }

    string AvatarUrlFixer(string avatarURL) {
      string badString = "{0}";
      string fixedString = "50x50";
      return avatarURL.Replace(badString, fixedString);
    }

    void LoggedOut() {
      Btn_Signin.SetActive(true);
      Item_Profile.SetActive(false);
      Btn_SignOut.SetActive(false);
      if (NavigationManager.Instance.SelectedNavigationSection == NavigationSection.MyCreations) {
        List_MyCreations();
      }

      SetSignInOutButtonsNavigation(false);
      Btn_Signin.GetComponent<Selectable>().Select();
    }

    public void ToggleSignOut() {
      Btn_SignOut.SetActive(!Btn_SignOut.activeSelf);
    }

    private void SetSignInOutButtonsNavigation(bool signedIn) {
      Selectable navigateTo = signedIn
        ? Item_Profile.GetComponent<Selectable>()
        : Btn_Signin.GetComponent<Selectable>();

      SetDropdownNavigation(DropDown_BrowseModsCategoryFilter, navigateTo);
      SetDropdownNavigation(DropDown_BrowseModsSortField, navigateTo);
      SetDropdownNavigation(DropDown_InstalledMods_SortField, navigateTo);

      void SetDropdownNavigation(TMP_Dropdown dropdown, Selectable navigateTo) {
        var nav = dropdown.navigation;
        nav.selectOnUp = navigateTo;
        dropdown.navigation = nav;
      }
    }

    #endregion SignIn

    #region Browse Mods
    /// <summary>
    /// List the Classes at the Top Tab bar
    /// </summary>
    /// <param name="categories"></param>
    public void List_ClassesInBrowseMods() {
      ClassItemsGameObjects = new List<GameObject>();

      for (int i = 0; i < Transform_BrowseMods_ClassTab.childCount; i++)
        Destroy(Transform_BrowseMods_ClassTab.GetChild(i).gameObject);

      bool isFirstClass = true;
      //List the classes at the Tab bar
      for (int i = 0; i < READ_APIManager.Instance.ClassList.Count; i++) {
        GameObject classObj = Instantiate(PopupClassTabItemPrefab, Transform_BrowseMods_ClassTab) as GameObject;
        classObj.GetComponent<ClassTabItem>().SetupItem(READ_APIManager.Instance.ClassList[i]);
        ClassItemsGameObjects.Add(classObj);
        if (isFirstClass) {
          classObj.GetComponent<ClassTabItem>().Select();
          isFirstClass = false;
        }
      }
    }

    public void ClearOldBrowsedModsFromUI() {
      Panel_BrowseMods_NoSearchResult.SetActive(false);

      //Clear old Mods
      for (int i = 0; i < Transform_BrowseMods_ModList.childCount; i++) {
        ModTileScript modItem = Transform_BrowseMods_ModList.GetChild(i).GetComponent<ModTileScript>();
        InstalledMod installedMod = READ_APIManager.Instance.CurrentInstallingModList.Find((item) => modItem.modData.Id == item.Details.Id);
        if (installedMod != null) {
          modItem.gameObject.SetActive(false);
          if (!stashedInstallingMods.ContainsKey(modItem.modData)) {
            stashedInstallingMods.Add(modItem.modData, modItem);
          }
        } else {
          Destroy(modItem.gameObject);
          if (modItem.modData != null) {
            stashedInstallingMods.Remove(modItem.modData);
          }
        }
      }
    }
    public void BrowseMods(SearchModsFilter searchFilter,
      ApiRequestPagination pagination) {
      ClearOldBrowsedModsFromUI();
      READ_APIManager.Instance.SearchMods(searchFilter, pagination,
        (response) => {
          List_SearchedMods(response.Data, response.Pagination,
                            searchFilter.SearchFilter);
          isPaginationBusy = false;
        },
         (e) => { isPaginationBusy = false; });
    }

    /// <summary>
    /// List the Mods at the Content view
    /// </summary>
    /// <param name="searchedModList"></param>
    /// <param name="pageIndex"></param>
    public virtual void List_SearchedMods(List<Mod> searchedModList,
      ApiResponsePagination pagination, string searchKeyword) {
      if (string.IsNullOrEmpty(searchKeyword)) {
        Item_BrowseModsTitle.SetActive(true);
        Item_BrowseMods_SearchedResultTitle.SetActive(false);
      } else {
        Item_BrowseModsTitle.SetActive(false);
        Item_BrowseMods_SearchedResultTitle.SetActive(true);
        Txt_BrowseMods_SearchedResultDetails.text = pagination.TotalCount + " Results for \"" + searchKeyword + "\"";
      }

      if (searchedModList.Count > 0) {
        Panel_BrowseMods_NoSearchResult.SetActive(false);

        uint pageCount;
        if (pagination.TotalCount % pagination.PageSize == 0)
          pageCount = (uint)pagination.TotalCount / (uint)pagination.PageSize;
        else
          pageCount = (uint)pagination.TotalCount / (uint)pagination.PageSize + 1;

        BrowseMods_SelectedClassTab.totalPageCountInBrowseMods = pageCount;

        BrowseMods_SelectedClassTab.currentPageIndexInBrowseMods = (uint)pagination.Index / modsPerPage;

        for (int i = 0; i < searchedModList.Count; i++) {
          ModTileScript installingMod = null;

          foreach (Mod mod in stashedInstallingMods.Keys) {
            if (mod.Id == searchedModList[i].Id)
              installingMod = stashedInstallingMods[mod];
          }

          if (installingMod != null) {
            installingMod.gameObject.SetActive(true);
            installingMod.transform.SetSiblingIndex(i);
          } else {
            GameObject modObj;
            modObj = Instantiate(PopupModTilePrefab, Transform_BrowseMods_ModList) as GameObject;
            ModTileScript item = modObj.GetComponent<ModTileScript>();
            InstalledMod installedMod = READ_APIManager.Instance.InstalledModList.Find(
              (m) => m.Details.Id.Equals(searchedModList[i].Id));

            item.SetupModItem(searchedModList[i], installedMod != null);
            if (installedMod != null) {
              item.SetUpdateRequired(installedMod.Status
             == Base.Library.Models.Enums.InstalledModStatus.OutOfDate);
              item.SetInvalid(installedMod.Status
                == Base.Library.Models.Enums.InstalledModStatus.Invalid);
            }
          }
        }

        Item_BrowseMods_Pagination.SetActive(true);
        gamepadPrompts.SetPromptActive(GamepadButton.Triggers, true);

        if (pageCount == 1) {
          Btn_BrowseMods_PrevPagination.gameObject.SetActive(false);
          Btn_BrowseMods_NextPagination.gameObject.SetActive(false);
        } else {
          if (BrowseMods_SelectedClassTab.currentPageIndexInBrowseMods == pageCount - 1) {
            Btn_BrowseMods_PrevPagination.interactable = true;
            Btn_BrowseMods_NextPagination.interactable = false;
          } else if (BrowseMods_SelectedClassTab.currentPageIndexInBrowseMods == 0) {
            Btn_BrowseMods_PrevPagination.interactable = false;
            Btn_BrowseMods_NextPagination.interactable = true;
          } else {
            Btn_BrowseMods_PrevPagination.interactable = true;
            Btn_BrowseMods_NextPagination.interactable = true;
          }
        }

        Txt_BrowseMods_PageCount.text = pageCount == 1 ? "" :
          "Page " + (BrowseMods_SelectedClassTab.currentPageIndexInBrowseMods + 1) + " of " + BrowseMods_SelectedClassTab.totalPageCountInBrowseMods;
      } else {
        Panel_BrowseMods_NoSearchResult.SetActive(true);

          NavigationManager.Instance.GetLastButtonSelected().Select();

        if (string.IsNullOrEmpty(searchKeyword)) {
          Txt_BrowseMods_NoResultForKeyword.text = "No results";
          Txt_BrowseMods_NoResultSuggestion.gameObject.SetActive(false);
        } else {
          Txt_BrowseMods_NoResultForKeyword.text = "No results for \"" + searchKeyword + "\"";
          Txt_BrowseMods_NoResultSuggestion.gameObject.SetActive(true);
        }

        Item_BrowseMods_Pagination.SetActive(false);
        gamepadPrompts.SetPromptActive(GamepadButton.Triggers, false);
      }

      currentModItems = new List<ModTileScript>(Transform_BrowseMods_ModList.GetComponentsInChildren<ModTileScript>());
      if (currentModItems.Count > 0)
        Debug.Log(currentModItems[0].modData.Name);
      SetupNavigation();
      if (currentModItems.Count > 0)
        CFCContext.Context.Instance.SetContext(CFCContext.CFCContextConstants.CurrentFirstModInPage,
          currentModItems[0].gameObject);
    }

    /// <summary>
    /// Back to normal UI from the searched mods result UI
    /// </summary>
    public void BackFromSearchedResult() {
      NavigationManager.Instance.InputField_Search.SetTextWithoutNotify("");
      NavigationManager.Instance.Btn_ClearSearchString.SetActive(false);

      if (NavigationManager.Instance.SelectedNavigationSection == NavigationSection.BrowseMods) {
        SearchModsFilter searchModsFilter = new SearchModsFilter();
        searchModsFilter.ClassId = BrowseMods_SelectedClassTab.category.Id;
        searchModsFilter.CategoryId = BrowseMods_SelectedClassTab.categoryFilter;
        searchModsFilter.SortField = BrowseMods_SelectedClassTab.searchSortField;
        if (searchModsFilter.SortField == ModsSearchSortField.Name || searchModsFilter.SortField == ModsSearchSortField.Author)
          searchModsFilter.SortOrder = SortOrder.asc;
        else
          searchModsFilter.SortOrder = SortOrder.desc;
        searchModsFilter.SearchFilter = NavigationManager.Instance.InputField_Search.text;

        ApiRequestPagination pagination = new ApiRequestPagination();
        pagination.Index = 0;
        pagination.PageSize = modsPerPage;

        BrowseMods(searchModsFilter, pagination);
      }
    }

    public void SearchMyCreation(string searchValue) {
      var view = GetComponentInChildren<MyCreationsView>();
      if (view != null) {
        view.Search(searchValue);
      }
    }

    public void SearchInstalledMods(string searchValue) {
      this.searchValue = searchValue;
      Paginate(next: true, searchValue, isResetToFirstPage: true); // the next value is disregarded
    }

    #endregion


    #region Installed Mods
    public void List_ClassesInInstalledMods() {
      ClassItemsGameObjects = new List<GameObject>();

      for (int i = 0; i < Transform_InstalledMods_ClassTab.childCount; i++)
        Destroy(Transform_InstalledMods_ClassTab.GetChild(i).gameObject);

      //List classes at class tab bar
      bool isFirstClass = true;
      for (int i = 0; i < READ_APIManager.Instance.ClassList.Count; i++) {
        GameObject classObj = Instantiate(PopupClassTabItemPrefab, Transform_InstalledMods_ClassTab) as GameObject;
        classObj.GetComponent<ClassTabItem>().SetupItem(READ_APIManager.Instance.ClassList[i]);
        ClassItemsGameObjects.Add(classObj);
        if (isFirstClass) {
          classObj.GetComponent<ClassTabItem>().Select();
          isFirstClass = false;
        }
      }
    }

    public void List_InstalledMods(List<InstalledMod> installedMods, bool resetFilterdMods = false) {
      //Clear old Mods
      for (int i = 0; i < Transform_InstalledMods_ModList.childCount; i++)
        Destroy(Transform_InstalledMods_ModList.GetChild(i).gameObject);


        currentModItems = new List<ModTileScript>();
      if (resetFilterdMods)
        filteredInstalledMods = InstalledMods_SelectedClassTab.installedMods;

      if (installedMods.Count > 0) {
        Panel_InstalledMods_EmptyView.SetActive(false);
        int pageCount;

        if (filteredInstalledMods.Count % modsPerPage == 0)
          pageCount = filteredInstalledMods.Count / (int)modsPerPage;
        else
          pageCount = filteredInstalledMods.Count / (int)modsPerPage + 1;

        InstalledMods_SelectedClassTab.totalPageCountInInstalledMods = pageCount;

        for (int i = 0; i < installedMods.Count; i++) {
          GameObject modObj;
          modObj = Instantiate(PopupModTilePrefab, Transform_InstalledMods_ModList) as GameObject;
          ModTileScript item = modObj.GetComponent<ModTileScript>();
          currentModItems.Add(item);
          item.SetupModItem(installedMods[i].Details, true);
          item.SetUpdateRequired(installedMods[i].Status
            == Base.Library.Models.Enums.InstalledModStatus.OutOfDate);
          item.SetInvalid(installedMods[i].Status
            == Base.Library.Models.Enums.InstalledModStatus.Invalid);
        }

        Item_InstalledMods_Pagination.SetActive(true);
        if (InstalledMods_SelectedClassTab.totalPageCountInInstalledMods == 1) {
          gamepadPrompts.SetPromptActive(GamepadButton.Triggers, false);
          Btn_InstalledMods_PrevPagination.gameObject.SetActive(false);
          Btn_InstalledMods_NextPagination.gameObject.SetActive(false);
        } else {
          gamepadPrompts.SetPromptActive(GamepadButton.Triggers, true);
          Btn_InstalledMods_PrevPagination.gameObject.SetActive(true);
          Btn_InstalledMods_NextPagination.gameObject.SetActive(true);
          if (InstalledMods_SelectedClassTab.currentPageIndexInInstalledMods == pageCount - 1) {
            Btn_InstalledMods_PrevPagination.interactable = true;
            Btn_InstalledMods_NextPagination.interactable = false;
          } else if (InstalledMods_SelectedClassTab.currentPageIndexInInstalledMods == 0) {
            Btn_InstalledMods_PrevPagination.interactable = false;
            Btn_InstalledMods_NextPagination.interactable = true;
          } else {
            Btn_InstalledMods_PrevPagination.interactable = true;
            Btn_InstalledMods_NextPagination.interactable = true;
          }
        }

        Txt_InstalledMods_PageCount.text = pageCount == 1 ? "" :
          "Page " + (InstalledMods_SelectedClassTab.currentPageIndexInInstalledMods + 1) + " of " + pageCount;
      } else {
        Panel_InstalledMods_EmptyView.SetActive(true);
        Item_InstalledMods_Pagination.SetActive(false);
        gamepadPrompts.SetPromptActive(GamepadButton.Triggers, false);
        NavigationManager.Instance.GetLastButtonSelected().Select();
      }
      if (currentModItems.Count>0)
      Debug.Log(currentModItems[0].modData.Name);
      SetupNavigation();
      if (currentModItems.Count > 0)
        CFCContext.Context.Instance.SetContext(CFCContext.CFCContextConstants.CurrentFirstModInPage,
          currentModItems[0].gameObject);

      isPaginationBusy = false;
    }
    #endregion

    #region My Creation

    public void List_MyCreations() {

      var view = GetComponentInChildren<MyCreationsView>();
      view.ClearView();
      if (view != null) {
        view.ClearTiles();
      }

      if (!READ_APIManager.Instance.IsSignedIn) {
        if (view != null) {
          view.Initialize();
        }
      } else {
        List<CreationItem> creations = new List<CreationItem>();
        READ_APIManager.Instance.RequestMyCreations((cr) => {
          if (view != null) {
            view.Initialize(cr);
          }
        }, (error) => {

          Debug.LogError(error.Description);
        });
      }
    }

    #endregion

    public void Paginate(bool next) {
      Paginate(next, searchString: searchValue, isResetToFirstPage: false);
    }

    /// <summary>
    /// Pagination for the displayed Mods
    /// </summary>
    /// <param name="next"></param>
    public void Paginate(bool next, string searchString, bool isResetToFirstPage = false) {
      if (isPaginationBusy) {
        return;
      }

      isPaginationBusy = true;
      switch (NavigationManager.Instance.SelectedNavigationSection) {
        case NavigationSection.BrowseMods: {
            if (next) {
              if (BrowseMods_SelectedClassTab.currentPageIndexInBrowseMods == BrowseMods_SelectedClassTab.totalPageCountInBrowseMods - 1){
                isPaginationBusy = false;
                return;
              }

              BrowseMods_SelectedClassTab.currentPageIndexInBrowseMods++;
            } else {
              if (BrowseMods_SelectedClassTab.currentPageIndexInBrowseMods == 0) {
                isPaginationBusy = false;
                return;
              }

              BrowseMods_SelectedClassTab.currentPageIndexInBrowseMods--;
            }

            SearchModsFilter searchModsFilter = new SearchModsFilter();
            searchModsFilter.ClassId = BrowseMods_SelectedClassTab.category.Id;
            searchModsFilter.CategoryId = BrowseMods_SelectedClassTab.categoryFilter;
            searchModsFilter.SortField = BrowseMods_SelectedClassTab.searchSortField;
            if (searchModsFilter.SortField == ModsSearchSortField.Name || searchModsFilter.SortField == ModsSearchSortField.Author)
              searchModsFilter.SortOrder = SortOrder.asc;
            else
              searchModsFilter.SortOrder = SortOrder.desc;
            searchModsFilter.SearchFilter = NavigationManager.Instance.InputField_Search.text;

            ApiRequestPagination pagination = new ApiRequestPagination();
            pagination.Index = BrowseMods_SelectedClassTab.currentPageIndexInBrowseMods * modsPerPage;
            pagination.PageSize = modsPerPage;

            BrowseMods(searchModsFilter, pagination);
          }
          break;
        case NavigationSection.InstalledMods: {
            int pageIndex;
            if (isResetToFirstPage) {
              pageIndex = 0;
              InstalledMods_SelectedClassTab.currentPageIndexInInstalledMods = 0;
            } else {

              if (next) {
                if (InstalledMods_SelectedClassTab.currentPageIndexInInstalledMods ==
                     InstalledMods_SelectedClassTab.totalPageCountInInstalledMods - 1) {
                  isPaginationBusy = false;
                  return;
                }

                InstalledMods_SelectedClassTab.currentPageIndexInInstalledMods++;
              } else {
                if (InstalledMods_SelectedClassTab.currentPageIndexInInstalledMods == 0) {
                  isPaginationBusy = false;
                  return;
                }

                InstalledMods_SelectedClassTab.currentPageIndexInInstalledMods--;
              }

              pageIndex = InstalledMods_SelectedClassTab.currentPageIndexInInstalledMods;
            }

            List<InstalledMod> installedModsToDisplay = new List<InstalledMod>();
            filteredInstalledMods = new List<InstalledMod>();
            if (searchString != null && searchString != "") {
              foreach (var mod in InstalledMods_SelectedClassTab.installedMods) {
                if (mod.Details.Name.Contains(searchString))
                  filteredInstalledMods.Add(mod);
              }
            } else {
              filteredInstalledMods = new List<InstalledMod>(InstalledMods_SelectedClassTab.installedMods);
            }

            for (int i = pageIndex * (int)modsPerPage; i < (pageIndex + 1) * modsPerPage; i++) {
              if (i < filteredInstalledMods.Count)
                installedModsToDisplay.Add(filteredInstalledMods[i]);
              else
                break;
            }

            List_InstalledMods(installedModsToDisplay);

          }
          break;
      }
    }

    public void OnInstallModSortDropDownChanged(int sortOption) {

      switch (sortOption) {
        case 0:
          InstalledMods_SelectedClassTab.installedMods =
          InstalledModsSorter.SortByLatestInstalled(InstalledMods_SelectedClassTab.installedMods);
          break;
        case 1:
          InstalledMods_SelectedClassTab.installedMods =
          InstalledModsSorter.SortByModName(InstalledMods_SelectedClassTab.installedMods);
          break;
        case 2:
          InstalledMods_SelectedClassTab.installedMods =
          InstalledModsSorter.SortByAuthorName(InstalledMods_SelectedClassTab.installedMods);
          break;
      }
      InstalledMods_SelectedClassTab.Select
        (InstalledMods_SelectedClassTab.currentPageIndexInInstalledMods);
    }

    #region Error/Alert

    private void ManageError(CFCoreError error, Action retryAction) {
      if (Application.internetReachability == NetworkReachability.NotReachable
        || (error.IsError && error.ApiError.ServerUnreachable)) {

        if (retryAction != null) {
          ConectionErrorObject?.SetActive(true);
          Button button = ConectionErrorObject.GetComponentInChildren<Button>();
          if (button != null) {
            button.onClick.AddListener(delegate {
              retryAction();
              button.onClick.RemoveAllListeners();
            });
            return;
          }
        }

        ToastManager.Instance.CreateErrorToast(
         "An error has occurred. Please check your connection and try again");
        return;
      }

      if (error.IsError)
        ToastManager.Instance.CreateErrorToast(error.Description, null);
    }


    /// <summary>
    /// Popup Error/Alert Dialog
    /// </summary>
    /// <param name="errorStr"></param>
    public virtual void Popup_Error(string errorStr) {
      GameObject alertPopup = Instantiate(PopupAlertPrefab, transform) as GameObject;
      alertPopup.GetComponentInChildren<TextMeshProUGUI>().text = errorStr;
    }
    #endregion

    #region Input Controller

    private void SetupNavigation() {
      InitializeDropDownSelectables();
      SetupModSelectables();
      SetupDropdowns();
      SelectFirstModInPage();
    }

    private void SetupModSelectables() {
      currentModItemsSelectables = new List<Selectable>();
      int firstIndexInSecondRow = ((int)modsPerPage) / 2;


      for (int i = 0; i < currentModItems.Count; i++) {
        Selectable selectable = currentModItems[i].transform.GetComponentInChildren<Selectable>();
        currentModItemsSelectables.Add(selectable);
      }

      for (int i = 0; i < currentModItems.Count; i++) {

        Navigation navigation = currentModItemsSelectables[i].navigation;
        navigation.mode = Navigation.Mode.Automatic;
        //if (i > 0) {
        //  navigation.selectOnLeft = currentModItemsSelectables[i - 1];
        //} else navigation.selectOnLeft = NavigationManager.Instance.GetFirstButton();

        //if (i < currentModItems.Count - 1) {
        //  navigation.selectOnRight = currentModItemsSelectables[i + 1];
        //}

        //Selectable dropDownSelectable = null;
        //switch (NavigationManager.Instance.SelectedNavigationSection) {
        //  case NavigationSection.BrowseMods:
        //      dropDownSelectable = Selectable_BrowseModsSortField;
        //      break;
        //  case NavigationSection.InstalledMods:
        //    dropDownSelectable = Selectable_InstalledMods_SortField;
        //    break;
        //}

        //navigation.selectOnUp =
        //  (i < firstIndexInSecondRow) ? dropDownSelectable : currentModItemsSelectables[i - firstIndexInSecondRow];

        //int downIndex = i + firstIndexInSecondRow;
        //if ((i <= firstIndexInSecondRow) && currentModItemsSelectables.Count > downIndex) {
        //  navigation.selectOnDown = currentModItemsSelectables[downIndex];
        //} else {
        //  navigation.selectOnDown = null;
        //}

        //currentModItemsSelectables[i].navigation = navigation;

      }
    }

    private void SetupDropdowns() {

      if (currentModItemsSelectables.Count == 0)
        return;

      switch (NavigationManager.Instance.SelectedNavigationSection) {
        case NavigationSection.BrowseMods: {

            Navigation navigation = Selectable_BrowseModsSortField.navigation;
            navigation.selectOnDown = currentModItemsSelectables[0];
            Selectable_BrowseModsSortField.navigation = navigation;

            navigation = Selectable_BrowseModsCategoryFilter.navigation;
            navigation.selectOnDown = currentModItemsSelectables[0];
            Selectable_BrowseModsCategoryFilter.navigation = navigation;

            break;
          }
        case NavigationSection.InstalledMods: {
            Navigation navigation = Selectable_InstalledMods_SortField.navigation;
            navigation.selectOnDown = currentModItemsSelectables[0];
            Selectable_InstalledMods_SortField.navigation = navigation;

            break;
          }
      }
    }

    private void InitializeDropDownSelectables() {
      if (Selectable_BrowseModsSortField == null) {
        Selectable_BrowseModsSortField =
          DropDown_BrowseModsSortField.transform.GetComponentInChildren<Selectable>();
      }

      if (Selectable_BrowseModsCategoryFilter == null) {
        Selectable_BrowseModsCategoryFilter =
          DropDown_BrowseModsCategoryFilter.transform.GetComponentInChildren<Selectable>();
      }

      if (Selectable_InstalledMods_SortField == null) {
        Selectable_InstalledMods_SortField =
          DropDown_InstalledMods_SortField.transform.GetComponentInChildren<Selectable>();
      }
    }

    private void SelectFirstModInPage() {
      if (currentModItemsSelectables == null || currentModItemsSelectables.Count == 0)
        return;
      currentModItemsSelectables[0].GetComponent<Selectable>().Select();
    }

    #endregion

    public void Quit() {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#else
      OnQuit.Invoke();
#endif
    }
  }
}

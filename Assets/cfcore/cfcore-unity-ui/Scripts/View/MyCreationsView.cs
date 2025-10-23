using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System;
using Overwolf.CFCore.Base.Creation.Models;
using Overwolf.CFCore.UnityUI.UIItems;
using Overwolf.CFCore.Base.Api.Models;
using Assets.Scripts.Utils;
using Overwolf.CFCore.UnityUI.Managers;
using Overwolf.CFCore.Base.Api.Models.Filters;
using System.Linq;
using Unity.VisualScripting;

namespace Overwolf.CFCore.UnityUI.View {
  public class MyCreationsView : MonoBehaviour {
    private List<CreationItem> _creations = new List<CreationItem>();
    private List<CreationItem> _filteredCreations = new List<CreationItem>();
    private string currentSearchedString = "";

    public const int TILES_PER_PAGE = 11;
#pragma warning disable 0649 // allocated in Prefab
    [SerializeField]
    private GameObject CreationTilePrefab;
    [SerializeField]
    private Transform TransformBrowseCreationPanel;
    private int currentPageNumber;
    [SerializeField]
    private Button NextPageButton;
    [SerializeField]
    private Button PrevPageButton;
    [SerializeField]
    private TextMeshProUGUI PageNumberText;
    [SerializeField]
    private Texture AscendingTexture;
    [SerializeField]
    private Texture DescendingTexture;

    [SerializeField]
    private GameObject SigninRequiredGameObject;
    [SerializeField]
    private GameObject NoResultGameObject;
    [SerializeField]
    private GameObject BrowseCreationsGameObject;

    [SerializeField]
    private TMP_Dropdown Drp_Class;
    [SerializeField]
    private TMP_Dropdown Drp_Category;
#pragma warning restore 0649

    private List<Category> Classes;
    private Dictionary<uint, List<Category>> categoryDictionary;
    private Category selectedClass;
    private Category selectedCategory;

    private int lastPage;

    private const string kDefaultClassFilterDropdownText = "All mods";
    private const string kDefaultCategoryFilterDropdownText = "All Categories";

    private static MyCreationsView _instance;

    public static MyCreationsView Instance {
      get {
        return _instance;
      }
    }

    public enum SortingStatus {
      Disabled = 0,
      Ascending = 1,
      Descending = 2,
    }

#pragma warning disable 0649 // allocated in Prefab
    [Serializable]
    private struct SortingButton {
      public int Id;
      public RawImage arrowImage;
      public SortingStatus status;
    }
    [SerializeField]
    private List<SortingButton> SortingButtons;
#pragma warning restore 0649

    public void Initialize() {
      UpdateCreationViewState();
      UnsubscribeToEvents();// makes sure we dont double subscribe
      SubscribeToEvents();
    }

    public void Initialize(List<CreationItem> creations) {
      _instance = this;
      _creations = creations;
      _filteredCreations = _creations;
      selectedCategory = null;
      selectedClass = null;
      Drp_Class.SetValueWithoutNotify(0);

      UnsubscribeToEvents();// makes sure we dont double subscribe
      SubscribeToEvents();
      creations.RemoveAll(c => c.Mod.Status == Base.Api.Models.Enums.ModStatus.Deleted);

      UpdateCreationViewState();
      ResetView();
      Classes = new List<Category>();
      categoryDictionary = new Dictionary<uint, List<Category>>();
      categoryDictionary.Add(0, new List<Category>());
      SetupCategories();
      SelectComponentOnView();
    }

    private void ResetView() {
      currentPageNumber = 0;

      ClearTiles();
      CreateTiles();
      CalculateLastPage();
      UpdatePageNavigation();
    }

    private void SubscribeToEvents() {
      Managers.READ_APIManager.LogoutSuccessEvent += UpdateCreationViewState;
    }

    private void UnsubscribeToEvents() {
      Managers.READ_APIManager.LogoutSuccessEvent -= UpdateCreationViewState;
    }

    public void ClearTiles() {
      for (int i = 0; i < TransformBrowseCreationPanel.childCount; i++) {
        Destroy(TransformBrowseCreationPanel.GetChild(i).gameObject);
      }
    }

    public void ClearView() {
      SigninRequiredGameObject.SetActive(false);
      NoResultGameObject.SetActive(false);
      BrowseCreationsGameObject.SetActive(false);
    }

    public void LoadPage(bool isNextPage) {
      if (isNextPage) {
        currentPageNumber++;
      } else {
        currentPageNumber--;
      }

      UpdatePageNavigation();
      ClearTiles();
      CreateTiles();
    }
    public virtual void UpdatePageNavigation() {
      PrevPageButton.interactable = currentPageNumber != 0;
      NextPageButton.interactable = currentPageNumber != lastPage;
      PageNumberText.text = string.Format("Page {0} of {1}",
                                           currentPageNumber + 1, lastPage + 1);
    }

    private void UpdateCreationViewState() {
      if (READ_APIManager.Instance.IsSignedIn) {
        SigninRequiredGameObject.SetActive(false);
        NoResultGameObject.SetActive(_filteredCreations.Count == 0);
        BrowseCreationsGameObject.SetActive(_filteredCreations.Count != 0);

      } else {
        SigninRequiredGameObject.SetActive(true);
        NoResultGameObject.SetActive(false);
        BrowseCreationsGameObject.SetActive(false);
      }
    }

    public void DeleteMod(Mod modToDelete) {
      CreationItem creation = _creations.Find(c => c.Mod.Id == modToDelete.Id);
      if (creation != null) {
        _creations.Remove(creation);

         creation = _filteredCreations.Find(c => c.Mod.Id == modToDelete.Id);
        if (creation != null)
          _filteredCreations.Remove(creation);

          UpdateCreationViewState();
        ClearTiles();
        CreateTiles();

        CalculateLastPage();
        if (currentPageNumber > lastPage) {
          currentPageNumber = lastPage;
        }
        UpdatePageNavigation();
      }
    }

    private void CreateTiles() {
      for (int i = currentPageNumber * TILES_PER_PAGE; i < _filteredCreations.Count && i < (currentPageNumber + 1) * TILES_PER_PAGE; i++) {
        GameObject go = GameObject.Instantiate(CreationTilePrefab, TransformBrowseCreationPanel);
        go.GetComponentInChildren<MyCreationTile>().Initialize(_filteredCreations[i]);
      }
    }

    private void CalculateLastPage() {
      if (_filteredCreations.Count == 0) {
        lastPage = 0;
        return;
      }

      if (_filteredCreations.Count % TILES_PER_PAGE == 0)
        lastPage = _filteredCreations.Count / TILES_PER_PAGE - 1;
      else
        lastPage = _filteredCreations.Count / TILES_PER_PAGE;
    }


    public void Search(string searchString,bool shouldSkipFilter=false, bool shouldResetView=true) {
      List<CreationItem> tempList = new List<CreationItem>();
      currentSearchedString = searchString;
      if (searchString == "") {
        tempList = _creations;
      }
      else {
        foreach (CreationItem creation in _creations) {
          if (creation.Mod.Name.Contains(searchString))
            tempList.Add(creation);
        }
      }

      _filteredCreations = tempList;

      if (!shouldSkipFilter) {
        if (selectedClass != null) {
          _filteredCreations = FilterByClass(selectedClass.Id, tempList);
          shouldResetView = false;
        }

        if (selectedCategory != null) {
          _filteredCreations = FilterByCategory(selectedCategory.Id);
          shouldResetView = false;
        }
      }

      if (shouldResetView) {
        ResetView();
      }
    }

    /// <summary>
    /// Select the first mod in the table if available, otherwise filter dropdown.
    /// </summary>
    private void SelectComponentOnView() {
      var selectableComponents = transform.GetComponentsInChildren<Selectable>();

      var creation = selectableComponents.FirstOrDefault(
        item => item.TryGetComponent<MyCreationTile>(out _));

      if (creation != null) {
        creation.Select();
        return;
      }

      var componentPriority = new string[] { "Dropdown_Classes", "Dropdown_Categories" };
      foreach (var componentName in componentPriority) {
        var component = selectableComponents.FirstOrDefault(
          item => item.name == componentName);
        if (component != null) {
          component.Select();
          return;
        }
      }

      Debug.LogWarning("Could not find a component to select");
    }

    #region Filter


    private void SetupCategories() {
      // get all the classes
      READ_APIManager.Instance.Request_GetCategoriesOfClass(
         new GetCategoriesFilter(), (categoryList) => {
           foreach (Category category in categoryList) {
             if (category.IsClass != null && category.IsClass.Value) {
               Classes.Add(category);
               categoryDictionary.Add(category.Id, new List<Category>());
             }
           }
           // create a dictionary between the class and its categories
           foreach (Category category in categoryList) {
             if ((category.IsClass == null || !category.IsClass.Value) && // not a class
                 (category.ClassId != null)) {  // has a parent class
               if (!categoryDictionary.ContainsKey(category.ClassId.Value)) {
                 Debug.LogError(string.Format("Error : Discrepancy  in database." +
                   " Class {0} doesn't exist although {1} has it as a class",
                   category.ClassId, category.Id));
               } else {
                 categoryDictionary[category.ClassId.Value].Add(category);
               }
             }
           }
           SetupDropDowns();
         });
    }

    private void SetupDropDowns() {
      Drp_Class.ClearOptions();
      List<string> options = new List<string> {
        kDefaultClassFilterDropdownText
      };
      foreach (var categoryClass in Classes) {
        options.Add(categoryClass.Name);
      }

      Drp_Class.AddOptions(options);
      if (selectedClass != null) {
        for (int i = 0; i < Classes.Count; i++) {
          if (selectedClass.Id == Classes[i].Id) {
            Drp_Class.SetValueWithoutNotify(i + 1);
          }
        }
      } else {
        Drp_Class.SetValueWithoutNotify(0);
      }

      if (Classes.Count == 1) {
        Drp_Class.Hide();
      }

      Drp_Class.RefreshShownValue();
      UpdateCategoriesDropdown();

      if (selectedCategory != null) {
        for (int i = 0; i < categoryDictionary[selectedClass.Id].Count; i++) {
          if (selectedCategory.Id == categoryDictionary[selectedClass.Id][i].Id) {
            Drp_Category.SetValueWithoutNotify(i + 1);
            Drp_Category.RefreshShownValue();
          }
        }
      }
    }

    private void UpdateCategoriesDropdown() {
      Drp_Category.ClearOptions();
      List<string> categoryNames = new List<string> {
        kDefaultCategoryFilterDropdownText
      };
      if (selectedClass == null) {
        Drp_Category.AddOptions(categoryNames);
        Drp_Category.SetValueWithoutNotify(0);
        Drp_Category.RefreshShownValue();
        return;
      }
      foreach (Category cat in categoryDictionary[selectedClass.Id]) {
        categoryNames.Add(cat.Name);
      }
      Drp_Category.AddOptions(categoryNames);
      Drp_Category.RefreshShownValue();
    }

    public void ClassDropDownValueChanged(TMP_Dropdown change) {
      if (currentSearchedString != "") {
        Search(currentSearchedString,true);
      }

      if (change.value == 0) {
        selectedClass = null;
        Search(currentSearchedString,true);
        ResetView();
      } else {
        selectedClass = Classes[change.value - 1];
        FilterByClass(selectedClass.Id);
      }
      UpdateCategoriesDropdown();
    }

    public void CategoryDropDownValueChanged(TMP_Dropdown change) {

      if (change.value == 0) {
        selectedCategory = null;
        FilterByClass(selectedClass.Id);
      } else {
        selectedCategory = categoryDictionary[selectedClass.Id][change.value - 1];
        FilterByClass(selectedClass.Id);
        FilterByCategory(selectedCategory.Id);
      }
    }

    private List<CreationItem> FilterByClass(uint classId, List<CreationItem> overidedList = null) {
      if (overidedList == null) {
        overidedList = _creations;
      }
      Search(currentSearchedString, true);
      overidedList =new List<CreationItem>( _filteredCreations);
      _filteredCreations.Clear();
      foreach (CreationItem creation in overidedList) {
        if (creation.Mod.ClassId == classId) {
          _filteredCreations.Add(creation);
        }
      }
      ResetView();
      return _filteredCreations;
    }

    private List<CreationItem> FilterByCategory(uint categoryId) {
      List<CreationItem> tempList = new List<CreationItem>();

      foreach (CreationItem creation in _filteredCreations) {
        if (creation.Mod.CategoryIds.Any(cat => cat == categoryId) ||
            creation.Mod.PrimaryCategoryId == categoryId) {
          tempList.Add(creation);
        }
      }
      _filteredCreations = tempList;
      ResetView();
      return _filteredCreations;
    }

    #endregion Filter


    #region Sorting

    public void OnSortingButtonPress(int buttonId) {
      for (int i = 0; i < SortingButtons.Count; i++) {
        SortingButton tempButton = SortingButtons[i];
        if (SortingButtons[i].Id == buttonId) {
          tempButton.arrowImage.enabled = true;
          if (SortingButtons[i].status == SortingStatus.Ascending) {
            tempButton.status = SortingStatus.Descending;
            tempButton.arrowImage.texture = DescendingTexture;
          } else {
            tempButton.status = SortingStatus.Ascending;
            tempButton.arrowImage.texture = AscendingTexture;
            SortingButtons[i] = tempButton;
          }
          SortingButtons[i] = tempButton;
        } else {
          tempButton.status = SortingStatus.Disabled;
          tempButton.arrowImage.enabled = false;
          SortingButtons[i] = tempButton;
        }
      }
    }

    public void SortByName(int buttonId) {
      if (SortingButtons.Find((bu) => bu.Id == buttonId).status == SortingStatus.Ascending)
        _filteredCreations.Sort((a, b) => string.Compare(a.Mod.Name, b.Mod.Name));

      else
        _filteredCreations.Sort((a, b) => string.Compare(b.Mod.Name, a.Mod.Name));

      ClearTiles();
      CreateTiles();
    }

    public void SortByDateCreated(int buttonId) {
      if (SortingButtons.Find((bu) => bu.Id == buttonId).status == SortingStatus.Ascending)
        _filteredCreations.Sort((a, b) => DateTime.Compare(a.Mod.DateCreated, b.Mod.DateCreated));
      else
        _filteredCreations.Sort((a, b) => DateTime.Compare(b.Mod.DateCreated, a.Mod.DateCreated));

      ClearTiles();
      CreateTiles();
    }

    public void SortByDateModified(int buttonId) {
      if (SortingButtons.Find((bu) => bu.Id == buttonId).status == SortingStatus.Ascending)
        _filteredCreations.Sort((a, b) => DateTime.Compare(a.Mod.DateModified, b.Mod.DateModified));
      else
        _filteredCreations.Sort((a, b) => DateTime.Compare(b.Mod.DateModified, a.Mod.DateModified));

      ClearTiles();
      CreateTiles();
    }

    public void SortByLikes(int buttonId) {
      if (SortingButtons.Find((bu) => bu.Id == buttonId).status == SortingStatus.Ascending)
        _filteredCreations.Sort((a, b) => a.Mod.ThumbsUpCount.Value.CompareTo(b.Mod.ThumbsUpCount.Value));
      else
        _filteredCreations.Sort((a, b) => b.Mod.ThumbsUpCount.Value.CompareTo(a.Mod.ThumbsUpCount.Value)); ;
      ClearTiles();
      CreateTiles();
    }

    public void SortByDownloads(int buttonId) {
      if (SortingButtons.Find((bu) => bu.Id == buttonId).status == SortingStatus.Ascending)
        _filteredCreations.Sort((a, b) => a.Mod.DownloadCount.CompareTo(b.Mod.DownloadCount));
      else
        _filteredCreations.Sort((a, b) => b.Mod.DownloadCount.CompareTo(a.Mod.DownloadCount));
      ClearTiles();
      CreateTiles();
    }

    public void SortByStatus(int buttonId) {


      if (SortingButtons.Find((bu) => bu.Id == buttonId).status == SortingStatus.Ascending)
        _filteredCreations.Sort((a, b) => CreationUtils.GetCombinedCreationStatus(a).
        CompareTo(CreationUtils.GetCombinedCreationStatus(b)));
      else
        _filteredCreations.Sort((a, b) => CreationUtils.GetCombinedCreationStatus(b).
        CompareTo(CreationUtils.GetCombinedCreationStatus(a)));
      ClearTiles();
      CreateTiles();
    }
    #endregion Sorting
  }
}

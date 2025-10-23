using Overwolf.CFCore.Base.Api.Common;
using Overwolf.CFCore.Base.Api.Models;
using Overwolf.CFCore.Base.Api.Models.Enums;
using Overwolf.CFCore.Base.Api.Models.Filters;
using Overwolf.CFCore.UnityUI.Managers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Overwolf.CFCore.UnityUI.UIItems {
  public class ModSearchCategoryFilter : MonoBehaviour {
    private static ModSearchCategoryFilter _instance;

    public static ModSearchCategoryFilter Instance {
      get {
        if (_instance == null)
          _instance = new ModSearchCategoryFilter();
        return _instance;
      }
    }

    TMP_Dropdown CategoryDropdown;

    private void Awake() {
      _instance = this;
    }

    private void Start() {
      CategoryDropdown = GetComponent<TMP_Dropdown>();
    }

    public void ListCategories(List<Category> categories) {
      //Clear old values from Dropdown
      CategoryDropdown.options.RemoveRange(1, CategoryDropdown.options.Count - 1);

      UIManager.Instance.BrowseMods_SelectedClassTab.categoryIDList.Clear();

      List<string> categoryNames = new List<string>();
      for (int i = 0; i < categories.Count; i++) {
        //Check if the parent category ID equals to the current selected class ID
        if (categories[i].ParentCategoryId.Equals(UIManager.Instance.BrowseMods_SelectedClassTab.category.Id)) {
          categoryNames.Add(categories[i].Name);
          UIManager.Instance.BrowseMods_SelectedClassTab.categoryIDList.Add(categories[i].Id);
        }
      }

      CategoryDropdown.AddOptions(categoryNames);

      if (UIManager.Instance.BrowseMods_SelectedClassTab.categoryFilter == null) {
        CategoryDropdown.SetValueWithoutNotify(0);
      } else {
        int index = -1;
        for (int i = 0; i < UIManager.Instance.BrowseMods_SelectedClassTab.categoryIDList.Count; i++) {
          if (UIManager.Instance.BrowseMods_SelectedClassTab.categoryIDList[i].Equals(UIManager.Instance.BrowseMods_SelectedClassTab.categoryFilter)) {
            index = i;
            break;
          }
        }
        if (index != -1)
          CategoryDropdown.SetValueWithoutNotify(index + 1);
        else
          Debug.LogError("Error");
      }
    }

    public void FilterChanged(int dropdownValue) {
      //Search mods of selected category
      SearchModsFilter searchModsFilter = new SearchModsFilter();
      searchModsFilter.ClassId = UIManager.Instance.BrowseMods_SelectedClassTab.category.Id;
      if (dropdownValue != 0)
        UIManager.Instance.BrowseMods_SelectedClassTab.categoryFilter = UIManager.Instance.BrowseMods_SelectedClassTab.categoryIDList[dropdownValue - 1];
      else
        UIManager.Instance.BrowseMods_SelectedClassTab.categoryFilter = null;
      searchModsFilter.CategoryId = UIManager.Instance.BrowseMods_SelectedClassTab.categoryFilter;
      searchModsFilter.SortField = UIManager.Instance.BrowseMods_SelectedClassTab.searchSortField;
      if (searchModsFilter.SortField == ModsSearchSortField.Name || searchModsFilter.SortField == ModsSearchSortField.Author)
        searchModsFilter.SortOrder = SortOrder.asc;
      else
        searchModsFilter.SortOrder = SortOrder.desc;
      searchModsFilter.SearchFilter = NavigationManager.Instance.InputField_Search.text;

      ApiRequestPagination pagination = new ApiRequestPagination();
      pagination.Index = 0;
      pagination.PageSize = UIManager.Instance.ModsPerPage;

      UIManager.Instance.BrowseMods(searchModsFilter, pagination);
    }
  }
}

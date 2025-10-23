using Assets.Scripts.Utils;
using Overwolf.CFCore.Base.Api.Common;
using Overwolf.CFCore.Base.Api.Models.Enums;
using Overwolf.CFCore.Base.Api.Models.Filters;
using Overwolf.CFCore.UnityUI.Managers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Overwolf.CFCore.UnityUI.UIItems {
  public class ModSearchSortor : MonoBehaviour {
    private static ModsSearchSortField[] kSearchSortFieldsToIgnore = new ModsSearchSortField[]
    {
        ModsSearchSortField.Category,
        ModsSearchSortField.GameVersion,
        ModsSearchSortField.LastUpdated
    };

    private Dictionary<int, ModsSearchSortField> _dropdownIndexToSortField;
    private Dictionary<ModsSearchSortField, int> dropdownSortFieldToIndex;

    void Start() {
      GetComponent<TMP_Dropdown>().ClearOptions();

      _dropdownIndexToSortField = new Dictionary<int, ModsSearchSortField>();
      dropdownSortFieldToIndex = new Dictionary<ModsSearchSortField, int>();
      List<string> sortOptions = new List<string>();

      foreach (ModsSearchSortField value in Enum.GetValues(typeof(ModsSearchSortField))) {
        if (!Array.Exists(kSearchSortFieldsToIgnore, i => i == value)) {
          string prettyName = StringUtils.ToSentenceCase(Enum.GetName(typeof(ModsSearchSortField), value));
          sortOptions.Add(prettyName);

          _dropdownIndexToSortField[_dropdownIndexToSortField.Count] = value;
          dropdownSortFieldToIndex.Add(value, _dropdownIndexToSortField.Count);
        }
      }

      GetComponent<TMP_Dropdown>().AddOptions(sortOptions);
    }

    public int GetDropDownIndex(ModsSearchSortField modsSearchSortField) {
      return (dropdownSortFieldToIndex[modsSearchSortField]);
    }

    public void SortFieldChanged(int sortValue) {
      if (UIManager.Instance.BrowseMods_SelectedClassTab == null)
        return;
      SearchModsFilter searchModsFilter = new SearchModsFilter();
      searchModsFilter.ClassId = UIManager.Instance.BrowseMods_SelectedClassTab.category.Id;
      searchModsFilter.CategoryId = UIManager.Instance.BrowseMods_SelectedClassTab.categoryFilter;
      searchModsFilter.SortField = _dropdownIndexToSortField[sortValue];

      if (searchModsFilter.SortField == ModsSearchSortField.Name || searchModsFilter.SortField == ModsSearchSortField.Author)
        searchModsFilter.SortOrder = SortOrder.asc;
      else
        searchModsFilter.SortOrder = SortOrder.desc;

      searchModsFilter.SearchFilter = NavigationManager.Instance.InputField_Search.text;

      ApiRequestPagination pagination = new ApiRequestPagination();
      pagination.Index = 0;
      pagination.PageSize = UIManager.Instance.ModsPerPage;

      UIManager.Instance.BrowseMods_SelectedClassTab.searchSortField = _dropdownIndexToSortField[sortValue];
      UIManager.Instance.BrowseMods_SelectedClassTab.currentPageIndexInBrowseMods = 0;

      UIManager.Instance.BrowseMods(searchModsFilter, pagination);
    }
  }
}

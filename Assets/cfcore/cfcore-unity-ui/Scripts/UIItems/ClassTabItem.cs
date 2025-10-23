using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Overwolf.CFCore.Base.Api.Models;
using Overwolf.CFCore.Base.Api.Models.Enums;
using Overwolf.CFCore.Base.Library.Models;
using Overwolf.CFCore.Base.Api.Models.Filters;
using Overwolf.CFCore.Base.Api.Common;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Overwolf.CFCore.UnityUI.Managers;
using Overwolf.CFCore.UnityUI.Themes;

namespace Overwolf.CFCore.UnityUI.UIItems {

  public class ClassTabItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public bool isSelected = false;
    public Category category;
    public TextMeshProUGUI Txt_ClassName;
    public GameObject SelectedLine;
    public List<uint> categoryIDList;

    //Browse Mods
    public ModsSearchSortField searchSortField = ModsSearchSortField.Featured;
    public uint? categoryFilter = null; //All Categories
    public uint currentPageIndexInBrowseMods = 0;
    public uint totalPageCountInBrowseMods = 0;

    //Installed Mods
    public List<InstalledMod> installedMods = new List<InstalledMod>();
    public int currentPageIndexInInstalledMods = 0;
    public int totalPageCountInInstalledMods = 0;

    public void ImplementUpdateInstalledModsList(List<InstalledMod> newList ) {
      installedMods = new List<InstalledMod>(newList);
      Select(currentPageIndexInInstalledMods);
    }

    public virtual void SetupItem(Category data) {
      SetupTheme();
      category = data;

      if (NavigationManager.Instance.SelectedNavigationSection == NavigationSection.BrowseMods) {
        Txt_ClassName.text = category.Name;
      } else {
        installedMods.Clear();
        for (int i = 0; i < READ_APIManager.Instance.InstalledModList.Count; i++) {
          if (READ_APIManager.Instance.InstalledModList[i].Details.ClassId == category.Id)
            installedMods.Add(READ_APIManager.Instance.InstalledModList[i]);
        }

        Txt_ClassName.text = category.Name + "(" + installedMods.Count + ")";
      }

      float width = LayoutUtility.GetPreferredWidth(Txt_ClassName.GetComponent<RectTransform>());
      GetComponent<RectTransform>().sizeDelta = new Vector2(width + 36f, 40);
    }

    public void Select(int staringPage = 0) {
      isSelected = true;
      Txt_ClassName.color = new Color(0.898f, 0.898f, 0.898f);
      SelectedLine.SetActive(isSelected);

      for (int i = 0; i < transform.parent.childCount; i++) {
        if (transform.parent.GetChild(i).Equals(transform))
          continue;
        transform.parent.GetChild(i).GetComponent<ClassTabItem>().UnSelect();
      }

      if (NavigationManager.Instance.SelectedNavigationSection == NavigationSection.BrowseMods) {
        UIManager.Instance.BrowseMods_SelectedClassTab = GetComponent<ClassTabItem>();
        ModSearchSortor sortor = UIManager.Instance.DropDown_BrowseModsSortField.transform.GetComponentInChildren<ModSearchSortor>();
        UIManager.Instance.DropDown_BrowseModsSortField.SetValueWithoutNotify(sortor.GetDropDownIndex(searchSortField)-1);


        //Load Mods of the selected Class ID
        SearchModsFilter searchModsFilter = new SearchModsFilter();
        searchModsFilter.ClassId = category.Id;
        searchModsFilter.CategoryId = categoryFilter;
        searchModsFilter.SortField = searchSortField;
        if (searchModsFilter.SortField == ModsSearchSortField.Name || searchModsFilter.SortField == ModsSearchSortField.Author)
          searchModsFilter.SortOrder = SortOrder.asc;
        else
          searchModsFilter.SortOrder = SortOrder.desc;
        searchModsFilter.SearchFilter = NavigationManager.Instance.InputField_Search.text;

        ApiRequestPagination pagination = new ApiRequestPagination();
        pagination.Index =(uint) (currentPageIndexInBrowseMods * UIManager.Instance.ModsPerPage);
        pagination.PageSize = UIManager.Instance.ModsPerPage;

        UIManager.Instance.BrowseMods(searchModsFilter, pagination);

        //Load Categories of the selected Class
        GetCategoriesFilter getCategoriesFilter = new GetCategoriesFilter();
        getCategoriesFilter.ClassId = category.Id;

        READ_APIManager.Instance.Request_GetCategoriesOfClass(getCategoriesFilter,
          (categories)=> {
            if (getCategoriesFilter.ClassId != null) {
              ModSearchCategoryFilter.Instance.ListCategories(categories);
            }
          }
        );
      } else if (NavigationManager.Instance.SelectedNavigationSection == NavigationSection.InstalledMods) {
        UIManager.Instance.InstalledMods_SelectedClassTab = GetComponent<ClassTabItem>();
        currentPageIndexInInstalledMods = staringPage;
        List<InstalledMod> installedModsToDisplay = new List<InstalledMod>();

        for (int i = (int)(currentPageIndexInInstalledMods * UIManager.Instance.ModsPerPage); i < (currentPageIndexInInstalledMods + 1) * UIManager.Instance.ModsPerPage; i++) {
          if (i < installedMods.Count)
            installedModsToDisplay.Add(installedMods[i]);
          else
            break;
        }

        UIManager.Instance.List_InstalledMods(installedModsToDisplay,true);
      }
    }

    public void UnSelect() {
      isSelected = false;
      SelectedLine.SetActive(isSelected);
      Txt_ClassName.color = new Color(0.6f, 0.6f, 0.6f);
    }

    public void OnPointerEnter(PointerEventData eventData) {
      if (!isSelected)
        Txt_ClassName.color = new Color(0.898f, 0.898f, 0.898f);
    }

    public void OnPointerExit(PointerEventData eventData) {
      if (!isSelected)
        Txt_ClassName.color = new Color(0.6f, 0.6f, 0.6f);
    }

    private void SetupTheme() {
      var theme = EternalUITheme.Instance;
      if (theme != null) {
        if (theme.ColorTheme != null) {
          var setters = GetComponentsInChildren<EternalColorSetter>(true);
          foreach (EternalColorSetter setter in setters) {
            setter.SetupColor(theme.ColorTheme);
          }
        }

        if (theme.ShapeTheme != null) {
          var setters = GetComponentsInChildren<EternalShapeSetter>(true);
          foreach (EternalShapeSetter setter in setters) {
            setter.SetupShape(theme.ShapeTheme);
          }
        }
      }
    }
  }
}

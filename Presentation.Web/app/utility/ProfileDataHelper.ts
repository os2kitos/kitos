module Kitos.Utility {
    export class ProfileDataHelper {

        static saveProfileLocalStorageData($window, orgUnitStorageKey) {
            const currentOrgUnitId = $window.sessionStorage.getItem(orgUnitStorageKey);
            if (currentOrgUnitId == null) {
                return;
            }
            $window.localStorage.setItem(orgUnitStorageKey + "-profile", currentOrgUnitId);
        }

        static saveProfileSessionStorageData($window, $, orgUnitStorageKey, dataField) {
            const orgUnitId = $window.localStorage.getItem(orgUnitStorageKey + "-profile");
            if (orgUnitId != null) {
               $window.sessionStorage.setItem(orgUnitStorageKey, orgUnitId);
               // find the org unit filter row section
                var orgUnitFilterRow = $(".k-filter-row [data-field='" + dataField +"']");
               // find the access modifier kendo widget
               var orgUnitFilterWidget = orgUnitFilterRow.find("input").data("kendoDropDownList");
               orgUnitFilterWidget.select(dataItem => (dataItem.Id === orgUnitId));
            }

        }
    }
}
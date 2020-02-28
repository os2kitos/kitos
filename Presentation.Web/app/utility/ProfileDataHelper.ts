module Kitos.Utility {
    export class ProfileDataHelper {

        static saveProfileLocalStorageData($window, orgUnitStorageKey) {
            const currentOrgUnitId = $window.sessionStorage.getItem(orgUnitStorageKey);
            if (currentOrgUnitId == null) {
                return;
            }
            $window.localStorage.setItem(orgUnitStorageKey + "-profile", currentOrgUnitId);
        }

        static saveProfileSessionStorageData($window, orgUnitStorageKey) {
            const orgUnitId = $window.localStorage.getItem(orgUnitStorageKey + "-profile");
            if (orgUnitId == null) {
                return null;
            }
            $window.sessionStorage.setItem(orgUnitStorageKey, orgUnitId);
            return orgUnitId;
        }
    }
}
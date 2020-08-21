module Kitos.ItSystem.Settings {
    export class CatalogState {

        private static getSettingKey(key: string, userId: number) : string {
            return userId + "-" + key;
        } 

        static getShowInactiveSystems($window, userId : number) : boolean {
            return $window.localStorage.getItem(CatalogState.getSettingKey("showInactive", userId)) === "true";
        }

        static setShowInactiveSystems($window, userId: number, showInactive : boolean): void {
            $window.localStorage.setItem(CatalogState.getSettingKey("showInactive", userId), showInactive);
        }
    }
}
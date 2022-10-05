module Kitos.ItSystem.Settings {
    export class OverviewState {

        private static getSettingKey(key: string, userId: number, view: string) : string {
            return `${userId}-${view}-${key}`;
        }

        static getShowInactiveSystems($window, userId : number, view: string) : boolean {
            return $window.localStorage.getItem(OverviewState.getSettingKey("showInactive", userId, view)) === "true";
        }

        static setShowInactiveSystems($window, userId: number, view: string, showInactive : boolean): void {
            $window.localStorage.setItem(OverviewState.getSettingKey("showInactive", userId, view), showInactive);
        }
    }
}
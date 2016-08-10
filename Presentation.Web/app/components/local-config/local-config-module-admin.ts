module Kitos.LocalAdmin.ModuleAdmin {
    "use strict";

    export interface IX extends Kitos.Models.IEntity {
        
    }


    export class ModuleAdminController {
        public mainGrid: IKendoGrid<IX>;

        public static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$http",
            "$timeout",
            "$window",
            "$state",
            "$",
            "_",
            "moment",
            "notify",
            "projectRoles",
            "user",
            "gridStateService",
            "orgUnits",
            "economyCalc"
        ];

        constructor(
            private $rootScope: IRootScope,
            private $scope: ng.IScope,
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private $window: ng.IWindowService,
            private $state: ng.ui.IStateService,
            private $: JQueryStatic,
            private _: ILoDashWithMixins,
            private moment: moment.MomentStatic,
            private notify,
            private projectRoles,
            private user,
            private gridStateService: Services.IGridStateFactory,
            private orgUnits: any,
            private economyCalc) {
            this.$rootScope.page.title = "IT Projekt - Overblik";

            this.$scope.$on("kendoWidgetCreated", (event, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.mainGrid) {
                    this.mainGrid.dataSource.read();

                    // show loadingbar when export to excel is clicked
                    // hidden again in method exportToExcel callback
                    $(".k-grid-excel").click(() => {
                        kendo.ui.progress(this.mainGrid.element, true);
                    });
                }
            });

            this.activate();
        }

        private activate() {
             throw new Error("Not implemented");
        }
    }

    angular
        .module("app")
        .config([
            '$stateProvider', ($stateProvider: ng.ui.IStateProvider) => {
                $stateProvider.state('local-config.module-admin', {
                    url: '/org',
                    templateUrl: 'app/components/local-config/local-config-module-admin.html',
                    controller: ModuleAdminController,
                    controllerAs: 'vm',
                    resolve: {
                        organization: ['$http', 'userService', ($http: ng.IHttpService, userService) => {
                            return userService.getUser().then((user) => {
                                return $http.get<Kitos.API.Models.IApiWrapper<any>>('api/organization/' + user.currentOrganizationId).then((result) => {
                                    return result.data.response;
                                });
                            });
                        }]
                    }
                });
            }]);
}

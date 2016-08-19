module Kitos.Reports.Overview {
    'use strict';

    export class ReportsOverviewController {
        public title:string;

        public static $inject: string[] = ["reports"];
        constructor(public reports) {
            this.title = 'Så mangler vi bare nogle rapporter ...';
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
                $stateProvider.state("reports.overview", {
                    url: "/overblik",
                    templateUrl: "app/components/reports/reports-overview.html",
                    controller: ReportsOverviewController, 
                    controllerAs: "vm",
                    resolve: {
                        reports: ["$http", ($http: ng.IHttpService) => $http.get<Models.IOdataWrapper<any>>("/odata/reports?$expand=CategoryType")
                            .then(result => result.data.value)]
                    } 
                });
            }
        ]);
}
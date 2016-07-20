module Kitos.Reports.Overview {
    'use strict';

    class ReportsOverviewController {
        public title:string;
        constructor() {
            this.title = 'Så mangler vi bare nogle rapporter ...';
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("reports.overview", {
                    url: "/overblik",
                    templateUrl: "app/components/reports/reports-overview.html",
                    controller: ReportsOverviewController,
                    controllerAs: "vm"
                });
            }
        ]);
}
module Kitos.Reports.Overview {
    'use strict';

    export class ReportsOverviewController{ }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("reports.overview", {
                    url: "/reports",
                    templateUrl: "app/components/reports/reports-overview.html",
                    controller: ReportsOverviewController,
                    controllerAs: "reportsOverviewVm"
                });
            }
        ]);
}
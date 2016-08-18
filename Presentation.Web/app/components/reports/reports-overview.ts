module Kitos.Reports.Overview {
    'use strict';

    export class ReportsOverviewController {
        public title:string;

        public static $inject: string[] = ['reportService'];
        constructor(private reportService) {
            this.title = 'Så mangler vi bare nogle rapporter ...';
        }

        allreports = () => {
            (new this.reportService()).$getAll().then((data) => {
                return data;
            })
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
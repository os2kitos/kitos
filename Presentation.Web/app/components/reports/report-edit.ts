/// <reference path="../../index.d.ts" />

module Kitos.Reports {
    "use strict";

    export class EditReportController {

        public static $inject: string[] = ["$uibModalInstance", "$stateParams", "$http", "notify"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private $stateParams: ng.ui.IStateParamsService, private $http: ng.IHttpService, private notify, private user) {
            console.log($stateParams["id"]);
            console.log($stateParams["userObj"]);
        }

        public ok() {

        }

        public cancel() {
            this.$uibModalInstance.close();
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("reports.report-edit", {
                    url: "/editreport",
                    templateUrl: "app/components/reports/report-edit.html",
                    controller: EditReportController,
                    controllerAs: "vm",
                    resolve: {
                        user: [
                            "userService", userService => userService.getUser()
                        ]
                    }
                });
            }
        ]);
}

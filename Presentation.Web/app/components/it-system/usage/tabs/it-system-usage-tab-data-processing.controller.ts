module Kitos.ItSystem.Usage.Tabs {
    "use strict";

    export class ItSystemUsageTabDataProcessingController {
        static $inject: Array<string> = [
            "itSystemUsage"
        ];

        constructor(
            private readonly itSystemUsage) {

            this.dataProcessingRegistrations = this.itSystemUsage.associatedDataProcessingRegistrations;
        }

        dataProcessingRegistrations: Models.Generic.NamedEntity.NamedEntityDTO[];
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("it-system.usage.dataprocessing", {
                url: "/dataprocessing",
                templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-data-processing.view.html",
                controller: ItSystemUsageTabDataProcessingController,
                controllerAs: "vm"
            });
        }]);
}

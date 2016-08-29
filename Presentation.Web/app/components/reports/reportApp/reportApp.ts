
var reportApp = angular.module('reportApp', [
    'ui.router',
    'ui.bootstrap',
    'ngAnimate',
    'ngSanitize']);

//angular.module("reportApp").controller("stimulsoftService", Kitos.Reports.ReportViewerController);

angular.element(document).ready(() => {

    angular.module("reportApp").service("stimulsoftService", Kitos.Services.StimulsoftService);

    angular
        .module("reportApp")
        .config(["$stateProvider", ($stateProvider) => {
            $stateProvider.state("reports-viewer",
                {
                    url: "/viewer",
                    templateUrl: "reportApp/reports-viewer.view.html",
                    controller: Kitos.Reports.ReportViewerController,
                    controllerAs: "vm"
                });
        }
        ]);

    angular.bootstrap(document, ["reportApp"]);
});

reportApp.run([
    '$rootScope', '$state',
    ($rootScope, $state) => {
        // init info
        $rootScope.page = {
            title: 'Index',
            subnav: []
        };

        $rootScope.$state = $state;
        $state.go("reports-viewer");
    }
]);

(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.edit.gdpr", {
            url: "/gdpr",
            templateUrl: "app/components/gdpr-views/gdpr.view.html",
            controller: "system.EditGdpr",
            resolve: {
                theSystem: ["$http", "itSystem", ($http, itSystem) => $http.get(`odata/ItSystems(${itSystem.id})?$expand=ExternalReferences($expand=ObjectOwner)`).then(result => result.data)]
            }
        });
    }]);

    app.controller("system.EditGdpr", ["$scope", "$http", "$timeout", "$state", "$stateParams", "$confirm", "notify", "hasWriteAccess", "theSystem",
        function ($scope, $http, $timeout, $state, $stateParams, $confirm, notify, hasWriteAccess, theSystem) {

            $scope.hasWriteAccess = hasWriteAccess;
            $scope.system = theSystem;
            $scope.updateUrl = 'api/itsystem/' + theSystem.Id;
        


        }]);
})(angular, app);

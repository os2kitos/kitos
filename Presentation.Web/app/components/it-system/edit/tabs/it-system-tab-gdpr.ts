(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.edit.gdpr", {
            url: "/gdpr",
            templateUrl: "app/components/gdpr-views/gdpr.view.html",
            controller: "system.EditGdpr",
            resolve: {
               // theSystem: ["$http", "itSystem", ($http, itSystem) => $http.get(`odata/ItSystems(${itSystem.id})?$expand=ExternalReferences($expand=ObjectOwner)`).then(result => result.data)],
                theSystem: ['$http', '$stateParams', function ($http, $stateParams) {
                return $http.get("api/itsystem/" + $stateParams.id)
                    .then(function (result) {
                        return result.data.response;
                    });
            }]
            }
        });
    }]);

    app.controller("system.EditGdpr", ["$scope", "$http", "$timeout", "$state", "$stateParams", "$confirm", "notify", "hasWriteAccess", "theSystem",
        function ($scope, $http, $timeout, $state, $stateParams, $confirm, notify, hasWriteAccess, theSystem) {

            $scope.hasWriteAccess = hasWriteAccess;
            $scope.system = theSystem;
            $scope.updateUrl = 'api/itsystem/' + theSystem.id;
            

            $scope.changeDataField = function (fieldName) {
                var data;

            switch (fieldName) {
                    case 'containsLegalInfo':
                     data = {
                        ContainsLegalInfo : $scope.system.containsLegalInfo
                    };
                    break;
                    case 'isDataTransferedToThirdCountries':
                     data = {
                            IsDataTransferedToThirdCountries: $scope.system.isDataTransferedToThirdCountries
                        };
                    break;
                }

                $http.patch("api/itsystem/" + theSystem.id + "?organizationId=" + theSystem.organizationId, data).success(function (result) {

                    notify.addSuccessMessage("Success");

                }).error(function (result) {
                    notify.addErrorMessage('Fejl!');
                });
            };
        }]);
})(angular, app);

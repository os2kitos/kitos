(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.edit.references", {
            url: "/reference/",
            templateUrl: "app/components/it-reference.view.html",
            controller: "system.EditReference"
        });
    }]);

    app.controller("system.EditReference",
        ["$scope", "$http", "$timeout", "$state", "$stateParams","itSystem","$confirm","notify",
            function ($scope, $http, $timeout, $state, $stateParams, itSystem,$confirm,notify) {

                $scope.objectId = itSystem.id;
                $scope.objectReference = 'it-system.edit.references.create';

                $scope.references = itSystem.externalReferences;

                $scope.deleteReference = function (referenceId) {
                    var msg = notify.addInfoMessage("Sletter...");
                    
                    $http.delete('api/Reference/' + referenceId + '?organizationId=' + itSystem.organizationId).success(() => {
                        msg.toSuccessMessage("Slettet!");
                    }).error(() => {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                        });
                    reload();
                };

                $scope.edit = function (refId) {
                    $state.go(".edit", { refId: refId, orgId: itSystem.organizationId });
                };

                function reload() {
                    $state.go(".", null, { reload: true });
                };
            }]);
})(angular, app);

(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-contract.edit.references", {
            url: "/reference/",
            templateUrl: "app/components/it-reference.view.html",
            controller: "contract.EditReference"
        });
    }]);

    app.controller("contract.EditReference",
        ["$scope", "$http", "$timeout", "$state", "$stateParams","contract","$confirm","notify",
            function ($scope, $http, $timeout, $state, $stateParams,contract,$confirm,notify) {

                $scope.objectId = contract.id;
                $scope.objectReference = 'it-contract.edit.references.create';

                $scope.references = contract.externalReferences;

                $scope.deleteReference = function (referenceId) {
                    var msg = notify.addInfoMessage("Sletter...");
                    
                    $http.delete('api/Reference/' + referenceId + '?organizationId=' + contract.organizationId).success(() => {
                        msg.toSuccessMessage("Slettet!");
                    }).error(() => {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                        });
                    reload();
                };

                $scope.edit = function (refId) {
                    $state.go(".edit", { refId: refId, orgId: contract.organizationId });
                };

                function reload() {
                    $state.go(".", null, { reload: true });
                };
            }]);
})(angular, app);

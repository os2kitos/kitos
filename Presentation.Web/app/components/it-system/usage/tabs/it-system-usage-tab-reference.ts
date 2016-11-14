(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.usage.references", {
            url: "/reference/",
            templateUrl: "app/components/it-reference.view.html",
            controller: "it-system-usage.EditReference"
        });
    }]);

    app.controller("it-system-usage.EditReference",
        ["$scope", "$http", "$timeout", "$state", "$stateParams","itSystemUsage","$confirm","notify",
            function ($scope, $http, $timeout, $state, $stateParams, itSystemUsage,$confirm,notify) {

                $scope.objectId = itSystemUsage.id;

                $scope.objectReference = 'it-system.usage.references.create';
                
                $scope.references = itSystemUsage.externalReferences;

                $scope.deleteReference = function (referenceId) {
                    var msg = notify.addInfoMessage("Sletter...");
                    
                    $http.delete('api/Reference/' + referenceId + '?organizationId=' + itSystemUsage.organizationId).success(() => {
                        msg.toSuccessMessage("Slettet!");
                    }).error(() => {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                        });
                    reload();
                };

                $scope.edit = function (refId) {
                    $state.go(".edit", { refId: refId, orgId: itSystemUsage.organizationId });
                };

                function reload() {
                    $state.go(".", null, { reload: true });
                };
            }]);
})(angular, app);

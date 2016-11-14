(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project.edit.references", {
            url: "/reference/",
            templateUrl: "app/components/it-reference.view.html",
            controller: "project.EditReference"
        });
    }]);

    app.controller("project.EditReference",
        ["$scope", "$http", "$timeout", "$state", "$stateParams","project","$confirm","notify",
            function ($scope, $http, $timeout, $state, $stateParams,project,$confirm,notify) {

                $scope.objectId = project.id;

                $scope.objectReference = 'it-project.edit.references.create';


                console.log(project);  
                $scope.references = project.externalReferences;

                $scope.deleteReference = function (referenceId) {
                    var msg = notify.addInfoMessage("Sletter...");
                    
                    $http.delete('api/Reference/' + referenceId + '?organizationId=' + project.organizationId).success(() => {
                        msg.toSuccessMessage("Slettet!");
                    }).error(() => {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                        });
                    reload();
                };

                $scope.edit = function (refId) {
                    $state.go(".edit", { refId: refId, orgId: project.organizationId });
                };

                function reload() {
                    $state.go(".", null, { reload: true });
                };
            }]);
})(angular, app);

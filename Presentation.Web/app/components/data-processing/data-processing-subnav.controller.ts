((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("data-processing", {
            url: "/data-processing",
            abstract: true,
            template: "<ui-view autoscroll=\"false\" />"
            ,
            controller: ["$rootScope", "$scope", "$state", "$http", "notify", ($rootScope, $scope, $state, $http, notify) => {
                $rootScope.page.title = "Databehandling";
                $rootScope.page.subnav = [
                    { state: "data-processing.overview", text: "Databehandleraftaler" }
                ];
                $rootScope.page.subnav.buttons = [
                    { func: remove, text: "Slet Databehandleraftale", style: "btn-danger", showWhen: "data-processing.edit-agreement" }
                ];

                $rootScope.subnavPositionCenter = false;

                $scope.$on("$viewContentLoaded", () => {
                    $rootScope.positionSubnav();
                });

                function remove() {
                    if (!confirm("Er du sikker på du vil slette Databehandleraftale?")) {
                        return;
                    }
                    var dataProcessingAgreementId = $state.params.id;
                    var msg = notify.addInfoMessage("Sletter Databehandleraftale...", false);
                    $http.delete("api/v1/data-processing-agreement/" + dataProcessingAgreementId)
                        .success(function (result) {
                            msg.toSuccessMessage("Databehandleraftale er slettet!");
                            $state.go("data-processing.overview");
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke slette Databehandleraftale!");
                        });

                }


            }]
        });
    }]);
})(angular, app);

(function(ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project", {
            url: "/project",
            abstract: true,
            template: "<ui-view autoscroll=\"false\" />",
            resolve: {
                user: ["userService", function (userService) {
                    return userService.getUser();
                }]
            },
            controller: ["$rootScope", "$http", "$state", "notify", "user", "$scope", "$timeout", function ($rootScope, $http, $state, notify, user, $scope, $timeout) {
                $rootScope.page.title = "IT Projekt";
                $rootScope.page.subnav = [
                    { state: "it-project.overview", text: "IT projekter" },
                    { state: "it-project.overview-inactive", text: "Inaktive IT projekter" }
                ];
                $rootScope.page.subnav.buttons = [
                    { func: remove, text: "Slet IT Projekt", style: "btn-danger", showWhen: "it-project.edit" }
                ];
                $rootScope.subnavPositionCenter = false;

                function remove() {
                    if (!confirm("Er du sikker på du vil slette projektet?")) {
                        return;
                    }
                    var projectId = $state.params.id;
                    var msg = notify.addInfoMessage("Sletter IT Projektet...", false);
                    $http.delete("api/itproject/" + projectId + "?organizationId=" + user.currentOrganizationId)
                        .success(function(result) {
                            msg.toSuccessMessage("IT Projektet er slettet!");
                            $state.go("it-project.overview");
                        })
                        .error(function() {
                            msg.toErrorMessage("Fejl! Kunne ikke slette IT Projektet!");
                        });
                }

                $scope.$on('$viewContentLoaded', function () {
                    $rootScope.positionSubnav();
                });
            }]
        });
    }]);
})(angular, app);

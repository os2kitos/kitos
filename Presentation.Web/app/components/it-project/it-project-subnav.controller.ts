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
            controller: ["$rootScope", "$http", "$state", "notify", "user", function($rootScope, $http, $state, notify, user) {
                $rootScope.page.title = "IT Projekt";
                $rootScope.page.subnav = [
                    { state: "it-project.overview", text: "Overblik" },
                    //{ state: "it-project.catalog", text: "IT Projekt katalog" },
                    { state: "it-project.edit", text: "IT Projekt", showWhen: "it-project.edit" },
                ];
                $rootScope.page.subnav.buttons = [
                    { func: create, text: "Opret IT Projekt", style: "btn-success", icon: "glyphicon-plus" },
                    { func: remove, text: "Slet IT Projekt", style: "btn-danger", icon: "glyphicon-minus", showWhen: "it-project.edit" }
                ];

                var orgUnitId = user.currentOrganizationUnitId;

                function create() {
                    var payload = {
                        name: "Unavngivet projekt",
                        itProjectTypeId: 1,
                        responsibleOrgUnitId: orgUnitId,
                        organizationId: user.currentOrganizationId
                    };

                    var msg = notify.addInfoMessage("Opretter projekt...", false);

                    $http.post("api/itproject", payload)
                        .success(function(result) {
                            msg.toSuccessMessage("Et nyt projekt er oprettet!");
                            var projectId = result.response.id;

                            if (orgUnitId) {
                                // add users default org unit to the new project
                                $http.post("api/itproject/" + projectId + "?organizationunit=" + orgUnitId + "&organizationId=" + user.currentOrganizationId);
                            }

                            $state.go("it-project.edit.status-project", { id: projectId });
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke oprette nyt projekt!");
                        });
                };

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
            }]
        });
    }]);
})(angular, app);

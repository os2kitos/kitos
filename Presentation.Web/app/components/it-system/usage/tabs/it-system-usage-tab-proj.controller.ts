(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.usage.proj", {
            url: "/proj",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-proj.view.html",
            controller: "system.EditProjCtrl",
            resolve: {
                user: ["userService", function (userService) {
                    return userService.getUser();
                }]
            }
        });
    }]);

    app.controller("system.EditProjCtrl", ["$scope", "$http", "$timeout", "$state", "$stateParams", "notify", "itSystemUsage", "user", function ($scope, $http, $timeout, $state, $stateParams, notify, itSystemUsage, user) {
        $scope.itProjects = itSystemUsage.itProjects;

        var usageId = $stateParams.id;
        $scope.save = function () {
            $http.post("api/itproject/" + $scope.selectedProject.id + "?usageId=" + usageId + "&organizationId=" + user.currentOrganizationId)
                .success(function () {
                    notify.addSuccessMessage("Projektet er tilknyttet.");
                    reload();
                })
                .error(function () {
                    notify.addErrorMessage("Fejl! Kunne ikke tilknytte projektet!");
                });
        };

        $scope.delete = function(projectId) {
            $http.delete("api/itproject/" + projectId + "?usageId=" + usageId + "&organizationId=" + user.currentOrganizationId)
                .success(function() {
                    notify.addSuccessMessage("Projektets tilknyttning er fjernet.");
                    reload();
                })
                .error(function() {
                    notify.addErrorMessage("Fejl! Kunne ikke fjerne projektets tilknyttning!");
                });
        };

        // work around for $state.reload() not updating scope
        // https://github.com/angular-ui/ui-router/issues/582
        function reload() {
            return $state.transitionTo($state.current, $stateParams, {
                reload: true
            }).then(function () {
                $scope.hideContent = true;
                return $timeout(function () {
                    return $scope.hideContent = false;
                }, 1);
            });
        };

        // select2 options for looking up it system usages
        $scope.itProjectsSelectOptions = {
            minimumInputLength: 1,
            initSelection: function (elem, callback) {
            },
            ajax: {
                data: function (term, page) {
                    return { query: term };
                },
                quietMillis: 500,
                transport: function (queryParams) {
                    var res = $http.get("api/itProject?orgId=" + user.currentOrganizationId + "&q=" + queryParams.data.query).then(queryParams.success);
                    res.abort = function () {
                        return null;
                    };

                    return res;
                },

                results: function (data, page) {
                    var results = [];
                    // for each system usages
                    _.each(data.data.response, function (dataWorker: { id; name; }) {
                        results.push({
                            // the id of the system usage is the id, that is selected
                            id: dataWorker.id,
                            // but the name of the system is the label for the select2
                            text: dataWorker.name
                        });
                    });
                    return { results: results };
                }
            }
        };
    }]);
})(angular, app);

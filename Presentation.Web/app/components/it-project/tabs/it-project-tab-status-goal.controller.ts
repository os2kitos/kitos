(function (ng, app) {
    app.config([
        "$stateProvider", function ($stateProvider) {
            $stateProvider.state("it-project.edit.status-goal", {
                url: "/status-goal",
                templateUrl: "app/components/it-project/tabs/it-project-tab-status-goal.view.html",
                controller: "project.EditStatusGoalCtrl",
                resolve: {
                    // re-resolve data from parent cause changes here wont cascade to it
                    project: [
                        "$http", "$stateParams", function ($http, $stateParams) {
                            return $http.get("api/itproject/" + $stateParams.id)
                                .then(function (result) {
                                    return result.data.response;
                                });
                        }
                    ],
                    goalTypes: [
                        "localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                            localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.GoalTypes).getAll()
                    ]
                }
            });
        }
    ]);

    app.controller("project.EditStatusGoalCtrl", [
        "$scope", "$http", "notify", "$uibModal", "$state", "project", "goalTypes", "user",
        function ($scope, $http, notify, $modal, $state, project, goalTypes: { Name }[], user) {
            $scope.goalStatus = project.goalStatus;
            $scope.goalStatus.updateUrl = "api/goalStatus/" + project.goalStatus.id;

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            $scope.getGoalTypeName = function (goalTypeId) {
                var type = _.find(goalTypes, { Id: goalTypeId });

                return type && type.Name;
            };

            $scope.goals = [];
            $scope.patchDate = (field, value) => {
                var date = moment(value, "DD-MM-YYYY");
                if (value === "") {
                    var payload = {};
                    payload[field] = null;
                    patch(payload, $scope.goalStatus.updateUrl + '?organizationId=' + user.currentOrganizationId);
                } else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");

                }
                else {
                    var dateString = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = dateString;
                    patch(payload, $scope.goalStatus.updateUrl + '?organizationId=' + user.currentOrganizationId);
                }
            }
            function patch(payload, url) {
                var msg = notify.addInfoMessage("Gemmer...", false);
                $http({ method: 'PATCH', url: url, data: payload })
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage("Feltet er opdateret.");
                    }, function onError(result) {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                    });
            }

            function addGoal(goal) {
                //add goals means show goal in list
                goal.show = true;

                //see if goal already in list - in that case, just update it
                var prevEntry = _.find($scope.goals, { id: goal.id });
                if (prevEntry) {
                    prevEntry = goal;
                    return;
                }

                //otherwise:

                //easy-access functions
                goal.edit = function () {
                    $state.go(".modal", { goalId: goal.id });
                };

                goal.delete = function () {
                    var msg = notify.addInfoMessage("Sletter... ");
                    $http.delete(goal.updateUrl + "?organizationId=" + user.currentOrganizationId)
                        .then(function onSuccess(result) {
                            goal.show = false;
                            msg.toSuccessMessage("Slettet!");
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke slette!");
                        });
                };

                goal.updateUrl = "api/goal/" + goal.id;
                $scope.goals.push(goal);
            }

            _.each($scope.goalStatus.goals, addGoal);
        }
    ]);
})(angular, app);

(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project.edit.risk", {
            url: "/risk",
            templateUrl: "app/components/it-project/tabs/it-project-tab-risk.view.html",
            controller: "project.EditRiskCtrl",
            resolve: {
                // re-resolve data from parent cause changes here wont cascade to it
                project: ["$http", "$stateParams", function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                risks: ["$http", "project", function ($http, project) {
                    return $http.get("api/risk/?getByProject=true&projectId=" + project.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                //returns a map with those users who have a role in this project.
                //the names of the roles is saved in user.roleNames
                usersWithRoles: ["$http", "$stateParams", function ($http, $stateParams) {
                    //get the rights of the projects
                    return $http.get("api/itprojectright/" + $stateParams.id)
                        .then(function (rightResult) {
                            var rights = rightResult.data.response;
                            //get the role names
                            return $http.get("odata/ItProjectRoles?%24format=json&%24top=100&%24orderby=priority+desc&%24count=true")
                                .then(function (roleResult) {
                                    var roles: { Name }[] = roleResult.data.value;
                                    //the resulting map
                                    var users = {};
                                    _.each(rights, function (right: { userId; user; roleId; }) {
                                        //use the user from the map if possible
                                        var user = users[right.userId] || right.user;

                                        var role = _.find(roles, { Id: right.roleId });
                                        var roleNames = user.roleNames || [];
                                        roleNames.push(role.Name);
                                        user.roleNames = roleNames;

                                        users[right.userId] = user;
                                    });

                                    return users;
                                });
                        });
                }]
            }
        });
    }]);

    app.controller("project.EditRiskCtrl", ["$scope", "$http", "$stateParams", "notify", "risks", "usersWithRoles", "user",
        function ($scope, $http, $stateParams, notify, risks, usersWithRoles, user) {

            var projectId = $stateParams.id;

            $scope.risks = [];
            $scope.usersWithRoles = _.values(usersWithRoles);

            function pushRisk(risk) {
                risk.show = true;

                risk.updateUrl = "api/risk/" + risk.id;

                $scope.risks.push(risk);
            }
            _.each(risks, pushRisk);

            $scope.product = function (risk) {
                risk.product = risk.consequence * risk.probability;
                return risk.product;
            };

            $scope.delete = function (risk) {
                $http.delete(risk.updateUrl + "?organizationId=" + user.currentOrganizationId)
                    .then(function onSuccess(result) {
                        risk.show = false;

                        notify.addSuccessMessage("Rækken er slettet");
                    }, function onError(result) {

                        notify.addErrorMessage("Fejl! Kunne ikke slette!");
                    });
            };

            $scope.averageProduct = function () {

                if ($scope.risks.length == 0) return 0;

                var sum = _.reduce($scope.risks, function (memo, risk: { product }) {
                    return memo + risk.product;
                }, 0);

                return sum / $scope.risks.length;
            };

            function resetNewRisk() {
                $scope.newRisk = {
                    consequence: 1,
                    probability: 1
                };
            }

            resetNewRisk();

            $scope.saveNewRisk = function () {
                $scope.$broadcast("show-errors-check-validity");

                if ($scope.riskForm.$invalid) { return; }

                var risk = $scope.newRisk;

                //name, action or user shouldn't be null or empty
                if (!risk.name || !risk.action || !risk.responsibleUserId) return;

                var data = {
                    itProjectId: projectId,
                    name: risk.name,
                    action: risk.action,
                    probability: risk.probability,
                    consequence: risk.consequence,
                    responsibleUserId: risk.responsibleUserId
                };

                var msg = notify.addInfoMessage("Gemmer række", false);
                $http.post(`api/risk?organizationId=${user.currentOrganizationId}`, data)
                    .then(function onSuccess(result) {

                        var responseRisk = result.data.response;
                        pushRisk(responseRisk);
                        resetNewRisk();

                        msg.toSuccessMessage("Rækken er gemt");
                    }, function onError(result) {
                        msg.toErrorMessage("Fejl! Prøv igen");
                    });

            };

        }
    ]);
})(angular, app);

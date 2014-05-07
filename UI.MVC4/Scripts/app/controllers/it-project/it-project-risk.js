(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.risk', {
            url: '/risk',
            templateUrl: 'partials/it-project/tab-risk.html',
            controller: 'project.EditRiskCtrl',
            resolve: {
                risks: ['$http', 'itProject', function ($http, itProject) {
                        return $http.get('api/risk/?getByProject&projectId=' + itProject.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }]
            }
        });
    }]);

    app.controller('project.EditRiskCtrl', ['$scope', '$http', '$stateParams', 'notify', 'risks',
        function($scope, $http, $stateParams, notify, risks) {

            var projectId = $stateParams.id;

            $scope.risks = [];

            function watchUser(risk) {

                function getUserId() {
                    return risk.userForSelect.id;
                }

                $scope.$watch(getUserId, function (newVal, oldVal) {

                    if (!newVal || newVal === oldVal) return;

                    $http({
                        method: 'PATCH',
                        url: risk.updateUrl,
                        data: {
                            'responsibleUserId': newVal
                        }
                    }).success(function() {
                        risk.responsibleUserId = risk.userForSelect.id;
                        risk.responsibleUser = risk.userForSelect.user;

                        notify.addSuccessMessage("Feltet er opdateret.");
                    }).error(function () {
                        notify.addErrorMessage("Fejl! Feltet kunne ikke ændres!");
                    });

                });


            }

            function pushRisk(risk) {
                risk.show = true;
                risk.userForSelect = {
                    id: risk.responsibleUserId,
                    text: risk.responsibleUser.name
                };

                risk.updateUrl = "api/risk/" + risk.id;

                watchUser(risk);

                $scope.risks.push(risk);
            }
            _.each(risks, pushRisk);

            $scope.product = function (risk) {

                return risk.consequence * risk.probability;
            };

            $scope.delete = function(risk) {
                $http.delete(risk.updateUrl).success(function(result) {
                    risk.show = false;

                    notify.addSuccessMessage("Rækken er slettet");
                }).error(function() {

                    notify.addErrorMessage("Fejl! Kunne ikke slette!");
                });
            };

            function resetNewRisk() {
                $scope.newRisk = {
                    consequence: 1,
                    probability: 1
                };
                $scope.userForSelect = null;
            }

            resetNewRisk();

            $scope.saveNewRisk = function() {
                var risk = $scope.newRisk;

                //name, action or user shouldn't be null or empty
                if (!risk.name || !risk.action || !risk.userForSelect) return;
                
                var data = {
                    itProjectId: projectId,
                    name: risk.name,
                    action: risk.action,
                    probability: risk.probability,
                    consequence: risk.consequence,
                    responsibleUserId: risk.userForSelect.id
                };

                var msg = notify.addInfoMessage("Gemmer række", false);
                $http.post("api/risk", data)
                    .success(function (result) {

                        var responseRisk = result.response;
                        responseRisk.responsibleUser = risk.userForSelect.user;
                        pushRisk(responseRisk);
                        resetNewRisk();

                        msg.toSuccessMessage("Rækken er gemt");
                    })
                    .error(function() {
                        msg.toErrorMessage("Fejl! Prøv igen");
                    });

            };

        }
    ]);
})(angular, app);
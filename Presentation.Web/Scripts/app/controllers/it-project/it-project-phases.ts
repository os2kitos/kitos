(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.phases', {
            url: '/phases',
            templateUrl: 'partials/it-project/tab-phases.html',
            controller: 'project.EditPhasesCtrl',
            resolve: {
                // re-resolve data from parent cause changes here wont cascade to it
                project: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('project.EditPhasesCtrl',
    ['$scope', '$http', 'notify', '$modal', 'project', 'user',
        function ($scope, $http, notify, $modal, project, user) {
            $scope.project = project;
            $scope.project.updateUrl = "api/itproject/" + project.id + '?organizationId=' + user.currentOrganizationId;

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            //Setup phases
            $scope.project.phases = [project.phase1, project.phase2, project.phase3, project.phase4, project.phase5];

            function patch(url, field, value) {
                var payload = {};
                payload[field] = value;

                return $http({
                    method: 'PATCH',
                    url: url,
                    data: payload
                });
            }

            $scope.updatePhaseName = function(phase, num) {
                var payload = {};
                payload["Name"] = phase.name;
                $http.post($scope.project.updateUrl + "&phaseNum=" + num, payload)
                .success(function() {
                    notify.addSuccessMessage("Feltet er opdateret");
                    })
                .error(function () {
                    notify.addErrorMessage("Fejl!");
                });
            }

            $scope.updateSelectedPhase = function (phaseNum) {
                patch($scope.project.updateUrl, "currentPhase", phaseNum)
                    .success(function (result) {
                        notify.addSuccessMessage("Feltet er opdateret");
                        $scope.project.currentPhase = phaseNum;
                    })
                    .error(function () {
                        notify.addErrorMessage("Fejl!");
                    });
            };

            $scope.updatePhaseDate = function(phase, num) {
                var dateObject = moment(phase.startDate, "DD-MM-YYYY");
                var startDate;
                if (dateObject.isValid()) {
                    startDate = dateObject.format("YYYY-MM-DD");
                } else {
                    startDate = null;
                }
                //Update start date of the current phase
                var firstPayload = {};
                firstPayload["StartDate"] = startDate;
                $http.post($scope.project.updateUrl + "&phaseNum=" + num, firstPayload)
                    .success(function () {
                        if (num > 1) {
                            var prevPhaseNum = num - 1;
                            var secondPayload = {};
                            secondPayload["EndDate"] = startDate;
                            //Also update end date of the previous phase
                            $http.post($scope.project.updateUrl + "&phaseNum=" + prevPhaseNum, secondPayload).success(function () {
                                notify.addSuccessMessage("Feltet er opdateret");
                            }).error(function() {
                                notify.addErrorMessage("Fejl!");
                            });
                        }
                    }).error(function() {
                        notify.addErrorMessage("Fejl!");
                    });
            };
        }
    ]);
})(angular, app);

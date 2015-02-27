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

            //Setup phases
            $scope.project.phases = [project.phase1, project.phase2, project.phase3, project.phase4, project.phase5];
            var prevPhase = null;
            _.each($scope.project.phases, function (phase) {
                phase.updateUrl = "api/itprojectphase/" + phase.id;
                phase.prevPhase = prevPhase;
                prevPhase = phase;
            });

            function patch(url, field, value) {
                var payload = {};
                payload[field] = value;

                return $http({
                    method: 'PATCH',
                    url: url,
                    data: payload
                });
            }

            $scope.updateSelectedPhase = function (phase) {
                patch($scope.project.updateUrl, "currentPhaseId", phase.id)
                    .success(function (result) {
                        notify.addSuccessMessage("Feltet er opdateret");
                        $scope.project.currentPhaseId = phase.id;
                    })
                    .error(function () {
                        notify.addErrorMessage("Fejl!");
                    });
            };


            $scope.updatePhaseDate = function (phase) {
                //Update start date of the current phase
                patch(phase.updateUrl + "?organizationId=" + user.currentOrganizationId, "startDate", phase.startDate)
                    .success(function (result) {
                        //Also update end date of the previous phase
                        patch(phase.prevPhase.updateUrl + "?organizationId=" + user.currentOrganizationId, "endDate", phase.startDate).success(function () {
                            notify.addSuccessMessage("Feltet er opdateret");
                        }).error(function () {
                            notify.addErrorMessage("Fejl!");
                        });

                    }).error(function () {
                        notify.addErrorMessage("Fejl!");
                    });
            };

        }]);
})(angular, app);

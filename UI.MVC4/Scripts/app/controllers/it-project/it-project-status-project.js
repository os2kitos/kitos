(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.status-project', {
            url: '/status-project',
            templateUrl: 'partials/it-project/tab-status-project.html',
            controller: 'project.EditStatusProjectCtrl'
        });
    }]);

    app.controller('project.EditStatusProjectCtrl',
    ['$scope', '$http', 'notify', 'itProject',
        function ($scope, $http, notify, itProject) {
            $scope.project = itProject;
            $scope.project.updateUrl = "api/itproject/" + itProject.id;

            $scope.project.phases = [itProject.phase1, itProject.phase2, itProject.phase3, itProject.phase4, itProject.phase5];

            var prevPhase = null;
            _.each($scope.project.phases, function (phase) {
                phase.updateUrl = "api/activity/" + phase.id;
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


            $scope.updatePhaseDate = function(phase) {
                //Update start date of the current phase
                patch(phase.updateUrl, "startDate", phase.startDate)
                    .success(function (result) {
                        //Also update end date of the previous phase
                        patch(phase.prevPhase.updateUrl, "endDate", phase.startDate).success(function () {
                            notify.addSuccessMessage("Feltet er opdateret");
                        }).error(function () {
                            notify.addErrorMessage("Fejl!");
                        });

                    }).error(function () {
                        notify.addErrorMessage("Fejl!");
                    });
            };

            $scope.updateStatusDate = function() {
                patch($scope.project.updateUrl, "statusDate", $scope.project.statusDate)
                    .success(function () {
                        notify.addSuccessMessage("Feltet er opdateret");
                    }).error(function () {
                        notify.addErrorMessage("Fejl!");
                    });
            };

        }]);
})(angular, app);

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
            $scope.updateUrl = "api/itproject/" + itProject.id;

            function parseDate(dateStr) {
                return new Date(dateStr);
            }

            $scope.project.phases = [itProject.phase1, itProject.phase2, itProject.phase3, itProject.phase4, itProject.phase5];

            var prevPhase = null;
            _.each($scope.project.phases, function (phase) {
                phase.updateUrl = "api/activity/" + phase.id;
                phase.startDateDate = parseDate(phase.startDate);
                phase.endDateDate = parseDate(phase.endDate);
                phase.prevPhase = prevPhase;
                prevPhase = phase;
            });

            $scope.updateSelectedPhase = function(phase) {
                $http({
                    method: 'PATCH',
                    url: $scope.updateUrl,
                    data: {
                        currentPhaseId: phase.id
                    }
                }).success(function (result) {
                    notify.addSuccessMessage("Feltet er opdateret");
                    $scope.project.currentPhaseId = phase.id;
                }).error(function () {
                    notify.addErrorMessage("Fejl!");
                });
            };

            $scope.updateDate = function(phase) {

                var dateStr = phase.startDateDate.toISOString();

                //Update start date of the current phase
                $http({
                    method: 'PATCH',
                    url: phase.updateUrl,
                    data: {
                        startDate: dateStr
                    }
                }).success(function (result) {

                    //Also update end date of the previous phase
                    $http({
                        method: 'PATCH',
                        url: phase.prevPhase.updateUrl,
                        data: {
                            endDate: dateStr
                        }
                    }).success(function() {

                        notify.addSuccessMessage("Feltet er opdateret");

                    }).error(function() {
                        notify.addErrorMessage("Fejl!");
                    });

                }).error(function () {
                    notify.addErrorMessage("Fejl!");
                });

            };

        }]);
})(angular, app);

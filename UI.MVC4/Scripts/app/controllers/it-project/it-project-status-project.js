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

            function parseDate(dateStr) {
                return new Date(dateStr);
            }

            $scope.project.phases = [itProject.phase1, itProject.phase2, itProject.phase3, itProject.phase4, itProject.phase5];

            $scope.project.statusDateDate = parseDate($scope.project.statusDate);

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

            function patchDate(url, fieldName, dateObj) {
                var data = {};
                data[fieldName] = dateObj.toISOString();

                return $http({
                    method: 'PATCH',
                    url: url,
                    data: data
                });
            }

            $scope.updatePhaseDate = function(phase) {
                
                //Update start date of the current phase
                patchDate(phase.updateUrl, "startDate", phase.startDateDate)
                    .success(function (result) {
                        //Also update end date of the previous phase
                        patchDate(phase.prevPhase.updateUrl, "endDate", phase.startDateDate).success(function () {
                            notify.addSuccessMessage("Feltet er opdateret");
                        }).error(function () {
                            notify.addErrorMessage("Fejl!");
                        });

                    }).error(function () {
                        notify.addErrorMessage("Fejl!");
                    });
            };

            $scope.updateStatusDate = function() {
                patchDate($scope.project.updateUrl, "StatusDate", $scope.project.statusDateDate)
                    .success(function () {
                        notify.addSuccessMessage("Feltet er opdateret");
                    }).error(function () {
                        notify.addErrorMessage("Fejl!");
                    });
            };
            
            $scope.foo = function() {
                console.log("FOO");
            }

        }]);
})(angular, app);

(function(ng, app) {

    app.config(['$stateProvider', function($stateProvider) {

        $stateProvider.state('it-project.edit.stakeholders', {
            url: '/stakeholders',
            templateUrl: 'partials/it-project/tab-stakeholders.html',
            controller: 'project.EditStakeholdersCtrl',
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

    app.controller('project.EditStakeholdersCtrl',
    ['$rootScope', '$scope', '$http', 'notify', 'project', 'user',
        function($rootScope, $scope, $http, notify, project, user) {

            $scope.stakeholders = [];

            function addStakeholder(stakeholder) {
                stakeholder.show = true;

                stakeholder.updateUrl = "api/stakeholder/" + stakeholder.id;

                stakeholder.delete = function() {
                    var msg = notify.addInfoMessage("Sletter...");
                    $http.delete(stakeholder.updateUrl + '?organizationId=' + user.currentOrganizationId).success(function () {
                        stakeholder.show = false;
                        msg.toSuccessMessage("Rækken er slettet");
                    }).error(function() {
                        msg.toErrorMessage("Kunne ikke slette!");
                    });
                };

                $scope.stakeholders.push(stakeholder);
            }

            _.each(project.stakeholders, addStakeholder);

            function resetNew() {
                $scope.new = {};
            }

            resetNew();

            $scope.saveNewStakeholder = function() {
                $scope.$broadcast('show-errors-check-validity');

                if ($scope.stakeholderForm.$invalid) { return; }
                
                var row = $scope.new;

                var data = {
                    itProjectId: project.id,
                    name: row.name,
                    role: row.role,
                    downsides: row.downsides,
                    benefits: row.benefits,
                    significance: row.significance,
                    howToHandle: row.howToHandle
                };

                var msg = notify.addInfoMessage("Gemmer... ");
                $http.post("api/stakeholder", data).success(function(result) {
                    msg.toSuccessMessage("Rækken er tilføjet.");
                    addStakeholder(result.response);
                    resetNew();
                }).error(function() {
                    msg.toErrorMessage("Fejl! Kunne ikke gemme!");
                });

            };


        }]);


})(angular, app);
    
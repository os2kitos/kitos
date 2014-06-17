(function(ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project', {
            url: '/project',
            abstract: true,
            template: '<ui-view/>',
            controller: ['$rootScope', '$scope', function($rootScope, $scope) {
                $rootScope.page.title = 'IT Projekt';
                $rootScope.page.subnav = [
                    { state: 'it-project.overview', text: 'Overblik' },
                    { state: 'it-project.portfolio', text: 'Portefølje' },
                    { state: 'it-project.catalog', text: 'IT Projekt katalog' },
                    { state: 'it-project.edit', text: 'IT Projekt', showWhen: 'it-project.edit' }
                ];
                
                $scope.create = function () {
                    var orgUnitId = user.currentOrganizationUnitId;
                    var payload = {
                        itProjectTypeId: 1,
                        ItProjectCategoryId: 1,
                        responsibleOrgUnitId: orgUnitId,
                        organizationId: user.currentOrganizationId,
                    };

                    $scope.creatingProject = true;
                    var msg = notify.addInfoMessage("Opretter projekt...", false);

                    $http.post('api/itproject', payload).success(function (result) {

                        msg.toSuccessMessage("Et nyt projekt er oprettet!");

                        var projectId = result.response.id;

                        if (orgUnitId) {
                            // add users default org unit to the new project
                            $http.post('api/itproject/' + projectId + '?organizationunit=' + orgUnitId);
                        }

                        $state.go('it-project.edit.status-project', { id: projectId });

                    }).error(function () {
                        msg.toErrorMessage("Fejl! Kunne ikke oprette nyt projekt!");
                        $scope.creatingProject = false;
                    });
                };
            }]
        });
    }]);
})(angular, app);
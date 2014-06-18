(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system', {
            url: '/system',
            abstract: true,
            template: '<ui-view/>',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            },
            controller: ['$rootScope', '$http', '$state', 'notify', 'user', function ($rootScope, $http, $state, notify, user) {
                $rootScope.page.title = 'IT System';
                $rootScope.page.subnav = [
                    { state: 'it-system.overview', text: 'Overblik' },
                    { state: 'it-system.catalog', text: 'IT System katalog' },
                    { state: 'it-system.edit', text: 'IT System', showWhen: 'it-system.edit' },
                    { state: 'it-system.usage', text: 'IT System anvendelse', showWhen: 'it-system.usage' }
                ];
                $rootScope.page.buttons = [
                    { func: create, text: 'Opret IT System' }
                ];

                function create() {
                    var payload = {
                        name: 'Unavngivent IT system',
                        belongsToId: user.currentOrganizationId,
                        organizationId: user.currentOrganizationId,
                        userId: user.id,
                        dataRows: [],
                        taskRefIds: [],
                        canUseInterfaceIds: []
                    };

                    var msg = notify.addInfoMessage("Opretter system...", false);
                    $http.post('api/itsystem', payload)
                        .success(function(result) {
                            msg.toSuccessMessage("Et nyt system er oprettet!");
                            var systemId = result.response.id;
                            $state.go('it-system.edit.interfaces', { id: systemId });
                        }).error(function() {
                            msg.toErrorMessage("Fejl! Kunne ikke oprette et nyt system!");
                        });
                };
            }]
        });
    }]);
})(angular, app);
(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('organization', {
            url: '/organization',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            },
            controller: ['$rootScope', '$uibModal', '$state', 'user', function ($rootScope, $modal, $state, user) {
                $rootScope.page.title = 'Organisation';

                var subnav = [];

                if (user.currentConfig.showTabOverview) {
                    subnav.push({ state: 'organization.overview', text: 'Overblik' });
                }

                subnav.push({ state: 'organization.structure', text: 'Organisation' });
                subnav.push({ state: 'organization.user', text: 'Bruger' });

                $rootScope.page.subnav = subnav;

                $rootScope.page.subnav.buttons = [
                    { func: createUser, text: 'Opret Bruger', style: 'btn-success', icon: 'glyphicon-plus', showWhen: 'organization.user' }
                ];

                function createUser() {
                    $state.go("organization.user.create");
                };
            }]
        });
    }]);
})(angular, app);

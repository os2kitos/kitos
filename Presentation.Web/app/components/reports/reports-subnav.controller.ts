((ng, app) => {
    app.config(['$stateProvider', $stateProvider => {
        $stateProvider.state('reports', {
            url: '/rapporter',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: ['userService', userService => userService.getUser()]
            },
            controller: ['$rootScope', '$state', 'user', '$scope', ($rootScope, $state, user, $scope) => {
                $rootScope.page.title = 'Rapporter';

                var subnav = [];
                subnav.push({ state: 'reports.overview', text: 'Overblik' });
                $rootScope.page.subnav = subnav;
                $rootScope.subnavPositionCenter = false;

                $scope.$on('$viewContentLoaded', function () {
                    $rootScope.positionSubnav();
                });
            }]
        });
    }]);
})(angular, app);
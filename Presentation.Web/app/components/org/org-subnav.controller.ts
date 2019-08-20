(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('organization', {
            url: '/organization',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }],
                hasWriteAccess: [
                    '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                        return $http.get('api/Organization/' + user.currentOrganizationId + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ]
            },
            controller: ['$rootScope', '$scope', '$uibModal', '$state', 'user', 'hasWriteAccess', '$timeout', function ($rootScope, $scope, $modal, $state, user, hasWriteAccess,$timeout) {
                $rootScope.page.title = 'Organisation';

                var subnav = [];

                if (user.currentConfig.showTabOverview) {
                    subnav.push({ state: 'organization.overview', text: 'Overblik' });
                }

                subnav.push({ state: 'organization.structure', text: 'Organisation' });
                subnav.push({ state: 'organization.user', text: 'Brugere' });
                subnav.push({ state: 'organization.gdpr', text: 'Stamdata' });

                $rootScope.page.subnav = subnav;

                $rootScope.page.subnav.buttons = [
                    { func: createUser, text: 'Opret Bruger', style: 'btn-success', disabled: !hasWriteAccess, icon: 'glyphicon-plus', showWhen: 'organization.user'}
                ];
                $rootScope.subnavPositionCenter = false;

                function createUser() {
                    if (hasWriteAccess == true) {
                        $state.go("organization.user.create");}
                    
                };

                $scope.$on('$viewContentLoaded', function () {
                    $rootScope.positionSubnav();
                });
            }]
        });
    }]);
})(angular, app);

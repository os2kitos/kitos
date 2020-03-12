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
                userAccessRights: ["authorizationServiceFactory", "user",
                    (authorizationServiceFactory: Kitos.Services.Authorization.IAuthorizationServiceFactory, user) =>
                    authorizationServiceFactory
                    .createOrganizationAuthorization()
                    .getAuthorizationForItem(user.currentOrganizationId)
                ],
                hasWriteAccess: ["userAccessRights", userAccessRights => userAccessRights.canEdit
                ],
            },
            controller: ['$rootScope', '$scope', '$state', 'user', 'hasWriteAccess', ($rootScope, $scope, $state, user, hasWriteAccess) => {
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
                    { func: createUser, dataElementType:'createUserButton', text: 'Opret Bruger', style: 'btn-success', disabled: !hasWriteAccess, icon: 'glyphicon-plus', showWhen: 'organization.user'}
                ];
                $rootScope.subnavPositionCenter = false;

                function createUser() {
                    if (hasWriteAccess === true) {
                        $state.go("organization.user.create");}
                    
                };

                $scope.$on('$viewContentLoaded', () => {
                    $rootScope.positionSubnav();
                });
            }]
        });
    }]);
})(angular, app);

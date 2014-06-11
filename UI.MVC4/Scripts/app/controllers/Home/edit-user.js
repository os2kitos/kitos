(function (ng, app) {

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('edit-user', {
            url: '/user',
            templateUrl: 'partials/home/edit-user.html',
            controller: 'home.EditUserCtrl',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }],
                orgUnits: ['userService', '$http', function (userService, $http) {
                    return userService.getUser().then(function (user) {
                        return $http.get('api/organizationunit/?byUser').then(function (result) {
                            return result.data.response;
                        });
                    });
                }]
            }
        });

    }]);

    app.controller('home.EditUserCtrl', ['$rootScope', '$scope', 'notify', 'userService', 'user', 'orgUnits',
        function ($rootScope, $scope, notify, userService, user, orgUnits) {
            $rootScope.page.title = 'Profil indstillinger';
            $rootScope.page.subnav = [];
            
            $scope.user = {
                name: user.name,
                email: user.email,
                defaultOrganizationUnitId: user.defaultOrganizationUnitId
            };

            $scope.orgUnits = orgUnits;

            $scope.patch = function (field, value) {
                var payload = {};
                payload[field] = value;

                userService.patchUser(payload).then(function() {
                    notify.addSuccessMessage("Feltet er opdateret!");
                }, function() {
                    notify.addErrorMessage("Fejl! Kunne ikke opdatere feltet!");
                });
            };

        }]);

})(angular, app);
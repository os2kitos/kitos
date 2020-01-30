(function (ng, app) {

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('edit-user', {
            url: '/user',
            templateUrl: 'app/components/home/home-edit-user.view.html',
            controller: 'home.EditUserCtrl',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }],
                orgUnits: ['userService', '$http', function (userService, $http) {
                    return userService.getUser().then(function (user) {
                        return $http.get('api/organizationunit/?byUser&organizationId=' + user.currentOrganizationId).then(function (result) {
                            return result.data.response;
                        });
                    });
                }]
            }
        });

    }]);

    app.controller('home.EditUserCtrl', ['$rootScope', '$scope', 'notify', 'userService', 'user', 'orgUnits',
        function($rootScope, $scope, notify, userService, user, orgUnits) {
            $rootScope.page.title = 'Profil indstillinger';
            $rootScope.page.subnav = [];

            function init(user) {
                $scope.user = {
                    name: user.name,
                    lastName: user.lastName,
                    email: user.email,
                    phoneNumber: user.phoneNumber,
                    defaultUserStartPreference: user.defaultUserStartPreference,
                    defaultOrganizationUnitId: user.defaultOrganizationUnitId,
                    isUsingDefaultOrgUnit: user.isUsingDefaultOrgUnit,
                    currentOrganizationName: user.currentOrganizationName,
                    currentOrganizationUnitName: user.currentOrganizationUnitName
                };

            }

            init(user);

            //check if user has any organizationunits to choose from
            if (orgUnits.length > 0) {
                $scope.orgUnits = orgUnits;
            //if not -> choose organization.root
            } else {
                $scope.orgUnits = [user.currentOrganization.root];
                $scope.fakeDefaultOrganizationUnitId = user.currentOrganization.root.id;
                $scope.noOrgUnits = true;
            }
            //can't use autosave - need to patch through userService!
            $scope.patch = function (field, value) {
                var payload = {};
                payload[field] = value;

                userService.patchUser(payload).then(function (newUser) {
                    init(newUser);
                    notify.addSuccessMessage("Feltet er opdateret!");
                }, function() {
                    notify.addErrorMessage("Fejl! Kunne ikke opdatere feltet!");
                });
            };
            $scope.updateDefaultOrgUnit = function() {
                userService.updateDefaultOrgUnit($scope.user.defaultOrganizationUnitId).then(function (newUser) {
                    userService.getUser().then(
                        (data) => { return data });
                    //init(newUser);
                    notify.addSuccessMessage("Feltet er opdateret!");
                }, function() {
                    notify.addErrorMessage("Fejl! Kunne ikke opdatere feltet!");
                });
            };
        }]);

})(angular, app);

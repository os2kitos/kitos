var app = angular.module('app', ['ui.router', 'ui.bootstrap', 'ui.select2', 'ngAnimate', 'notify', 'xeditable', 'restangular', 'ui.utils']);

app.config(['$urlRouterProvider', function ($urlRouterProvider) {
    $urlRouterProvider.otherwise('/');
}]);

app.config(['$httpProvider', 'notifyProvider', 'RestangularProvider', function ($httpProvider, notifyProvider, restangularProvider) {
    $httpProvider.interceptors.push("httpBusyInterceptor");

    notifyProvider.globalTimeToLive(5000);
    notifyProvider.onlyUniqueMessages(false);

    //Restangular config
    restangularProvider.setBaseUrl('/api');
    restangularProvider.setRestangularFields({
        id: 'id'
    });
    restangularProvider.setResponseExtractor(function (response, operation) {
        return response.response;
    });
}]);

app.run(['$rootScope', '$http', '$state', 'editableOptions', '$modal', 'notify', 'userService',
    function ($rootScope, $http, $state, editableOptions, $modal, notify, userService) {
        //init info
        $rootScope.page = {
            title: 'Index',
            subnav: []
        };

        //x-editable config
        editableOptions.theme = 'bs3'; // bootstrap3 theme.

        userService.getUser().then(function (user) {
            $rootScope.openProfileModal = function () {
                $modal.open({
                    templateUrl: 'partials/topnav/profileModal.html',
                    resolve: {
                        orgUnits: [function () {
                            return $http.get('api/organizationunit/?userid2=' + user.id).success(function (result) {
                                return result.response;
                            });
                        }]
                    },
                    controller: ['$scope', '$modalInstance', 'orgUnits', function ($modalScope, $modalInstance, orgUnits) {
                        $modalScope.user = user;
                        $modalScope.orgUnits = orgUnits;

                        $modalScope.ok = function () {
                            var userData = {};
                            if ($modalScope.user.name)
                                userData.name = $modalScope.user.name;
                            if ($modalScope.user.defaultOrganizationUnitId)
                                userData.defaultOrganizationUnitId = $modalScope.user.defaultOrganizationUnitId;
                            if ($modalScope.user.email)
                                userData.email = $modalScope.user.email;

                            $http({
                                method: 'PATCH',
                                url: 'api/user/' + user.id,
                                data: userData
                            }).success(function () {
                                notify.addSuccessMessage('OK');
                                $modalInstance.close();
                            }).error(function () {
                                notify.addErrorMessage('Fejl');
                            });
                        };

                        $modalScope.cancel = function () {
                            $modalInstance.dismiss('cancel');
                        };
                    }]
                });
            };

        });



        //logout function for top navigation bar
        $rootScope.logout = function () {
            userService.logout().then(function () {
                $state.go('index');
            });

        };

        $rootScope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {

            if (toState.noAuth) return; //no need to auth
            
            userService.auth(toState.adminRoles).then(function (val) {
                console.log("AUTH OK");
                
                //Authentication OK!
                
            }, function () {
                console.log("AUTH BAD");
                
                event.preventDefault();

                //Bad authentication
                $state.go('index', { to: toState.name, toParams: toParams });
            });
        });

        $rootScope.$on('$stateChangeError', function (event, toState, toParams, fromState, fromParams, error) {
            console.log(error);
            $state.go('index');
        });


    }]);
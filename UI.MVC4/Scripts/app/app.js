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

app.run(['$rootScope', '$http', '$state', 'editableOptions', '$modal', 'notify', function ($rootScope, $http, $state, editableOptions, $modal, notify) {
    //init info
    $rootScope.page = {
        title: 'Index',
        subnav: []
    };

    //x-editable config
    editableOptions.theme = 'bs3'; // bootstrap3 theme.

    $rootScope.user = {};

    $rootScope.openProfileModal = function() {
        $modal.open({
            templateUrl: 'partials/topnav/profileModal.html',
            resolve: {
                user: function() {
                    return $rootScope.user;
                },
                orgUnits: function() {
                    return $http.get('api/organizationunit/?userid2=' + $rootScope.user.id);
                }
            },
            controller: ['$scope', '$modalInstance', 'user', 'orgUnits', function ($modalScope, $modalInstance, user, orgUnits) {
                $modalScope.user = user;
                $modalScope.orgUnits = orgUnits.data.response;

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
                    }).success(function() {
                        notify.addSuccessMessage('OK');
                        $modalInstance.close();
                    }).error(function() {
                        notify.addErrorMessage('Fejl');
                    });
                };

                $modalScope.cancel = function () {
                    $modalInstance.dismiss('cancel');
                };
            }]
        });
    };
    

    //logout function for top navigation bar
    $rootScope.logout = function () {
        $http.post('api/authorize?logout').success(function (result) {
            $rootScope.user = {};
            $state.go('index');
        });
    };

    $rootScope.saveUser = function (result) {
        var isLocalAdmin = _.some(result.response.adminRights, function(userRight) {
            return userRight.roleName == "LocalAdmin";
        });

        $rootScope.user = {
            id: result.response.id,
            authStatus: 'authorized',
            name: result.response.name,
            email: result.response.email,
            isGlobalAdmin: result.response.isGlobalAdmin,
            isLocalAdmin: isLocalAdmin,
            isLocalAdminFor: _.pluck(result.response.adminRights, 'organizationId'),
            defaultOrganizationUnitId: result.response.defaultOrganizationUnitId
        };

    };

    var hasInitUser = false;
    var initUser = $http.get('api/authorize').success($rootScope.saveUser).finally(function() {
        hasInitUser = true;
    });
    
    function auth(toState, toParams) {
        var user = $rootScope.user;

        if (user.authStatus != 'authorized') return false; //user hasn't authorized
        
        var adminRoles = toState.adminRoles;
        if (!adminRoles) return true; //no specific admin role needed

        //go through each of the roles on the state
        return _.some(adminRoles, function (role) {
            //if the state role is global admin, and the user is global admin, it's cool
            //same for local admin
            return (role == "GlobalAdmin" && user.isGlobalAdmin) || (role == "LocalAdmin" && user.isLocalAdmin);
        });
    }

    $rootScope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {
        if (toState.noAuth) return; //no need to auth

        //need to auth, but first when the initUser call is finished.
        if (!hasInitUser) {

            //initUser is not yet loaded - wait for it
            event.preventDefault();
            
            initUser.finally(function() {
                var user = $rootScope.user;
                var userRole = user.role;
                var authRoles = toState.authRoles;

                if (!auth(toState)) {
                    $state.go('login', { to: toState.name, toParams: toParams });
                } else {
                    $state.go(toState, toParams);
                }
            });
        } else {
            //initUser has loaded, just run auth()
            if (!auth(toState)) {
                event.preventDefault();
                $state.go('login', { to: toState.name, toParams: toParams });
            }
        }

    });
}]);
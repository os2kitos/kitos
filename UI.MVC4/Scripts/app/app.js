var app = angular.module('app', ['ui.router', 'ui.bootstrap', 'ui.select2', 'ngAnimate', 'angular-growl', 'xeditable', 'restangular', 'ui.utils']);

app.config(['$urlRouterProvider', function ($urlRouterProvider) {
    $urlRouterProvider.otherwise('/');
}]);

app.config(['growlProvider', 'RestangularProvider', function (growlProvider, RestangularProvider) {
    growlProvider.globalTimeToLive(5000);
    growlProvider.onlyUniqueMessages(false);
    
    //Restangular config
    RestangularProvider.setBaseUrl('/api');
    RestangularProvider.setRestangularFields({
        id: 'id'
    });
    RestangularProvider.setResponseExtractor(function (response, operation) {
        return response.response;
    });
}]);

app.run(['$rootScope', '$http', '$state', 'editableOptions', '$modal', 'growl', function ($rootScope, $http, $state, editableOptions, $modal, growl) {
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
            controller: ['$scope', '$modalInstance', function ($modalScope, $modalInstance) {
                $modalScope.user = $rootScope.user;

                $http.get('api/user/' + $modalScope.user.id + '?organizations').success(function (data) {
                    $modalScope.organizations = data;
                });

                $modalScope.ok = function () {
                    $http({
                        method: 'PATCH',
                        url: 'api/user/' + $rootScope.user.id,
                        data: data
                    }).success(function() {
                        growl.addSuccessMessage('OK');
                        $modalInstance.close();
                    }).error(function() {
                        growl.addErrorMessage('Fejl');
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
            isLocalAdminFor: _.pluck(result.response.adminRights, 'organizationId')
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
var app = angular.module('app', ['ui.router', 'ui.bootstrap', 'ui.select2', 'ngAnimate', 'angular-growl', 'xeditable', 'restangular']);

app.config(['$urlRouterProvider', function ($urlRouterProvider) {
    $urlRouterProvider.otherwise('/');
}]);

app.config(['growlProvider', 'RestangularProvider', function (growlProvider, RestangularProvider) {
    growlProvider.globalTimeToLive(5000);
    growlProvider.onlyUniqueMessages(false);
    
    //Restangular config
    RestangularProvider.setBaseUrl('/api');
    RestangularProvider.setRestangularFields({
        id: 'Id'
    });
    RestangularProvider.setResponseExtractor(function (response, operation) {
        return response.Response;
    });
}]);

app.run(['$rootScope', '$http', '$state', 'editableOptions', function ($rootScope, $http, $state, editableOptions) {
    //init info
    $rootScope.page = {
        title: 'Index',
        subnav: []
    };

    //users cache
    $rootScope.users = [];
    $rootScope.selectUserOptions = {
        minimumInputLength: 1,
        initSelection: function (element, callback) {
        },
        ajax: {
            data: function (term, page) {
                return { query: term };
            },
            quietMillis: 500,
            transport: function (queryParams) {
                //console.log(queryParams);
                var res = $http.get('api/user?q=' + queryParams.data.query).then(queryParams.success);
                res.abort = function () {
                    return null;
                };

                return res;
            },
            results: function (data, page) {
                console.log(data);
                var results = [];

                _.each(data.data.Response, function (user) {
                    //Save to cache
                    $rootScope.users[user.Id] = user;

                    results.push({
                        id: user.Id,
                        text: user.Name
                    });
                });

                return { results: results };
            }
        }


    };

    $rootScope.user = {};

    //x-editable config
    editableOptions.theme = 'bs3'; // bootstrap3 theme.

    //logout function for top navigation bar
    $rootScope.logout = function () {
        $http.post('api/authorize?logout').success(function (result) {
            $rootScope.user = {};
            $state.go('index');
        });
    };

    $rootScope.saveUser = function (result) {
        var isLocalAdmin = _.some(result.Response.AdminRights, function(userRight) {
            return userRight.RoleName == "LocalAdmin";
        });

        $rootScope.user = {
            id: result.Response.Id,
            authStatus: 'authorized',
            name: result.Response.Name,
            email: result.Response.Email,
            isGlobalAdmin: result.Response.IsGlobalAdmin,
            isLocalAdmin: isLocalAdmin,
            isLocalAdminFor: _.pluck(result.Response.AdminRights, 'Organization_Id')
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
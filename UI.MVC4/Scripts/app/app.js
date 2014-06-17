var app = angular.module('app', ['ui.router', 'ui.bootstrap', 'ui.select2', 'ngAnimate', 'notify', 'xeditable', 'restangular', 'ui.utils', 'angularjs-dropdown-multiselect']);

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

app.run(['$rootScope', '$http', '$state', 'editableOptions', '$modal', 'notify', 'userService', 'uiSelect2Config',
    function ($rootScope, $http, $state, editableOptions, $modal, notify, userService, uiSelect2Config) {
        //init info
        $rootScope.page = {
            title: 'Index',
            subnav: []
        };

        $rootScope.$state = $state;

        //this will try to authenticate - to see if the user's already logged in
        userService.getUser();

        uiSelect2Config.dropdownAutoWidth = true;

        //x-editable config
        editableOptions.theme = 'bs3'; // bootstrap3 theme.

        //logout function for top navigation bar
        $rootScope.logout = function () {
            userService.logout().then(function () {
                $state.go('index');
            });

        };

        //when changing states, we might need to authorize the user
        $rootScope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {

            if (toState.noAuth) return; //no need to auth
            
            userService.auth(toState.adminRoles).then(function (val) {
                //Authentication OK!
                
            }, function () {
                event.preventDefault();

                //Bad authentication
                $state.go('index', { to: toState.name, toParams: toParams });
            });
        });

        //when something goes wrong during state change (e.g a rejected resolve)
        $rootScope.$on('$stateChangeError', function (event, toState, toParams, fromState, fromParams, error) {
            console.log(error);
            $state.go('index');
        });


    }]);
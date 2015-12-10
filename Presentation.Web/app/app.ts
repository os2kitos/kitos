﻿var app = angular.module('app', [
    'ui.router',
    'ui.bootstrap',
    'ui.select2',
    'ngAnimate',
    'notify',
    'ui.utils',
    'angularjs-dropdown-multiselect',
    'ngSanitize',
    'kendo.directives']);

app.constant('moment', moment);

app.config([
    '$urlRouterProvider', ($urlRouterProvider: angular.ui.IUrlRouterProvider) => {
        $urlRouterProvider.otherwise('/');
    }
]);

app.config([
    '$httpProvider',
    'notifyProvider',
    'datepickerConfig',
    'datepickerPopupConfig',
    function ($httpProvider, notifyProvider, datepickerConfig, datepickerPopupConfig) {
        $httpProvider.interceptors.push('httpBusyInterceptor');
        // for some reason templates aren't updated so this is needed
        $httpProvider.defaults.headers.get = {
            'Cache-Control': 'no-cache'
        };
        notifyProvider.globalTimeToLive(5000);
        notifyProvider.onlyUniqueMessages(false);

        // global config for UI Bootstrap
        datepickerConfig.startingDay = 1; // set starting day of the calendar to monday
        datepickerPopupConfig.datepickerPopup = 'dd-MM-yyyy'; // set default date format
    }
]);

app.run([
    '$rootScope', '$http', '$state', '$modal', 'notify', 'userService', 'uiSelect2Config',
    function ($rootScope, $http, $state, $modal, notify, userService, uiSelect2Config) {
        // init info
        $rootScope.page = {
            title: 'Index',
            subnav: []
        };

        $rootScope.$state = $state;

        // this will try to authenticate - to see if the user's already logged in
        userService.getUser();

        uiSelect2Config.dropdownAutoWidth = true;

        // logout function for top navigation bar
        $rootScope.logout = function() {
            userService.logout().then(function() {
                $state.go('index');
            });
        };

        // when changing states, we might need to authorize the user
        $rootScope.$on('$stateChangeStart', function(event, toState, toParams, fromState, fromParams) {

            if (toState.noAuth) { // no need to auth
                 return;
            }

            userService.auth(toState.adminRoles).then(function(val) {
                // authentication OK!

            }, function() {
                event.preventDefault();

                // bad authentication
                $state.go('index', { to: toState.name, toParams: toParams });
            });
        });

        // when something goes wrong during state change (e.g a rejected resolve)
        $rootScope.$on('$stateChangeError', function(event, toState, toParams, fromState, fromParams, error) {
            console.log(error);
            $state.go('index');
        });
    }
]);

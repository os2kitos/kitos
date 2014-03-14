﻿(function (ng, app) {
   
    var subnav = [
            { state: "global-admin", text: "Opret kommune" },
            { state: "new-global-admin", text: "Opret global admin" },
            { state: "new-local-admin", text: "Opret lokal admin" }
    ];

    
    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('global-admin', {
            url: '/global-admin',
            templateUrl: 'partials/global-admin/new-municipality.html',
            controller: 'globalAdmin.NewMunicipalityCtrl',
            authRoles: ['GlobalAdmin']
        }).state('new-local-admin', {
            url: '/global-admin/new-local-admin',
            templateUrl: 'partials/global-admin/new-local-admin.html',
            controller: 'globalAdmin.NewLocalAdminCtrl',
            authRoles: ['GlobalAdmin']
        }).state('new-global-admin', {
            url: '/global-admin/new-global-admin',
            templateUrl: 'partials/global-admin/new-global-admin.html',
            controller: 'globalAdmin.NewGlobalAdminCtrl',
            authRoles: ['GlobalAdmin']
        });

    }]);
    
    app.controller('globalAdmin.NewMunicipalityCtrl', ['$rootScope', '$scope', '$http', 'growl', function ($rootScope, $scope, $http, growl) {
        $rootScope.page.title = 'Ny kommune';
        $rootScope.page.subnav = subnav;

        $scope.submit = function() {
            if ($scope.addForm.$invalid) return;

            var data = { "Name": $scope.name };
            
            $http.post('api/municipality', data).success(function (result) {
                growl.addSuccessMessage("Kommunen " + $scope.name + " er blevet oprettet!");

                $scope.name = "";
            }).error(function (result) {
                growl.addErrorMessage("Kommunen " + $scope.name + " kunne ikke oprettes!");
            });
        };
    }]);

    app.controller('globalAdmin.NewLocalAdminCtrl', ['$rootScope', '$scope', '$http', 'growl', function ($rootScope, $scope, $http, growl) {
        $rootScope.page.title = 'Ny lokal admin';
        $rootScope.page.subnav = subnav;

        $scope.municipalities = [];
        $http.get("api/municipality").success(function(result) {
            $scope.municipalities = result.Response;
        });

        $scope.submit = function () {
            if ($scope.addForm.$invalid) return;

            var data = {
                "Name": $scope.name,
                "Email": $scope.email,
                "Municipality_Id": $scope.municipality
            };

            $http.post('api/localadmin', data).success(function (result) {
                growl.addSuccessMessage("Lokaladmin " + $scope.name + " er blevet oprettet!");

                $scope.name = "";
                $scope.email = "";
                $scope.municipality = "";
                
            }).error(function (result) {
                growl.addSuccessMessage("Lokaladmin " + $scope.name + " kunne ikke oprettes!");
            });
        };
    }]);

    app.controller('globalAdmin.NewGlobalAdminCtrl', ['$rootScope', '$scope', '$http', 'growl', function ($rootScope, $scope, $http, growl) {
        $rootScope.page.title = 'Ny global admin';
        $rootScope.page.subnav = subnav;

        $scope.submit = function () {
            if ($scope.addForm.$invalid) return;

            var data = {
                "Name": $scope.name,
                "Email": $scope.email
            };

            $http.post('api/globaladmin', data).success(function (result) {
                growl.addSuccessMessage("Global admin " + $scope.name + " er blevet oprettet!");

                $scope.name = "";
                $scope.email = "";

            }).error(function (result) {
                growl.addSuccessMessage("Global admin " + $scope.name + " kunne ikke oprettes!");
            });
        };
    }]);

})(angular, App);
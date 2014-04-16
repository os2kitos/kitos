(function (ng, app) {
   
    var subnav = [
            { state: "global-admin", text: "Opret organisation" },
            { state: "new-global-admin", text: "Opret global admin" },
            { state: "new-local-admin", text: "Opret lokal admin" }
    ];

    
    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('global-admin', {
            url: '/global-admin',
            templateUrl: 'partials/global-admin/new-organization.html',
            controller: 'globalAdmin.NewOrganizationCtrl',
            authRoles: ['GlobalAdmin']
        }).state('new-local-admin', {
            url: '/global-admin/new-local-admin',
            templateUrl: 'partials/global-admin/new-local-admin.html',
            controller: 'globalAdmin.NewLocalAdminCtrl',
            authRoles: ['GlobalAdmin']
        }).state('local-admins', {
            url: '/global-admin/local-admins',
            templateUrl: 'partials/global-admin/local-admins.html',
            controller: 'globalAdmin.LocalAdminsCtrl',
            authRoles: ['GlobalAdmin']
        }).state('new-global-admin', {
            url: '/global-admin/new-global-admin',
            templateUrl: 'partials/global-admin/new-global-admin.html',
            controller: 'globalAdmin.NewGlobalAdminCtrl',
            authRoles: ['GlobalAdmin']
        });

    }]);
    
    app.controller('globalAdmin.NewOrganizationCtrl', ['$rootScope', '$scope', '$http', 'notify', function ($rootScope, $scope, $http, notify) {
        $rootScope.page.title = 'Ny organisation';
        $rootScope.page.subnav = subnav;

        $scope.submit = function() {
            if ($scope.addForm.$invalid) return;

            var data = { "name": $scope.name };
            
            $http.post('api/organization', data).success(function (result) {
                notify.addSuccessMessage("Organisationen " + $scope.name + " er blevet oprettet!");

                $scope.name = "";
            }).error(function (result) {
                notify.addErrorMessage("Organisationen " + $scope.name + " kunne ikke oprettes!");
            });
        };
    }]);

    app.controller('globalAdmin.NewLocalAdminCtrl', ['$rootScope', '$scope', '$http', 'notify', function ($rootScope, $scope, $http, notify) {
        $rootScope.page.title = 'Ny lokal admin';
        $rootScope.page.subnav = subnav;

        $scope.organizations = [];
        $http.get("api/organization").success(function(result) {
            $scope.organizations = result.response;
        });

        $scope.submit = function () {
            if ($scope.addForm.$invalid) return;

            var selectedUser = $scope.selectedUser;
            var orgId = $scope.organization;

            var data = {
                "userId": selectedUser.id,
                "organizationId": orgId
            };

            $http.post('api/localadmin', data).success(function (result) {
                notify.addSuccessMessage(selectedUser.text + " er blevet lokal admin!");

                $scope.selectedUser = null;
                $scope.organization = ""; 
                
            }).error(function (result) {
                notify.addErrorMessage("Fejl! " + selectedUser.text + " blev ikke lokal admin!");
            });
        };
    }]);

    app.controller('globalAdmin.NewGlobalAdminCtrl', ['$rootScope', '$scope', '$http', 'notify', function ($rootScope, $scope, $http, notify) {
        $rootScope.page.title = 'Ny global admin';
        $rootScope.page.subnav = subnav;

        $scope.submit = function () {
            if ($scope.addForm.$invalid) return;

            var selectedUser = $scope.selectedUser;

            var data = {
                "userId": selectedUser.id,
            };

            $http.post('api/globaladmin', data).success(function (result) {
                notify.addSuccessMessage(selectedUser.text + " er blevet global admin!");

                $scope.selectedUser = null;

            }).error(function (result) {
                notify.addErrorMessage("Fejl! " + selectedUser.text + " blev ikke global admin!");
            });
        };
    }]);
    
    app.controller('globalAdmin.LocalAdminsCtrl', ['$rootScope', '$scope', '$http', 'notify', function ($rootScope, $scope, $http, notify) {
        $rootScope.page.title = 'Lokal administratorer';
        $rootScope.page.subnav = subnav;

        $scope.organizations = {};
        $http.get("api/organization").success(function (result) {
            _.each(result.response, function (org) {
                $scope.organizations[org.id] = org;
            });
        });

        $scope.adminRights = [];
        $http.get("api/adminright").success(function(result) {
            _.each(result.response, function (right) {
                right.show = true;
                $scope.adminRights.push(right);
            });
        });

    }]);

})(angular, app);
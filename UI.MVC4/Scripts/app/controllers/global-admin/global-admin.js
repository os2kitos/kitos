(function (ng, app) {
   
    
    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('global-admin', {
            url: '/global-admin',
            templateUrl: 'partials/global-admin/new-municipality.html',
            controller: 'globalAdmin.NewMunicipalityCtrl',
            authRoles: ['GlobalAdmin']
        });

    }]);
    
    app.controller('globalAdmin.NewMunicipalityCtrl', ['$rootScope', '$scope', '$http', 'growl', function ($rootScope, $scope, $http, growl) {
        $rootScope.page.title = 'Ny kommune';
        $rootScope.page.subnav = [
            { state: "global-admin", text: "Opret kommune" }
        ];

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

})(angular, App);
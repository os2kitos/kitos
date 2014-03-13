(function (ng, app) {
   
    
    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('global-admin', {
            url: '/global-admin',
            templateUrl: 'partials/global-admin/new-municipality.html',
            controller: 'globalAdmin.NewMunicipalityCtrl',
            authRoles: ['GlobalAdmin']
        });

    }]);
    
    app.controller('globalAdmin.NewMunicipalityCtrl', ['$rootScope', '$scope', '$http', function ($rootScope, $scope, $http) {
        $rootScope.page.title = 'Ny kommune';
        $rootScope.page.subnav = [
            { state: "global-admin", text: "Opret kommune" }
        ];

        $scope.submit = function() {
            if ($scope.addForm.$invalid) return;

            var data = { "Name": $scope.name };

            $scope.requestSuccess = $scope.requestFailure = false;
            
            $http.post('api/municipality', data).success(function (result) {
                $scope.prevName = $scope.name;
                $scope.name = "";

                $scope.requestSuccess = true;
            }).error(function(result) {

                $scope.prevName = $scope.name;

                $scope.requestFailure = true;
            });
        };
    }]);

})(angular, App);
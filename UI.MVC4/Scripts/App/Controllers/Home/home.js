(function(ng, app) {

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        
        $stateProvider.state('index', {
            url: '/',
            templateUrl: 'partials/home/index.html',
            controller: 'home.IndexCtrl',
            noAuth: true
        }).state('login', {
            url: '/login?to',
            templateUrl: 'partials/home/login.html',
            controller: 'home.LoginCtrl',
            noAuth: true
        }).state('forgot-password', {
            url: '/forgot-password',
            templateUrl: 'partials/home/forgot-password.html',
            controller: 'home.ForgotPasswordCtrl',
            noAuth: true
        }).state('reset-password', {
            url: '/reset-password/:requestId',
            templateUrl: 'partials/home/reset-password.html',
            controller: 'home.ResetPasswordCtrl',
            noAuth: true
        });

    }]);

    app.controller('home.IndexCtrl', ['$rootScope', '$scope', function ($rootScope, $scope) {
        $rootScope.page.title = 'Index';
        $rootScope.page.subnav = [];
    }]);
    
    app.controller('home.LoginCtrl', ['$rootScope', '$scope', '$http', '$state', '$stateParams', function ($rootScope, $scope, $http, $state, $stateParams) {
        $rootScope.page.title = 'Log ind';
        $rootScope.page.subnav = [];
        
        //login
        $scope.submit = function () {
            if ($scope.loginForm.$invalid) return;

            var data = {
                'Email': $scope.email,
                'Password': $scope.password,
                'RememberMe': $scope.remember
            };

            $http.post('api/authorize', data).success(function (result) {

                $rootScope.saveUser(result);

                var to = $stateParams.to ? $stateParams.to : 'index';

                $state.go(to);
            });

        };
    }]);

    app.controller('home.ForgotPasswordCtrl', ['$rootScope', '$scope', '$http', 'growl', function ($rootScope, $scope, $http, growl) {
        $rootScope.page.title = 'Glemt password';
        $rootScope.page.subnav = [];
        
        //submit 
        $scope.submit = function() {
            if ($scope.requestForm.$invalid) return;

            var data = { 'Email': $scope.email };

            $scope.requestSuccess = $scope.requestFailure = false;
            
            $http.post('api/passwordresetrequest', data).success(function(result) {
                growl.addSuccessMessage("En email er blevet sent til " + $scope.email);
                $scope.email = '';

            }).error(function (result) {
                growl.addErrorMessage("Emailadressen kunne ikke findes i systemet!");
            });
        };
    }]);

    app.controller('home.ResetPasswordCtrl', ['$rootScope', '$scope', '$http', '$stateParams', function ($rootScope, $scope, $http, $stateParams) {
        $rootScope.page.title = 'Nyt password';
        $rootScope.page.subnav = [];
        
        var requestId = $stateParams.requestId;
        $http.get('api/passwordresetrequest?requestId=' + requestId).success(function(result) {
            $scope.resetStatus = 'enterPassword';
            $scope.email = result.Response.UserEmail;
        }).error(function() {
            $scope.resetStatus = 'missingRequest';
        });

        $scope.submit = function() {
            if ($scope.resetForm.$invalid) return;

            var data = { 'RequestId': requestId, 'NewPassword': $scope.password };
            
            $http.post('api/authorize?resetPassword', data).success(function (result) {
                $scope.resetStatus = 'success';
                $scope.email = '';
            }).error(function (result) {
                $scope.requestFailure = true;
            });
        };


    }]);

})(angular, app);
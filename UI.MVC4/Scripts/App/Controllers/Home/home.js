(function(ng, app) {

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        
        $stateProvider.state('index', {
            url: '/?login?to',
            templateUrl: 'partials/home/index.html',
            controller: 'home.IndexCtrl',
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

    app.controller('home.IndexCtrl', ['$rootScope', '$scope', '$http', '$state', '$stateParams', 'growl', 'Restangular', function ($rootScope, $scope, $http, $state, $stateParams, growl, Restangular) {
        $rootScope.page.title = 'Index';
        $rootScope.page.subnav = [];
        
        Restangular.one('text', 'intro-head').get().then(function (data) {
            $scope.introhead = data.value;

            $scope.submitIntroHead = function(newValue) {
                data.value = newValue;
                data.put();
            };
        });
        
        Restangular.one('text', 'intro-body').get().then(function (data) {
            $scope.introbody = data.value;
            
            $scope.submitIntroBody = function (newValue) {
                data.value = newValue;
                data.put();
            };
        });

        $scope.highlightLogin = $stateParams.login;

        //login
        $scope.submitLogin = function () {
            if ($scope.loginForm.$invalid) return;

            var data = {
                "email": $scope.email,
                "password": $scope.password,
                "rememberMe": $scope.remember
            };

            $http.post('api/authorize', data).success(function (result) {
                growl.addSuccessMessage("Du er nu logget ind!");

                $rootScope.saveUser(result);

                //redirect to another state or to the org-view state by default
                var to = $stateParams.to ? $stateParams.to : 'org-view';

                $state.go(to);
            }).error(function (result) {
                growl.addErrorMessage("Forkert brugernavn eller password!");
            });

        };
    }]);
    
    app.controller('home.ForgotPasswordCtrl', ['$rootScope', '$scope', '$http', 'growl', function ($rootScope, $scope, $http, growl) {
        $rootScope.page.title = 'Glemt password';
        $rootScope.page.subnav = [];
        
        //submit 
        $scope.submit = function() {
            if ($scope.requestForm.$invalid) return;

            var data = { "email": $scope.email };

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
            $scope.email = result.response.userEmail;
        }).error(function() {
            $scope.resetStatus = 'missingRequest';
        });

        $scope.submit = function() {
            if ($scope.resetForm.$invalid) return;

            var data = { "requestId": requestId, "newPassword": $scope.password };
            
            $http.post('api/authorize?resetPassword', data).success(function (result) {
                $scope.resetStatus = 'success';
                $scope.email = '';
            }).error(function (result) {
                $scope.requestFailure = true;
            });
        };


    }]);

})(angular, app);
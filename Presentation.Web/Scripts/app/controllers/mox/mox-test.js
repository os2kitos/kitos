(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('mox.test', {
            url: '/test',
            templateUrl: 'partials/mox/mox-test.html',
            controller: 'mox.TestCtrl',
            resolve: {
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ]
            }
        });
    }]);

    app.controller('mox.TestCtrl', ['$scope', '$http', 'notify', 'user',
            function ($scope, $http, notify, user) {
                $scope.token = user.uuid;
                $scope.data = { brugerid: user.email };

                $scope.newToken = function() {
                    $http.post('/api/user?token&userId=' + user.id).success(function(data, status) {
                        $scope.token = data.response;
                    });
                };

                $scope.submit = function () {
                    var formData = new FormData();
                    // need to convert our json object to a string version of json otherwise
                    // the browser will do a 'toString()' on the object which will result 
                    // in the value '[Object object]' on the server.
                    formData.append('model', angular.toJson($scope.data));
                    if ($scope.file) {
                        formData.append('file', $scope.file, $scope.file.name);
                    }
                    
                    $http.post($scope.url, formData, {
                        // angular.identity, a bit of Angular magic to parse our FormData object
                        transformRequest: angular.identity,
                        // IMPORTANT!!! You might think this should be set to 'multipart/form-data' 
                        // but this is not true because when we are sending up files the request 
                        // needs to include a 'boundary' parameter which identifies the boundary 
                        // name between parts in this multi-part request and setting the Content-type 
                        // manually will not set this boundary parameter. For whatever reason, 
                        // setting the Content-type to 'undefined' will force the request to automatically
                        // populate the headers properly including the boundary parameter.
                        headers: { 'Content-Type': undefined, 'X-Auth': $scope.token },
                    }).success(function (data, status) {
                        console.log('Submitted with status:' + status);
                    }).error(function(data, status) {
                        console.log('Error ' + status + ' occured: ' + data);
                    });
                };
            }]);
})(angular, app);

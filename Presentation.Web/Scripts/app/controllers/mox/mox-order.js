(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('mox.order', {
            url: '/order',
            templateUrl: 'partials/mox/mox-order.html',
            controller: 'mox.OrderCtrl',
            resolve: {
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ]
            }
        });
    }]);

    app.controller('mox.OrderCtrl', ['$scope', '$http', 'notify', 'user',
            function ($scope, $http, notify, user) {
                $scope.tmplUrl = 'api/mox?organizationId=' + user.currentOrganizationId;
                $scope.data = {};


                $scope.submit = function () {
                    var msg = notify.addInfoMessage("Læser excel ark...", false);
                    var formData = new FormData();
                    // need to convert our json object to a string version of json otherwise
                    // the browser will do a 'toString()' on the object which will result 
                    // in the value '[Object object]' on the server.
                    formData.append('model', angular.toJson($scope.data));
                    if ($scope.file) {
                        formData.append('file', $scope.file, $scope.file.name);
                    }

                    $http.post('/api/mox?organizationId=' + user.currentOrganizationId, formData, {
                        // angular.identity, a bit of Angular magic to parse our FormData object
                        transformRequest: angular.identity,
                        // IMPORTANT!!! You might think this should be set to 'multipart/form-data' 
                        // but this is not true because when we are sending up files the request 
                        // needs to include a 'boundary' parameter which identifies the boundary 
                        // name between parts in this multi-part request and setting the Content-type 
                        // manually will not set this boundary parameter. For whatever reason, 
                        // setting the Content-type to 'undefined' will force the request to automatically
                        // populate the headers properly including the boundary parameter.
                        headers: { 'Content-Type': undefined },
                    }).success(function (data, status) {
                        msg.toSuccessMessage("Excel arket er blevet læst og værdier er blevet sat ind i systemet.");
                        //console.log('Submitted with status:' + status);
                    }).error(function (data, status) {
                        msg.toErrorMessage("Fejl! Der er en fejl i excel arket.");

                        if (status == 409) {
                            $scope.data.showMoxErrors = true;
                            $scope.data.errors = data;
                        } else {
                            $scope.data.showGenericError = true;
                        }

                        //console.log('Error ' + status + ' occured: ' + data);
                    });
                };
            }]);
})(angular, app);

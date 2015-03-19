(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('local-config.import.users', {
                url: '/users',
                templateUrl: 'partials/local-config/import-users.html',
                controller: 'local-config.import.ImportUserCtrl',
                resolve: {
                    user: [
                        'userService', function (userService) {
                            return userService.getUser();
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('local-config.import.ImportUserCtrl', [
        '$rootScope', '$scope', '$http', 'notify', 'config', 'user',
        function ($rootScope, $scope, $http, notify, config, user) {

            $scope.usersUrl = 'api/excel?organizationId=' + user.currentOrganizationId + '&exportUsers';
            $scope.userData = {};

            $scope.submitUsers = function () {
                var msg = notify.addInfoMessage("Læser excel ark...", false);
                var formData = new FormData();
                // need to convert our json object to a string version of json otherwise
                // the browser will do a 'toString()' on the object which will result 
                // in the value '[Object object]' on the server.
                formData.append('model', angular.toJson($scope.userData));
                if ($scope.userFile) {
                    formData.append('userFile', $scope.userFile, $scope.userFile.name);
                }

                $http.post('/api/excel?organizationId=' + user.currentOrganizationId + '&importUsers', formData, {
                    // angular.identity, a bit of Angular magic to parse our FormData object
                    transformRequest: angular.identity,
                    // read why it's undefined at $scope.submitOrg
                    headers: { 'Content-Type': undefined },
                }).success(function (data, status) {
                    msg.toSuccessMessage("Excel arket er blevet læst og værdier er blevet sat ind i systemet.");
                    //console.log('Submitted with status:' + status);
                }).error(function (data, status) {
                    msg.toErrorMessage("Fejl! Der er en fejl i excel arket.");

                    if (status == 409) {
                        $scope.userData.showExcelErrors = true;
                        $scope.userData.errors = data;
                    } else {
                        $scope.userData.showGenericError = true;
                    }
                });
            }
           
        }]
    );
})(angular, app);

(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('local-config.import.contracts', {
                url: '/contracts',
                templateUrl: 'app/components/local-config/import/local-config-import-template.view.html',
                controller: 'local-config.import.ImportContractCtrl',
            });
        }
    ]);

    app.controller('local-config.import.ImportContractCtrl', [
        '$rootScope', '$scope', '$http', 'notify', 'user',
        function ($rootScope, $scope, $http, notify, user) {
            $scope.url = 'api/excel?organizationId=' + user.currentOrganizationId + '&exportContracts';
            $scope.title = 'IT Kontrakter';

            $scope.submit = function () {
                var msg = notify.addInfoMessage("Læser excel ark...", false);
                var formData = new FormData();
                // need to convert our json object to a string version of json otherwise
                // the browser will do a 'toString()' on the object which will result
                // in the value '[Object object]' on the server.
                if ($scope.file) {
                    formData.append('file', $scope.file, $scope.file.name);
                }

                $http.post('/api/excel?organizationId=' + user.currentOrganizationId + '&importContracts', formData, {
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
                }).then(function onSuccess(result) {
                    msg.toSuccessMessage("Excel arket er blevet læst og værdier er blevet sat ind i systemet.");
                    $scope.errorData = {};
                }, function onError(result) {
                    msg.toErrorMessage("Fejl! Der er en fejl i excel arket.");
                    $scope.errorData = {};
                    if (result.status == 409) {
                        $scope.errorData.showExcelErrors = true;
                        $scope.errorData.errors = result.data;
                    } else {
                        $scope.errorData.showGenericError = true;
                    }
                });
            }
        }]
    );
})(angular, app);

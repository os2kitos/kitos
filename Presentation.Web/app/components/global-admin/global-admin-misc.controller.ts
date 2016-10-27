((ng, app) => {
    app.config([
        '$stateProvider', $stateProvider => {
            $stateProvider.state('global-admin.misc', {
                url: '/global-misc',
                templateUrl: 'app/components/global-admin/global-admin-misc.view.html',
                controller: 'globalAdmin.misc',
                authRoles: ['GlobalAdmin']
            });
        }
    ]);

    app.controller('globalAdmin.misc', [
        '$rootScope', '$scope', '$http', 'notify', 'UploadFile', ($rootScope, $scope, $http, notify, UploadFile) => {

            $rootScope.page.title = 'Andet';

            $scope.uploadFile = function () {
                var fileToBeUploaded = $scope.myFile;

                alert(fileToBeUploaded.type);

                console.log(fileToBeUploaded);

                UploadFile.uploadFile(fileToBeUploaded);

            }
        }]);
})(angular, app);

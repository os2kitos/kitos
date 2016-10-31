(function (ng, app) {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("global-admin.misc", {
            url: "/misc",
            templateUrl: "app/components/global-admin/global-admin-misc.view.html",
            controller: "globalAdminMisc",
            authRoles: ["GlobalAdmin"]
        });
    }]);

    app.controller("globalAdminMisc", ["$rootScope", "$scope", "uploadFile", ($rootScope, $scope, $http, notify, uploadFile) => {
        $rootScope.page.title = "Andet";
        $scope.uploadFile = function () {
            var fileToBeUploaded = $scope.myFile;
            uploadFile.uploadFile(fileToBeUploaded);
        };
    }]);
})(angular, app);
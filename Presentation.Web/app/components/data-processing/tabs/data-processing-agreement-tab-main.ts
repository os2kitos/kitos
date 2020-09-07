(function (ng, app) {
    app.config([
        '$stateProvider', $stateProvider => {
            $stateProvider.state("data-processing.edit-agreement.main", {
                url: "/main",
                templateUrl: "app/components/data-processing/tabs/data-processing-agreement-tab-main.view.html",
                controller: "data-processing.EditMain",
                resolve: {
                }
            }); 
        }
    ]);

    app.controller('data-processing.EditMain', ['$scope', '$http', '_', '$stateParams', '$uibModal', 'notify', 'dataProcessingAgreement','hasWriteAccess',
        ($scope, $http, _, $stateParams, $uibModal, notify, dataProcessingAgreement, hasWriteAccess) => {
            $scope.dataProcessing = dataProcessingAgreement;
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.autosaveUrl = `api/v1/data-processing-agreement/${dataProcessingAgreement.id}`;
        }]);
})(angular, app);

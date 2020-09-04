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

    app.controller('data-processing.EditMain',['$scope', '$http', '_', '$stateParams', '$uibModal', 'notify',
            ($scope, $http, _, $stateParams, $uibModal, notify, orgUnits, hasWriteAccess) => {


            }]);
})(angular, app);

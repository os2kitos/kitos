(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.contracts', {
            url: '/contracts',
            templateUrl: 'partials/it-system/tab-contracts.html',
            controller: 'system.EditContracts',
            resolve: {
            }
        });
    }]);

    app.controller('system.EditContracts', ['$rootScope', '$scope', '$http', 
        function ($rootScope, $scope, $http) {

            $scope.updateActiveStatus = function () {
                var mainContract = _.findWhere($scope.usage.contracts, { id: $scope.usage.mainContractId });
                
                $scope.systemActive = mainContract ? mainContract.isActive : false;
            };

            $scope.updateActiveStatus();

        }]);
})(angular, app);
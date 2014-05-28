(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.overview', {
            url: '/overview',
            templateUrl: 'partials/it-contract/it-contract-overview.html',
            controller: 'contract.OverviewCtrl',
            resolve: {
                itContracts: ['$http', 'userService', function ($http, userService) {
                    return userService.getUser().then(function (user) {
                        return $http.get('api/itcontract?overview&organizationId=' + user.currentOrganizationId).then(function (result) {
                            return result.data.response;
                        });
                    });
                }],
                itContractRoles: ['$http', function ($http) {
                    return $http.get("api/itcontractrole/")
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('contract.OverviewCtrl', ['$scope', '$http', 'notify', 'itContracts', 'itContractRoles',
            function ($scope, $http, notify, itContracts, itContractRoles) {
                $scope.itContractRoles = itContractRoles;

                var activeContracts = [];
                $scope.activeContracts = activeContracts;
                
                var inactiveContracts = [];
                $scope.inactiveContracts = inactiveContracts;

                
                //decorates the contracts and adds it to a collection.
                //then repeats recursively for all children
                function visit(contract, collection) {
                    contract.hasChildren = contract.children.length > 0;

                    contract.unfolded = false;

                    contract.toggleFold = function() {
                        contract.unfolded = !contract.unfolded;

                        _.each(contract.children, function(child) {
                            child.show = contract.unfolded;
                        });
                    };

                    collection.push(contract);

                    _.each(contract.children, function(child) {
                        visit(child, collection);
                    });
                }

                _.each(itContracts, function(contract) {
                   contract.show = true;

                    var collection = contract.isActive ? activeContracts : inactiveContracts;

                    visit(contract, collection);
                });

            }]);
})(angular, app);
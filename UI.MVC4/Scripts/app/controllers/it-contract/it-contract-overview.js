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
                function visit(contract, collection, indentation) {
                    contract.hasChildren = contract.children && contract.children.length > 0;
                    contract.indentation = indentation;
                    
                    contract.unfolded = false;

                    contract.status = {
                        max: contract.totalRedStatuses + contract.totalYellowStatuses + contract.totalGreenStatuses,
                        red: contract.totalRedStatuses,
                        yellow: contract.totalYellowStatuses,
                        green: contract.totalGreenStatuses
                    };

                    contract.toggleFold = function() {
                        contract.unfolded = !contract.unfolded;

                        function hide(theContract) {
                            theContract.show = theContract.unfolded = false;
                            _.each(theContract.children, hide);
                        }
                        
                        _.each(contract.children, function(child) {
                            if (contract.unfolded) child.show = true;
                            else hide(child);
                        });
                    };

                    collection.push(contract);

                    _.each(contract.children, function(child) {
                        visit(child, collection, indentation + "     ");
                    });
                }

                _.each(itContracts, function(contract) {
                   contract.show = true;

                    var collection = contract.isActive ? activeContracts : inactiveContracts;

                    visit(contract, collection, "");
                });

            }]);
})(angular, app);
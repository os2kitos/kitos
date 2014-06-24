(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.plan', {
            url: '/plan',
            templateUrl: 'partials/it-contract/it-contract-plan.html',
            controller: 'contract.PlanCtrl',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            }
        });
    }]);

    app.controller('contract.PlanCtrl', ['$scope', '$http', 'notify','user',
            function ($scope, $http, notify, user) {
                $scope.pagination = {
                    skip: 0,
                    take: 20
                };

                $scope.contracts = [];

                //decorates the contracts and adds it to a collection.
                //then repeats recursively for all children
                function visit(contract, indentation) {
                    contract.hasChildren = contract.children && contract.children.length > 0;
                    contract.indentation = indentation;

                    contract.unfolded = false;
                    
                    contract.toggleFold = function () {
                        contract.unfolded = !contract.unfolded;

                        function hide(theContract) {
                            theContract.show = theContract.unfolded = false;
                            _.each(theContract.children, hide);
                        }

                        _.each(contract.children, function (child) {
                            if (contract.unfolded) child.show = true;
                            else hide(child);
                        });
                    };

                    $scope.contracts.push(contract);

                    _.each(contract.children, function (child) {
                        visit(child, indentation + "     ");
                    });
                }

                $scope.$watchCollection('pagination', loadContracts);

                function loadContracts() {

                    // clear lists 
                    $scope.contracts = [];

                    var url = 'api/itcontract?plan&organizationId=' + user.currentOrganizationId;

                    url += '&skip=' + $scope.pagination.skip + "&take=" + $scope.pagination.take;

                    if ($scope.pagination.orderBy) {
                        url += '&orderBy=' + $scope.pagination.orderBy;
                        if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                    }

                    $http.get(url).success(function (result) {
                        _.each(result.response, function (contract) {
                            contract.show = true;

                            visit(contract, "");
                        });
                    }).error(function () {
                        notify.addErrorMessage("Kunne ikke hente kontrakter!");
                    });
                }
            }]);
})(angular, app);
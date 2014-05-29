(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.plan', {
            url: '/plan',
            templateUrl: 'partials/it-contract/it-contract-plan.html',
            controller: 'contract.PlanCtrl',
            resolve: {
                itContracts: ['$http', 'userService', function ($http, userService) {
                    return userService.getUser().then(function (user) {
                        return $http.get('api/itcontract?plan&organizationId=' + user.currentOrganizationId).then(function (result) {
                            return result.data.response;
                        });
                    });
                }]
            }
        });
    }]);

    app.controller('contract.PlanCtrl', ['$scope', '$http', 'notify','itContracts',
            function ($scope, $http, notify, itContracts) {

                var contracts = [];
                $scope.contracts = contracts;

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

                    contracts.push(contract);

                    _.each(contract.children, function (child) {
                        visit(child, indentation + "     ");
                    });
                }

                _.each(itContracts, function (contract) {
                    contract.show = true;

                    visit(contract, "");
                });
            }]);
})(angular, app);
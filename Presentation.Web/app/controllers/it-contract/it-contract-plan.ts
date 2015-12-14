(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-contract.plan", {
            url: "/plan",
            templateUrl: "partials/it-contract/it-contract-plan.html",
            controller: "contract.PlanCtrl",
            resolve: {
                user: ["userService", function (userService) {
                    return userService.getUser();
                }]
            }
        });
    }]);

    app.controller("contract.PlanCtrl", ["$scope", "$http", "notify","user",
            function ($scope, $http, notify, user) {
                $scope.pagination = {
                    search: "",
                    skip: 0,
                    take: 20
                };

                $scope.csvUrl = "api/itcontract?csvplan&organizationId=" + user.currentOrganizationId;

                $scope.activeContracts = [];
                $scope.inactiveContracts = [];

                // decorates the contracts and adds it to a collection.
                // then repeats recursively for all children
                function visit(contract, collection, indentation) {
                    contract.hasChildren = contract.children && contract.children.length > 0;
                    contract.indentation = indentation;

                    contract.unfolded = false;

                    contract.toggleFold = function () {
                        contract.unfolded = !contract.unfolded;

                        function hide(theContract) {
                            theContract.show = theContract.unfolded = false;
                            _.each(theContract.children, hide);
                        }

                        _.each(contract.children, function (child: { show }) {
                            if (contract.unfolded) child.show = true;
                            else hide(child);
                        });
                    };


                    collection.push(contract);

                    _.each(contract.children, function (child) {
                        visit(child, collection, indentation + "     ");
                    });
                }

                $scope.$watchCollection("pagination", function() {
                    // var url = "api/itcontract?csvplan&organizationId=" + user.currentOrganizationId;

                    // url += "&skip=" + $scope.pagination.skip + "&take=" + $scope.pagination.take;

                    // if ($scope.pagination.orderBy) {
                    //     url += "&orderBy=" + $scope.pagination.orderBy;
                    //     if ($scope.pagination.descending) url += "&descending=" + $scope.pagination.descending;
                    // }

                    // if ($scope.pagination.search) url += "&q=" + $scope.pagination.search;
                    // else url += "&q=";

                    // $scope.csvUrl = url;
                    loadContracts();
                });

                function loadContracts() {
                    var url = "api/itcontract?plan&organizationId=" + user.currentOrganizationId;

                    url += "&skip=" + $scope.pagination.skip + "&take=" + $scope.pagination.take;

                    if ($scope.pagination.orderBy) {
                        url += "&orderBy=" + $scope.pagination.orderBy;
                        if ($scope.pagination.descending) url += "&descending=" + $scope.pagination.descending;
                    }

                    if ($scope.pagination.search) url += "&q=" + $scope.pagination.search;
                    else url += "&q=";

                    $http.get(url).success(function (result, status, headers) {
                        var paginationHeader = JSON.parse(headers("X-Pagination"));
                        $scope.totalCount = paginationHeader.TotalCount;

                        // clear list
                        $scope.activeContracts = [];
                        $scope.inactiveContracts = [];

                        _.each(result.response, function (contract: { show; isActive; }) {
                            contract.show = true;

                            // TODO isActive filtering should be handle by backend and not by frontend as here
                            var collection = contract.isActive ? $scope.activeContracts : $scope.inactiveContracts;

                            visit(contract, collection, "");
                        });
                    }).error(function () {
                        notify.addErrorMessage("Kunne ikke hente kontrakter!");
                    });
                }
            }]);
})(angular, app);

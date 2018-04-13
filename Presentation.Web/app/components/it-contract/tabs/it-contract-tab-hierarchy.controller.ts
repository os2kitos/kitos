((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-contract.edit.hierarchy", {
            url: "/hierarchy",
            templateUrl: "app/components/it-contract/tabs/it-contract-tab-hierarchy.view.html",
            controller: "contract.EditHierarchyCtrl",
            resolve: {
                hierarchyFlat: ["$http", "$stateParams", ($http, $stateParams) => $http.get(`api/itcontract/${$stateParams.id}?hierarchy`).then(result => result.data.response)]
            }
        });
    }]);

    app.controller("contract.EditHierarchyCtrl",
        ["$scope", "_", "hierarchyFlat", "$stateParams", "notify", "contract", "hasWriteAccess", "$http", "user",
            ($scope, _, hierarchyFlat, $stateParams, notify, contract, hasWriteAccess, $http, user) => {
                $scope.hierarchy = _.toHierarchy(hierarchyFlat, "id", "parentId", "children");
                $scope.autoSaveUrl = 'api/itcontract/' + $stateParams.id;
                $scope.contract = contract;
                $scope.hasWriteAccess = hasWriteAccess;

                $scope.itContractsSelectOptions = selectLazyLoading('api/itcontract', true, formatContract, ['orgId=' + user.currentOrganizationId]);

                function formatContract(supplier) {
                    return '<div>' + supplier.text + '</div>';
                }

                if (contract.parentId) {
                    $scope.contract.parent = {
                        id: contract.parentId,
                        text: contract.parentName
                    };
                }

                function selectLazyLoading(url, excludeSelf, format, paramAry) {
                    return {
                        minimumInputLength: 1,
                        allowClear: true,
                        placeholder: ' ',
                        formatResult: format,
                        initSelection: function (elem, callback) {
                        },
                        ajax: {
                            data: function (term, page) {
                                return { query: term };
                            },
                            quietMillis: 500,
                            transport: function (queryParams) {
                                var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                                var res = $http.get(url + '?q=' + queryParams.data.query + extraParams).then(queryParams.success);
                                res.abort = function () {
                                    return null;
                                };

                                return res;
                            },

                            results: function (data, page) {
                                var results = [];

                                _.each(data.data.response, function (obj: { id; name; cvr; }) {
                                    if (excludeSelf && obj.id == contract.id)
                                        return; // don't add self to result

                                    results.push({
                                        id: obj.id,
                                        text: obj.name ? obj.name : 'Unavngiven',
                                        cvr: obj.cvr
                                    });
                                });
                                results = _.orderBy(results, x => x.text, 'asc');
                                return { results: results };
                            }
                        }
                    };
                }
            }
        ]
    );
})(angular, app);

(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.paymentmodel', {
            url: '/paymentmodel',
            templateUrl: 'app/components/it-contract/tabs/it-contract-tab-paymentmodel.view.html',
            controller: 'contract.PaymentmodelCtrl',
            resolve: {
                paymentFrequencies: ['$http', function ($http) {
                    return $http.get('odata/LocalPaymentFrequencyTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc').then(function (result) {
                        return result.data.value;
                    });
                }],
                paymentModels: ['$http', function ($http) {
                    return $http.get('odata/LocalPaymentModelTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc').then(function (result) {
                        return result.data.value;
                    });
                }],
                priceRegulations: ['$http', function ($http) {
                    return $http.get('odata/LocalPriceRegulationTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc').then(function (result) {
                        return result.data.value;
                    });
                }]
            }
        });
    }]);

    app.controller('contract.PaymentmodelCtrl', ['$scope', '$http', 'notify', 'user', 'contract', 'paymentFrequencies', 'paymentModels', 'priceRegulations',
        function ($scope, $http, notify, user, contract, paymentFrequencies, paymentModels, priceRegulations) {
                $scope.contract = contract;
                $scope.autosaveUrl = 'api/itcontract/' + contract.id;
                $scope.paymentFrequencies = paymentFrequencies;
                $scope.paymentModels = paymentModels;
                $scope.priceRegulations = priceRegulations;

                $scope.datepickerOptions = {
                    format: "dd-MM-yyyy",
                    parseFormats: ["yyyy-MM-dd"]
                };
                $scope.patchDate = (field, value) => {
                    var date = moment(value, "DD-MM-YYYY");
                    if (value === "") {
                        var payload = {};
                        payload[field] = null;
                        patch(payload, $scope.autosaveUrl + '?organizationId=' + user.currentOrganizationId);
                    } else if (value == null) {
    
                    } else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                        notify.addErrorMessage("Den indtastede dato er ugyldig.");

                    }
                    else {
                        var dateString = date.format("YYYY-MM-DD");
                        var payload = {};
                        payload[field] = dateString;
                        patch(payload, $scope.autosaveUrl + '?organizationId=' + user.currentOrganizationId);
                    }
                }
                function patch(payload, url) {
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    $http({ method: 'PATCH', url: url, data: payload })
                        .success(function () {
                            msg.toSuccessMessage("Feltet er opdateret.");
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                        });
                }
            }]);
})(angular, app);

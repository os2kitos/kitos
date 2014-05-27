(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.deadlines', {
            url: '/deadlines',
            templateUrl: 'partials/it-contract/tab-deadlines.html',
            controller: 'contract.DeadlinesCtrl',
            resolve: {
                optionExtensions: ['$http', function($http) {
                    return $http.get('api/optionextend').then(function(result) {
                        return result.data.response;
                    });
                }],
                terminationDeadlines: ['$http', function ($http) {
                    return $http.get('api/terminationdeadline').then(function (result) {
                        return result.data.response;
                    });
                }],
                paymentFrequencies: ['$http', function ($http) {
                    return $http.get('api/paymentFrequency').then(function (result) {
                        return result.data.response;
                    });
                }],
                paymentModels: ['$http', function ($http) {
                    return $http.get('api/paymentModel').then(function (result) {
                        return result.data.response;
                    });
                }],
                priceRegulations: ['$http', function ($http) {
                    return $http.get('api/priceRegulation').then(function (result) {
                        return result.data.response;
                    });
                }],
                paymentMilestones: ['$http', function ($http) {
                    return $http.get('api/paymentMilestone').then(function (result) {
                        return result.data.response;
                    });
                }]
            }
        });
    }]);

    app.controller('contract.DeadlinesCtrl', ['$scope', '$http', 'notify', 'contract', 'optionExtensions', 'terminationDeadlines', 'paymentFrequencies', 'paymentModels', 'priceRegulations', 'paymentMilestones',
            function ($scope, $http, notify, contract, optionExtensions, terminationDeadlines, paymentFrequencies, paymentModels, priceRegulations, paymentMilestones) {
                $scope.contract = contract;
                $scope.autosaveUrl = 'api/itcontract/' + contract.id;
                $scope.optionExtensions = optionExtensions;
                $scope.terminationDeadlines = terminationDeadlines;
                $scope.paymentFrequencies = paymentFrequencies;
                $scope.paymentModels = paymentModels;
                $scope.priceRegulations = priceRegulations;
                $scope.paymentMilestones = paymentMilestones;

                $scope.save = function (paymentMilestone) {
                    paymentMilestone.itContractId = contract.id;
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    $http.post('api/paymentmilestone', paymentMilestone)
                        .success(function (result) {
                            msg.toSuccessMessage("Gemt.");
                            var obj = result.response;
                            $scope.paymentMilestones.push(obj);
                            delete $scope.paymentMilestone; // clear input fields
                        })
                        .error(function() {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                };
            }]);
})(angular, app);
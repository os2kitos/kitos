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
                paymentMilestones: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get('api/paymentMilestone/' + $stateParams.id + '?contract').then(function (result) {
                        return result.data.response;
                    });
                }],
                handoverTrialTypes: ['$http', function ($http) {
                    return $http.get('api/handoverTrialType').then(function (result) {
                        return result.data.response;
                    });
                }],
                handoverTrials: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get('api/handoverTrial/' + $stateParams.id + '?byContract').then(function (result) {
                        return result.data.response;
                    });
                }]
            }
        });
    }]);

    app.controller('contract.DeadlinesCtrl', ['$scope', '$http', '$timeout', '$state', '$stateParams', 'notify', 'contract', 'optionExtensions', 'terminationDeadlines', 'paymentMilestones', 'handoverTrialTypes', 'handoverTrials', 'user', 'moment',
        function ($scope, $http, $timeout, $state, $stateParams, notify, contract, optionExtensions, terminationDeadlines, paymentMilestones, handoverTrialTypes, handoverTrials, user, moment) {
            $scope.contract = contract;
            $scope.autosaveUrl = 'api/itcontract/' + contract.id;
            $scope.optionExtensions = optionExtensions;
            $scope.terminationDeadlines = terminationDeadlines;
            $scope.paymentMilestones = paymentMilestones;
            $scope.handoverTrialTypes = handoverTrialTypes;
            $scope.handoverTrials = handoverTrials;

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            $scope.saveMilestone = function(paymentMilestone) {
                paymentMilestone.itContractId = contract.id;

                var approvedDate = moment(paymentMilestone.approved, "DD-MM-YYYY");
                if (approvedDate.isValid()) {
                    paymentMilestone.approved = approvedDate.format("YYYY-MM-DD");
                } else {
                    paymentMilestone.approved = null;
                }

                var expectedDate = moment(paymentMilestone.expected, "DD-MM-YYYY");
                if (expectedDate.isValid()) {
                    paymentMilestone.expected = expectedDate.format("YYYY-MM-DD");
                } else {
                    paymentMilestone.expected = null;
                }

                var msg = notify.addInfoMessage("Gemmer...", false);
                $http.post('api/paymentmilestone', paymentMilestone)
                    .success(function(result) {
                        msg.toSuccessMessage("Gemt");
                        var obj = result.response;
                        $scope.paymentMilestones.push(obj);
                        delete $scope.paymentMilestone; // clear input fields
                        $scope.milestoneForm.$setPristine();
                    })
                    .error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                    });
            };

            $scope.deleteMilestone = function(id) {
                var msg = notify.addInfoMessage("Sletter...", false);
                $http.delete('api/paymentmilestone/' + id + '?organizationId=' + user.currentOrganizationId)
                    .success(function() {
                        msg.toSuccessMessage("Slettet");
                        reload();
                    })
                    .error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                    });
            };

            $scope.saveTrial = function(handoverTrial) {
                handoverTrial.itContractId = contract.id;

                var approvedDate = moment(handoverTrial.approved, "DD-MM-YYYY");
                if (approvedDate.isValid()) {
                    handoverTrial.approved = approvedDate.format("YYYY-MM-DD");
                } else {
                    handoverTrial.approved = null;
                }

                var expectedDate = moment(handoverTrial.expected, "DD-MM-YYYY");
                if (expectedDate.isValid()) {
                    handoverTrial.expected = expectedDate.format("YYYY-MM-DD");
                } else {
                    handoverTrial.expected = null;
                }

                var msg = notify.addInfoMessage("Gemmer...", false);
                $http.post('api/handoverTrial', handoverTrial)
                    .success(function(result) {
                        msg.toSuccessMessage("Gemt");
                        var obj = result.response;
                        $scope.handoverTrials.push(obj);
                        delete $scope.handoverTrial; // clear input fields
                        $scope.trialForm.$setPristine();
                    })
                    .error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                    });
            };

            $scope.deleteTrial = function (id) {
                var msg = notify.addInfoMessage("Sletter...", false);
                $http.delete('api/handoverTrial/' + id + '?organizationId=' + user.currentOrganizationId)
                    .success(function() {
                        msg.toSuccessMessage("Slettet");
                        reload();
                    })
                    .error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                    });
            };

            // work around for $state.reload() not updating scope
            // https://github.com/angular-ui/ui-router/issues/582
            function reload() {
                return $state.transitionTo($state.current, $stateParams, {
                    reload: true
                }).then(function () {
                    $scope.hideContent = true;
                    return $timeout(function () {
                        return $scope.hideContent = false;
                    }, 1);
                });
            };
        }]);
})(angular, app);
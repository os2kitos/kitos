(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.relation', {
            url: '/relation',
            templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-relation.view.html',
            controller: 'system.EditRelation',
            resolve: {
            }
        });
    }]);

    app.controller('system.EditRelation', ['$scope', '$http', '$state', '$stateParams', '$timeout', 'itSystemUsage', 'notify', '$uibModal', 
        function ($scope, $http, $state, $stateParams, $timeout, itSystemUsage, notify, $modal) {
            var usageId = itSystemUsage.id;



            $scope.usage = itSystemUsage;


            var modalOpen = false;

            $scope.createRelation = function () {
                console.log("Create Relation called");
                if (modalOpen === false) {
                    modalOpen = true;
                    var modalInstance = $modal.open({

                        windowClass: "modal fade in",
                        templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-relation-modal-view.html",
                        controller: ["$scope", "$uibModalInstance", "$window", 'paymentFrequencies', function ($scope, $modalInstance, $window, paymentFrequencies ) {
                            modalOpen = true;




                            $scope.paymentFrequencies = paymentFrequencies;
                            $scope.relationSystem = "";
                            $scope.relationInterfaces = "";
                            $scope.relationContracts = "";
                            $scope.save = function () {
                                modalOpen = false;
                                $scope.$close(true);
                            }

                            $scope.dismiss = function () {
                                modalOpen = false;
                                $scope.$close(true);
                            }
                            modalOpen = false;
                        }],
                        resolve: {
                            paymentFrequencies: ['$http', function ($http) {
                                return $http.get('odata/LocalPaymentFrequencyTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc').then(function (result) {
                                    return result.data.value;
                                });
                            }],
                        }

                    });
                }

            }

            function reload() {
                $state.go(".", null, { reload: true });
            };
            
        }]);
})(angular, app);

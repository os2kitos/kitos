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
            $scope.systemName = "";
            var modalOpen = false;
 
            $http.get(`api/v1/systemrelations/from/${usageId}`).success(result => {
                console.log(result);
                $scope.relationTabledata = result.response;
            });

            $scope.createRelation = function () {
                console.log("Create Relation called");
                if (modalOpen === false) {
                    modalOpen = true;


                    var modalInstance = $modal.open({
                        windowClass: "modal fade in",
                        templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-relation-modal-view.html",
                        controller: ["$scope", "$uibModalInstance", "$window", 'select2LoadingService', function ($scope, $modalInstance, $window, select2LoadingService) {
                            modalOpen = true;
                            $scope.RelationExposedSystemDataCall = select2LoadingService.loadSelect2WithNamedContent(`api/v1/systemrelations/options/${usageId}/systems-which-can-be-related-to`, true, [`fromSystemUsageId=${usageId}`, `amount=10`], true);
                            $scope.interfaceOptions = "";

                            $scope.ExposedSystemSelected = () => {
                                console.log("Data changed ");
                                if ($scope.RelationExposedSystemData != null) {
                                    console.log("Data valid: " + $scope.RelationExposedSystemData.id + " - " + $scope.RelationExposedSystemData.text);
                                    $http.get(`api/v1/systemrelations/options/${usageId}/in-relation-to/${$scope.RelationExposedSystemData.id}`).success(result => {
                                        console.log(result);
                                        $scope.relationPaymentFrequenciesOptions = result.response.availableFrequencyTypes;
                                        $scope.relationInterfaceOptions = result.response.availableInterfaces;
                                        $scope.relationContractsOptions = result.response.availableContracts;
                                    });


                                } else {
                                    console.log("Data invalid resetting");
                                    $scope.relationPaymentFrequenciesOptions = "";
                                    $scope.relationInterfaceOptions = "";
                                    $scope.relationContractsOptions = "";
                                }
                            }

                            $scope.save = function () {
                                modalOpen = false;
                                var InterfaceId = null;
                                var FrequencyTypeId = null;
                                var ContractId = null;
                                var Reference = "";
                                var Description = "";

                                if (!!$scope.relationInterfacesValue) {
                                    InterfaceId = $scope.relationInterfacesValue.id;
                                }

                                if (!!$scope.relationPaymentFrequenciesValue) {
                                    FrequencyTypeId = $scope.relationPaymentFrequenciesValue.id;
                                }

                                if (!!$scope.relationContractsValue) {
                                    ContractId = $scope.relationContractsValue.id;
                                }
                                if ($scope.relationDescriptionValue !== 'undefined') {
                                    Description = $scope.relationDescriptionValue;
                                }
                                if ($scope.relationReferenceValue !== 'undefined') {
                                    Reference = $scope.relationReferenceValue;
                                }
                                    
                                console.log("Relation Values are ...");
                                console.log("FromUsageId ... " + usageId);
                                console.log("ToUsageId ... " + $scope.RelationExposedSystemData.id);
                                console.log("Description ... " + $scope.relationDescriptionValue );
                                console.log("InterfaceId ... " + InterfaceId);
                                console.log("FrequencyTypeId ... " + FrequencyTypeId);
                                console.log("ContractId ... " + ContractId);
                                console.log("Reference ... " + $scope.relationReferenceValue);

                                const relation = <Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationDTO>{
                                    FromUsageId: usageId,
                                    ToUsageId: $scope.RelationExposedSystemData.id,
                                    Description: Description,
                                    InterfaceId: InterfaceId,
                                    FrequencyTypeId: FrequencyTypeId, 
                                    ContractId: ContractId,
                                    Reference: Reference,
                                }

                                notify.addInfoMessage("Tilføjer relation ...", true);
                                $http.post("api/v1/systemrelations", relation, { handleBusy: true }).success(result => {
                                    notify.addSuccessMessage("´Relation tilføjet");
                                }).error(result => {
                                    notify.addErrorMessage("Kunne ikke tilføje relation");
                                });
                                $scope.$close(true);
                                reload();
                            }

                            $scope.dismiss = function () {
                                modalOpen = false;
                                $scope.$close(true);
                            }
                            modalOpen = false;

                        }],
                        resolve: {

                        }

                    });
                }

            }

            function reload() {
                $state.go(".", null, { reload: true });
            };



        }]);
})(angular, app);

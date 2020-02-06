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

            $http.get(`api/v1/systemrelations/from/${usageId}`).success(result => {
                $scope.relationTabledata = result.response;
            });

            $scope.createRelation = function () {
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
                                      checkIfValueIsStillPresent(result);

                                    });
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
                                    $scope.$close(true);
                                    reload();
                                }).error(result => {
                                    notify.addErrorMessage("er opstod en fejl! Kunne ikke tilføje relation");
                                });

                            }

                            $scope.dismiss = function () {
                                modalOpen = false;
                                $scope.$close(true);
                            }

                            modalOpen = false;

                            function checkIfValueIsStillPresent(result) {

                                if ($scope.relationPaymentFrequenciesValue) {

                                    for (let i = 0; i < result.response.availableFrequencyTypes.length; i++) {

                                        if (result.response.availableFrequencyTypes[i].id === $scope.relationPaymentFrequenciesValue.id) {
                                            $scope.relationPaymentFrequenciesOptions = result.response.availableFrequencyTypes;
                                            $scope.relationPaymentFrequenciesValue = result.response.availableFrequencyTypes[i];
                                            break;
                                        }
                                    }
                                        $scope.relationPaymentFrequenciesOptions = result.response.availableFrequencyTypes;

                                } else {
                                    $scope.relationPaymentFrequenciesOptions = result.response.availableFrequencyTypes;
                                }

                                if ($scope.relationInterfacesValue) {

                                    for (let i = 0; i < result.response.availableInterfaces.length; i++) {

                                        if (result.response.availableInterfaces[i].id === $scope.relationInterfacesValue.id) {
                                            $scope.relationInterfaceOptions = result.response.availableInterfaces;
                                            $scope.relationInterfacesValue = result.response.availableInterfaces[i];
                                            break;
                                        }
                                    }
                                    $scope.relationInterfaceOptions = result.response.availableInterfaces;

                                } else {
                                    $scope.relationInterfaceOptions = result.response.availableInterfaces;
                                }

                                if ($scope.relationContractsValue) {

                                    for (let i = 0; i < result.response.availableContracts.length; i++) {

                                        if (result.response.availableContracts[i].id === $scope.relationContractsValue.id) {
                                            $scope.relationContractsOptions = result.response.availableContracts;
                                            $scope.relationContractsValue = result.response.availableContracts[i];
                                            break;
                                        }
                                    }
                                    $scope.relationContractsOptions = result.response.availableContracts;

                                } else {
                                    $scope.relationContractsOptions = result.response.availableContracts;
                                }

                            }

                        }],
                        resolve: {

                        }

                    });
                }

            }

            function reload() {
                $state.go(".", null, { reload: true });
            };

            $scope.validateReference = function (ref) {
                if (ref !== null) {
                    return Kitos.Utility.Validation.validateUrl(ref);
                }
                return false;
            }

        }]);
})(angular, app);

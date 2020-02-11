(function (ng, app) {
    app.config(['$stateProvider', ($stateProvider) => {
        $stateProvider.state('it-system.usage.relation', {
            url: '/relation',
            templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-relation.view.html',
            controller: 'system.EditRelation'
        });
    }]);

    app.controller('system.EditRelation', ['$scope', '$http', '$state', 'itSystemUsage', 'notify', '$uibModal',
        ($scope, $http, $state, itSystemUsage, notify, $modal) => {
            var usageId = itSystemUsage.id;
            $scope.usage = itSystemUsage;
            var modalOpen = false;
            const maxTextFieldCharCount = 199;
            const shortTextLineCount = 4;

            $http.get(`api/v1/systemrelations/from/${usageId}`).success(result => {

                $scope.relationTabledata = result.response;

                var overviewData: Kitos.Models.ItSystemUsage.Relation.ISystemRelationViewModel[] = new Array();

                for (let i = 0; i < result.response.length; i++) {

                    const relationRow = new Kitos.Models.ItSystemUsage.Relation.SystemRelationViewModel(maxTextFieldCharCount, shortTextLineCount, result.response[i]);

                    overviewData.push(relationRow);
                }

                $scope.relationTableTestData = overviewData;
            });

            $http.get(`api/v1/systemrelations/to/${usageId}`).success(result => {
                var usedByOverviewData: Kitos.Models.ItSystemUsage.Relation.ISystemRelationViewModel[] = new Array();

                for (let i = 0; i < result.response.length; i++) {

                    const relationRow = new Kitos.Models.ItSystemUsage.Relation.SystemRelationViewModel(maxTextFieldCharCount, shortTextLineCount, result.response[i]);
                    usedByOverviewData.push(relationRow);
                }

                $scope.usedByRelations = usedByOverviewData;
            });

            const reload = () => {
                $state.go(".", null, { reload: true });
            };

            $scope.createRelation = () => {
                if (modalOpen === false) {
                    modalOpen = true;

                    $modal.open({
                        windowClass: "modal fade in",
                        templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-relation-modal-view.html",
                        controller: ["$scope", 'select2LoadingService', ($scope, select2LoadingService) => {
                            modalOpen = true;
                            $scope.RelationExposedSystemDataCall = select2LoadingService.loadSelect2(`api/v1/systemrelations/options/${usageId}/systems-which-can-be-related-to`, true, [`fromSystemUsageId=${usageId}`, `amount=10`], true, "nameContent");
                            $scope.interfaceOptions = "";

                            $scope.ExposedSystemSelected = () => {
                                const checkIfValueIsStillPresent = (result) => {

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
                                if ($scope.RelationExposedSystemData != null) {
                                    $http
                                        .get(`api/v1/systemrelations/options/${usageId}/in-relation-to/${$scope.RelationExposedSystemData.id}`)
                                        .success(result => {
                                            checkIfValueIsStillPresent(result);
                                        });
                                }
                            }

                            $scope.save = () => {
                                modalOpen = false;
                                var interfaceId = null;
                                var frequencyTypeId = null;
                                var contractId = null;
                                var reference = "";
                                var description = "";

                                if (!!$scope.relationInterfacesValue) {
                                    interfaceId = $scope.relationInterfacesValue.id;
                                }

                                if (!!$scope.relationPaymentFrequenciesValue) {
                                    frequencyTypeId = $scope.relationPaymentFrequenciesValue.id;
                                }

                                if (!!$scope.relationContractsValue) {
                                    contractId = $scope.relationContractsValue.id;
                                }
                                if (!!$scope.relationDescriptionValue) {
                                    description = $scope.relationDescriptionValue;
                                }
                                if (!!$scope.relationReferenceValue) {
                                    reference = $scope.relationReferenceValue;
                                }

                                const relation = <Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationDTO>{
                                    FromUsageId: usageId,
                                    ToUsageId: $scope.RelationExposedSystemData.id,
                                    Description: description,
                                    InterfaceId: interfaceId,
                                    FrequencyTypeId: frequencyTypeId,
                                    ContractId: contractId,
                                    Reference: reference,
                                }

                                notify.addInfoMessage("Tilføjer relation ...", true);
                                $http.post("api/v1/systemrelations", relation, { handleBusy: true }).success(_ => {
                                    notify.addSuccessMessage("´Relation tilføjet");
                                    $scope.$close(true);
                                    reload();
                                }).error(_ => {
                                    notify.addErrorMessage("er opstod en fejl! Kunne ikke tilføje relation");
                                });

                            }

                            $scope.dismiss = () => {
                                modalOpen = false;
                                $scope.$close(true);
                            }

                            modalOpen = false;
                        }],
                    });
                }

            }

            $scope.expandParagraph = (e) => {
                var element = angular.element(e.currentTarget);
                var para = element.closest('td').find(document.getElementsByClassName("readMoreParagraph"))[0];
                var btn = element[0];

                if (para.getAttribute("style") != null) {
                    para.removeAttribute("style");
                    btn.innerText = "Se mindre";
                }
                else {
                    console.log("Toggling overflow on");
                    para.setAttribute("style", "height: " + shortTextLineCount + "em;overflow: hidden;");
                    btn.innerText = "Se mere";
                }
            }
        }]);
})(angular, app);

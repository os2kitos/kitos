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
            $scope.editRelation = false;
            const maxTextFieldCharCount = 199;
            const shortTextLineCount = 4;

            const reload = () => {
                $http.get(`api/v1/systemrelations/from/${usageId}`).success(result => {

                    $scope.relationTabledata = result.response;

                    var overviewData: Kitos.Models.ItSystemUsage.Relation.ISystemRelationViewModel[] = new Array();

                    for (let i = 0; i < result.response.length; i++) {

                        const relationRow = new Kitos.Models.ItSystemUsage.Relation.SystemRelationViewModel(maxTextFieldCharCount, shortTextLineCount, result.response[i]);

                        overviewData.push(relationRow);
                    }

                    $scope.relationTableTestData = overviewData;
                });
            };

            reload();

            $scope.createRelation = () => {
                if (modalOpen === false) {
                    modalOpen = true;

                    $modal.open({
                        windowClass: "modal fade in",
                        templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-relation-modal-view.html",
                        controller: ["$scope", 'select2LoadingService', ($scope, select2LoadingService) => {
                            modalOpen = true;
                            $scope.RelationExposedSystemDataCall = select2LoadingService.loadSelect2(`api/v1/systemrelations/options/${usageId}/systems-which-can-be-related-to`, true, [`fromSystemUsageId=${usageId}`, `amount=10`], true, "nameContent");
                            $scope.RelationModalState = "Opret relation for  " + itSystemUsage.itSystem.name;
                            $scope.RelationModalViewModel = new Kitos.Models.ItSystemUsage.Relation.SystemRelationModalViewModel(usageId, itSystemUsage.itSystem.name);

                            const exposedSystemChanged = () => {
                                if ($scope.RelationModalViewModel.toSystem != null) {
                                    $http.get(`api/v1/systemrelations/options/${usageId}/in-relation-to/${$scope.RelationModalViewModel.toSystem.id}`)
                                        .success(result => {
                                            const updatedView = $scope.RelationModalViewModel;
                                            updatedView.updateAvailableOptions(result);
                                            $scope.RelationModalViewModel = updatedView;
                                        });
                                }
                            }

                            $scope.ExposedSystemSelectedTrigger = () => {
                                exposedSystemChanged();
                            }

                            $scope.save = () => {
                            const postData = new Kitos.Models.ItSystemUsage.Relation.SystemRelationModelPostDataObject($scope.RelationModalViewModel);
                                notify.addInfoMessage("Tilføjer relation ...", true);
                                $http.post("api/v1/systemrelations", postData , { handleBusy: true }).success(_ => {
                                    notify.addSuccessMessage("´Relation tilføjet");
                                    modalOpen = false;
                                    $scope.$close(true);
                                    reload();
                                }).error(_ => {
                                    notify.addErrorMessage("Der opstod en fejl! Kunne ikke tilføje relation");
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

            $scope.editRelation = (relationId) => {
                if (modalOpen === false) {  
                    modalOpen = true;

                    $modal.open({
                        windowClass: "modal fade in",
                        templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-relation-modal-view.html",
                        controller: ["$scope", 'select2LoadingService', ($scope, select2LoadingService) => {
                            var relationData: Kitos.Models.ItSystemUsage.Relation.ISystemGetRelationDTO;
                            $http.get(`api/v1/systemrelations/from/${usageId}/${relationId}`).success(result => {
                                        console.log("GOT DATA");
                                        console.log(result);
                                        relationData = result;

                            modalOpen = true;
                            $scope.RelationExposedSystemDataCall = select2LoadingService.loadSelect2(`api/v1/systemrelations/options/${usageId}/systems-which-can-be-related-to`, true, [`fromSystemUsageId=${usageId}`, `amount=10`], true, "nameContent");
                            $scope.RelationModalState = "Redigere relation imellem " + relationData.fromUsage.name + " og " + relationData.toUsage.name;

                            var modalModelView = new Kitos.Models.ItSystemUsage.Relation.SystemRelationModalViewModel(usageId, itSystemUsage.itSystem.name);
                            $scope.RelationModalViewModel = modalModelView;

                            const exposedSystemChanged = () => {
                                if ($scope.RelationModalViewModel.toSystem != null) {
                                    $http.get(`api/v1/systemrelations/options/${usageId}/in-relation-to/${$scope.RelationModalViewModel.toSystem.id}`)
                                        .success(result => {
                                            const updatedView = $scope.RelationModalViewModel;
                                            updatedView.updateAvailableOptions(result);
                                            modalModelView.setValuesFrom(relationData);
                                            $scope.RelationModalViewModel = updatedView;
                                        });
                                }
                            }

                            exposedSystemChanged();
                            $scope.ExposedSystemSelectedTrigger = () => {
                                exposedSystemChanged();
                            }
                            $scope.save = () => {
                                //const postData = new Kitos.Models.ItSystemUsage.Relation.SystemRelationModelPostDataObject($scope.relationModalViewModel);
                                notify.addInfoMessage("Tilføjer relation ...", true);
                                //$http.post("api/v1/systemrelations", postData, { handleBusy: true }).success(_ => {
                                //    notify.addSuccessMessage("´Relation tilføjet");
                                //    modalOpen = false;
                                //    $scope.$close(true);
                                //    reload();
                                //}).error(_ => {
                                //    notify.addErrorMessage("Der opstod en fejl! Kunne ikke tilføje relation");
                                //});

                            }

                            $scope.dismiss = () => {
                                modalOpen = false;
                                $scope.$close(true);
                            }

                            modalOpen = false;
                            });





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
                    para.setAttribute("style", "height: " + shortTextLineCount + "em;overflow: hidden;");
                    btn.innerText = "Se mere";
                }
            }
        }]);
})(angular, app);

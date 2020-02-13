﻿(function (ng, app) {
    app.config(['$stateProvider', ($stateProvider) => {
        $stateProvider.state('it-system.usage.relation', {
            url: '/relation',
            templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-relation.view.html',
            controller: 'system.EditRelation'
        });
    }]);

    app.controller('system.EditRelation', ['$scope', '$http', 'itSystemUsage', 'notify', '$uibModal',
        ($scope, $http, itSystemUsage, notify, $modal) => {
            var usageId = itSystemUsage.id;
            $scope.usage = itSystemUsage;
            var modalOpen = false;
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
                                $http.post("api/v1/systemrelations", postData, { handleBusy: true }).success(_ => {
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
                            modalOpen = true;
                            $scope.RelationExposedSystemDataCall = select2LoadingService.loadSelect2(`api/v1/systemrelations/options/${usageId}/systems-which-can-be-related-to`, true, [`fromSystemUsageId=${usageId}`, `amount=10`], true, "nameContent");

                            $http.get(`api/v1/systemrelations/from/${usageId}/${relationId}`).success(result => {
                                var relationData = result.response as Kitos.Models.ItSystemUsage.Relation.ISystemGetRelationDTO;
                                
                                $scope.RelationModalState = "Redigere relation";
                                var modalModelView = new Kitos.Models.ItSystemUsage.Relation.SystemRelationModalViewModel(relationData.fromUsage.id, relationData.fromUsage.name);
                                modalModelView.setTargetSystem(relationData.toUsage.id, relationData.toUsage.name);
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

                                $scope.ExposedSystemSelectedTrigger = () => {
                                    exposedSystemChanged();
                                }
                                exposedSystemChanged();

                            }).error(_ => {
                                notify.addErrorMessage("Det var ikke muligt at redigere denne relation");
                            });


                            $scope.save = () => {
                                var data = $scope.RelationModalViewModel;
                                const postData = new Kitos.Models.ItSystemUsage.Relation.SystemRelationModelPatchDataObject(data);
                                notify.addInfoMessage("Tilføjer relation ...", true);
                                $http.patch("api/v1/systemrelations", postData, { handleBusy: true }).success(_ => {
                                    notify.addSuccessMessage("Relation ændret");
                                    modalOpen = false;
                                    $scope.$close(true);
                                    reload();
                                }).error(_ => {
                                    notify.addErrorMessage("Der opstod en fejl! Kunne ikke redigere relation");
                                });

                            }

                            $scope.delete = () => {
                                $http.delete(`api/v1/systemrelations/from/${usageId}/${$scope.RelationModalViewModel.id}`)
                                    .success(_ => {
                                        notify.addSuccessMessage("Relation slettet");
                                        modalOpen = false;
                                        $scope.$close(true);
                                        reload();
                                    }).error(_ => {
                                        notify.addErrorMessage("Kunne ikke slette relation");
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
                    para.setAttribute("style", "height: " + shortTextLineCount + "em;overflow: hidden;");
                    btn.innerText = "Se mere";
                }
            }
        }]);
})(angular, app);

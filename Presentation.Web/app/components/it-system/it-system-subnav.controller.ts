(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system', {
            url: '/system',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            },
            controller: ['$rootScope', '$http', '$state', '$uibModal', 'notify', 'user', function ($rootScope, $http, $state, $modal, notify, user) {
                $rootScope.page.title = 'IT System';
                $rootScope.page.subnav = [
                    { state: 'it-system.overview', text: "IT systemer" },
                    { state: 'it-system.catalog', text: 'IT System katalog' },
                    { state: 'it-system.interfaceCatalog', text: 'Snitflade katalog' },
                    { state: 'it-system.edit', text: 'IT System', showWhen: 'it-system.edit' },
                    { state: 'it-system.usage', text: 'IT System anvendelse', showWhen: 'it-system.usage' },
                    { state: 'it-system.interface-edit', text: 'Snitflade', showWhen: 'it-system.interface-edit' }
                ];
                $rootScope.page.subnav.buttons = [
                    { func: createSystem, text: 'Opret IT System', style: 'btn-success', icon: 'glyphicon-plus' },
                    { func: createInterface, text: 'Opret Snitflade', style: 'btn-success', icon: 'glyphicon-plus' },
                    { func: removeUsage, text: 'Fjern anvendelse', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-system.usage' },
                    { func: removeSystem, text: 'Slet IT System', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-system.edit' },
                    { func: removeInterface, text: 'Slet Snitflade', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-system.interface-edit' }
                ];

                function createSystem() {
                    var modalInstance = $modal.open({
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: 'modal fade in',
                        templateUrl: 'app/components/it-system/it-system-modal-create.view.html',
                        controller: ['$scope', '$uibModalInstance', function ($scope, $modalInstance) {
                            $scope.formData = {};
                            $scope.uuidPattern = /^[0-9A-Fa-f]{8}\-[0-9A-Fa-f]{4}\-[0-9A-Fa-f]{4}\-[0-9A-Fa-f]{4}\-[0-9A-Fa-f]{12}$/;
                            $scope.type = 'IT System';
                            $scope.checkAvailbleUrl = 'api/itSystem/';

                            $scope.submit = function() {
                                var payload = {
                                    name: $scope.formData.name,
                                    uuid: $scope.formData.uuid,
                                    belongsToId: user.currentOrganizationId,
                                    organizationId: user.currentOrganizationId,
                                    taskRefIds: [],
                                };

                                var msg = notify.addInfoMessage('Opretter system...', false);
                                $http.post('api/itsystem', payload)
                                    .success(function(result) {
                                        msg.toSuccessMessage('Et nyt system er oprettet!');
                                        var systemId = result.response.id;
                                        $modalInstance.close(systemId);
                                    }).error(function() {
                                        msg.toErrorMessage('Fejl! Kunne ikke oprette et nyt system!');
                                    });
                            };
                        }]
                    });

                    modalInstance.result.then(function (id) {
                        // modal was closed with OK
                        $state.go('it-system.edit.interfaces', { id: id });
                    });
                };

                function removeSystem() {
                    if (!confirm('Er du sikker på du vil slette systemet?')) {
                        return;
                    }
                    var systemId = $state.params.id;
                    var msg = notify.addInfoMessage('Sletter IT System...', false);
                    $http.delete('api/itsystem/' + systemId + '?organizationId=' + user.currentOrganizationId)
                        .success(function (result) {
                            msg.toSuccessMessage('IT System  er slettet!');
                            $state.go('it-system.catalog');
                        })
                        .error(function (data, status) {
                            if (status == 409)
                                msg.toErrorMessage('Fejl! IT Systemet er i lokal anvendelse!');
                            else
                                msg.toErrorMessage('Fejl! Kunne ikke slette IT System!');
                        });
                }

                function removeUsage() {
                    if (!confirm('Er du sikker på at du vil fjerne den lokale anvendelse af systemet? Dette sletter ikke systemet, men vil slette alle lokale detaljer vedrørende anvendelsen.')) {
                        return;
                    }
                    var usageId = $state.params.id;
                    var msg = notify.addInfoMessage('Sletter IT System anvendelsen...', false);
                    $http.delete('api/itSystemUsage/' + usageId + '?organizationId=' + user.currentOrganizationId)
                        .success(function (result) {
                            msg.toSuccessMessage('IT System anvendelsen er slettet!');
                            $state.go('it-system.overview');
                        })
                        .error(function () {
                            msg.toErrorMessage('Fejl! Kunne ikke slette IT System anvendelsen!');
                        });
                }

                function createInterface() {
                    var modalInstance = $modal.open({
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: 'modal fade in',
                        templateUrl: 'app/components/it-system/it-interface/it-interface-modal-create.view.html',
                        controller: ['$scope', '$uibModalInstance', function ($scope, $modalInstance) {
                            $scope.formData = { itInterfaceId: "" }; // set itInterfaceId to an empty string
                            $scope.uuidPattern = /^[0-9A-Fa-f]{8}\-[0-9A-Fa-f]{4}\-[0-9A-Fa-f]{4}\-[0-9A-Fa-f]{4}\-[0-9A-Fa-f]{12}$/;
                            $scope.type = 'IT Snitflade';
                            $scope.checkAvailbleUrl = 'api/itInterface/';

                            $scope.validateName = function () {
                                $scope.createForm.name.$validate();
                            }
                            $scope.validateItInterfaceId = function () {
                                $scope.createForm.itInterfaceId.$validate();
                            }

                            $scope.uniqueConstraintError = false;

                            $scope.submit = function () {
                                var payload = {
                                    name: $scope.formData.name,
                                    uuid: $scope.formData.uuid,
                                    itInterfaceId: $scope.formData.itInterfaceId,
                                    belongsToId: user.currentOrganizationId,
                                    organizationId: user.currentOrganizationId
                                };

                                var msg = notify.addInfoMessage('Opretter snitflade...', false);
                                $http.post('api/itinterface', payload)
                                    .success(function (result) {
                                        msg.toSuccessMessage('En ny snitflade er oprettet!');
                                        var interfaceId = result.response.id;
                                        $modalInstance.close(interfaceId);
                                    }).error(function () {
                                        msg.toErrorMessage('Fejl! Kunne ikke oprette snitflade!');
                                    });
                            };
                        }]
                    });

                    modalInstance.result.then(function (id) {
                        // modal was closed with OK
                        $state.go('it-system.interface-edit.interface-details', { id: id });
                    });
                }

                function removeInterface() {
                    if (!confirm('Er du sikker på du vil slette snitfladen?')) {
                        return;
                    }
                    var interfaceId = $state.params.id;
                    var msg = notify.addInfoMessage('Sletter Snitflade...', false);
                    $http.delete('api/itinterface/' + interfaceId + '?organizationId=' + user.currentOrganizationId)
                        .success(function (result) {
                            msg.toSuccessMessage('Snitflade er slettet!');
                            $state.go('it-system.interfaceCatalog');
                        })
                        .error(function (data, status) {
                            if (status == 409)
                                msg.toErrorMessage('Fejl! Kan ikke slette snitflade, den er tilknyttet et IT System, som er i lokal anvendelse!');
                            else
                                msg.toErrorMessage('Fejl! Kunne ikke slette Snitfladen!');
                        });
                }
            }]
        });
    }]);
})(angular, app);

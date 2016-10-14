(function(ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract', {
            url: '/contract',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            },
            controller: ['$rootScope', '$http', '$state', '$uibModal', 'notify', 'user', function ($rootScope, $http, $state, $modal, notify, user) {
                $rootScope.page.title = 'IT Kontrakt';
                $rootScope.page.subnav = [
                    { state: 'it-contract.overview', text: "IT kontrakter: økonomi" },
                    { state: 'it-contract.plan', text: "IT kontrakter: tid" },
                    { state: 'it-contract.edit', text: 'IT Kontrakt', showWhen: 'it-contract.edit' },
                ];
                $rootScope.page.subnav.buttons = [
                    { func: create, text: 'Opret IT Kontrakt', style: 'btn-success', icon: 'glyphicon-plus' },
                    { func: remove, text: 'Slet IT Kontrakt', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-contract.edit' }
                ];

                function create() {

                    var self = this;

                    var modalInstance = $modal.open({
                        windowClass: 'modal fade in',
                        templateUrl: 'app/components/it-contract/it-contract-modal-create.view.html',
                         controller: ['$scope', '$uibModalInstance', function ($scope, $modalInstance) {
                            $scope.formData = {};
                            $scope.type = 'IT Kontrakt';
                            $scope.checkAvailbleUrl = 'api/itProject/';

                            $scope.saveAndProceed = function () {

                                var orgId = user.currentOrganizationId;
                                var msg = notify.addInfoMessage("Opretter kontrakt...", false);

                                $http.post('api/itcontract', { organizationId: orgId, name: $scope.formData.name })
                                    .success(function (result) {
                                        msg.toSuccessMessage("En ny kontrakt er oprettet!");
                                        var contract = result.response;
                                        $modalInstance.close(contract.id);
                                        $state.go('it-contract.edit.systems', { id: contract.id });
                                    })
                                    .error(function () {
                                        msg.toErrorMessage("Fejl! Kunne ikke oprette en ny kontrakt!");
                                    });
                            };

                            $scope.save = function () {

                                var orgId = user.currentOrganizationId;
                                var msg = notify.addInfoMessage("Opretter kontrakt...", false);

                                $http.post('api/itcontract', { organizationId: orgId, name: $scope.formData.name })
                                    .success(function (result) {
                                        msg.toSuccessMessage("En ny kontrakt er oprettet!");
                                        var contract = result.response;
                                        $modalInstance.close(contract.id);
                                        $state.reload();
                                    })
                                    .error(function () {
                                        msg.toErrorMessage("Fejl! Kunne ikke oprette en ny kontrakt!");
                                    });
                            };
                        }]
                    });
                }

                function remove() {
                    if (!confirm("Er du sikker på du vil slette kontrakten?")) {
                        return;
                    }
                    var contractId = $state.params.id;
                    var msg = notify.addInfoMessage("Sletter IT Kontrakten...", false);
                    $http.delete('api/itcontract/' + contractId + '?organizationId=' + user.currentOrganizationId)
                        .success(function (result) {
                            msg.toSuccessMessage("IT Kontrakten er slettet!");
                            $state.go('it-contract.overview');
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke slette IT Kontrakten!");
                        });
                }
            }]
        });
    }]);
})(angular, app);

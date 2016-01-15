(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('global-admin.organizations.create', {
                url: '/create',
                authRoles: ['GlobalAdmin'],
                onEnter: ['$state', '$stateParams', '$uibModal',
                    function ($state, $stateParams, $modal) {
                        $modal.open({
                            templateUrl: 'app/components/global-admin/global-admin-organization-modal.view.html',
                            // fade in instead of slide from top, fixes strange cursor placement in IE
                            // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                            windowClass: 'modal fade in',
                            controller: 'globalAdmin.createOrganizationCtrl',
                        }).result.then(function () {
                            // OK
                            // GOTO parent state and reload
                            $state.go('^', null, { reload: true });
                        }, function () {
                            // Cancel
                            // GOTO parent state
                            $state.go('^');
                        });
                    }
                ]
            });
        }
    ]);

    app.controller('globalAdmin.createOrganizationCtrl', [
        '$rootScope', '$scope', '$http', 'notify', function ($rootScope, $scope, $http, notify) {
            $rootScope.page.title = 'Ny organisation';
            $scope.title = 'Opret organisation';

            function init() {
                $scope.org = {};
                $scope.org.accessModifier = 0;
                $scope.org.type = 1; // set type to municipality by default
            };

            init();

            $scope.dismiss = function () {
                $scope.$dismiss();
            };

            $scope.submit = function () {
                var payload = $scope.org;
                $http.post('api/organization', payload)
                    .success(function (result) {
                        notify.addSuccessMessage("Organisationen " + result.response.name + " er blevet oprettet!");
                        $scope.$close(true);
                    })
                    .error(function (result) {
                        notify.addErrorMessage("Organisationen " + $scope.org.name + " kunne ikke oprettes!");
                    });
            };
        }
    ]);
})(angular, app);

(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('global-admin.organizations.edit', {
                url: '/edit/:id',
                authRoles: ['GlobalAdmin'],
                onEnter: ['$state', '$stateParams', '$modal',
                    function($state, $stateParams, $modal) {
                        $modal.open({
                            templateUrl: 'partials/global-admin/organization-modal.html',
                            // fade in instead of slide from top, fixes strange cursor placement in IE
                            // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                            windowClass: 'modal fade in',
                            resolve: {
                                org: [
                                    '$http', function($http) {
                                        return $http.get('api/organization/' + $stateParams.id).then(function(result) {
                                            return result.data.response;
                                        });
                                    }
                                ],
                                user: [
                                    'userService', function(userService) {
                                        return userService.getUser();
                                    }
                                ]
                            },
                            controller: 'globalAdmin.editOrganizationCtrl',
                        }).result.then(function() {
                            // OK
                            // GOTO parent state and reload
                            $state.go('^', null, { reload: true });
                        }, function() {
                            // Cancel
                            // GOTO parent state
                            $state.go('^');
                        });
                    }
                ]
            });
        }
    ]);

    app.controller('globalAdmin.editOrganizationCtrl', [
        '$rootScope', '$scope', '$http', 'notify', 'org', 'user', function ($rootScope, $scope, $http, notify, org, user) {
            $rootScope.page.title = 'Rediger organisation';
            $scope.title = 'Rediger organisation';
            $scope.org = org;

            $scope.dismiss = function () {
                $scope.$dismiss();
            };

            $scope.submit = function () {
                var payload = {
                    name: $scope.org.name,
                    accessModifier: $scope.org.accessModifier,
                    cvr: $scope.org.cvr,
                    type: $scope.org.type
                };

                $http({
                    method: 'PATCH',
                    url: 'api/organization/' + org.id + '?organizationId=' + user.currentOrganizationId,
                    data: payload
                }).success(function(result) {
                    notify.addSuccessMessage("Ændringerne er blevet gemt!");
                    $scope.$close(true);
                }).error(function(result) {
                    notify.addErrorMessage("Ændringerne kunne ikke gemmes!");
                });
            };
        }
    ]);
})(angular, app);

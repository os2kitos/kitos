(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('organization', {
            url: '/organization',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }],
                organizationRoles: [
                        '$http', function ($http) {
                            return $http.get('api/adminrole').then(function (result) {
                                return result.data.response;
                            });
                        }
                ],
            },
            controller: ['$rootScope', '$uibModal', '$state', 'user', 'organizationRoles', function ($rootScope, $modal, $state, user, organizationRoles) {
                $rootScope.page.title = 'Organisation';

                var subnav = [];

                if (user.currentConfig.showTabOverview) {
                    subnav.push({ state: 'organization.overview', text: 'Overblik' });
                }

                subnav.push({ state: 'organization.structure', text: 'Organisation' });
                subnav.push({ state: 'organization.user', text: 'Bruger' });

                $rootScope.page.subnav = subnav;

                $rootScope.page.subnav.buttons = [
                    { func: createUser, text: 'Opret Bruger', style: 'btn-success', icon: 'glyphicon-plus', showWhen: 'organization.user' }
                ];

                function reload() {
                    $state.go('.', null, { reload: true });
                }

                var orgUserRole = _.find(organizationRoles, function (role) { return role.name == 'Medarbejder'; });


                function createUser() {
                    var modal = $modal.open({
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: 'modal fade in',
                        templateUrl: 'partials/org/user/org-createuser-modal.html',
                        controller: ['$scope', '$uibModalInstance', '$http', 'notify', 'autofocus', function ($modalScope, $modalInstance, $http, notify, autofocus) {
                            if (!orgUserRole && !user.currentOrganizationId) {
                                notify.addErrorMessage("Fejl! Kunne ikke oprette bruger.", true);
                                return;
                            }

                            $modalScope.checkAvailbleUrl = 'api/user';
                            $modalScope.checkOrgUserUrl = 'api/user';

                            autofocus();
                            $modalScope.busy = false;
                            $modalScope.create = function (sendMail) {
                                $modalScope.busy = true;
                                var newUser = {
                                    name: $modalScope.name,
                                    email: $modalScope.email,
                                    lastName: $modalScope.lastName,
                                    phoneNumber: $modalScope.phoneNumber
                                };
                                
                                var params = { organizationId: user.currentOrganizationId };
                                // set params if sendMail is true
                                if (sendMail) {
                                    params.sendMailOnCreation = sendMail;
                                }
                                
                                var msg = notify.addInfoMessage("Opretter bruger", false);
                                $http.post("api/user", newUser, { handleBusy: true, params: params }).success(function (result, status) {
                                    var userResult = result.response;
                                    var oId = user.currentOrganizationId;

                                    var data = {
                                        userId: userResult.id,
                                        roleId: orgUserRole.id,
                                    };

                                    $http.post("api/adminrights/?rightByOrganizationRight&organizationId=" + oId + "&userId=" + user.id, data, { handleBusy: true }).success(function (result) {
                                        msg.toSuccessMessage(userResult.fullName + " er oprettet i KITOS");
                                        reload();
                                    }).error(function() {
                                        msg.toErrorMessage("Kunne ikke tilknytte " + user.fullName + ' til organisationen!');
                                    });

                                    $modalInstance.close(userResult);
                                }).error(function (result) {
                                    msg.toErrorMessage("Fejl! Noget gik galt ved oprettelsen af " + newUser.name + "!");
                                    $modalInstance.close();
                                });
                            };

                            $modalScope.cancel = function () {
                                $modalInstance.close();
                            };
                        }]
                    });

                    modal.result.then(
                        function () {
                            reload();
                        });
                };
            }]
        });
    }]);
})(angular, app);
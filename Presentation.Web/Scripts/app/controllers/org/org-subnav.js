(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('organization', {
            url: '/organization',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            },
            controller: ['$rootScope', '$modal', '$state', 'user', function ($rootScope, $modal, $state, user) {
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

                function createUser() {
                    var modal = $modal.open({
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: 'modal fade in',
                        templateUrl: 'partials/org/user/org-createuser-modal.html',
                        controller: ['$scope', '$modalInstance', '$http', 'notify', 'autofocus', function ($modalScope, $modalInstance, $http, notify, autofocus) {
                            autofocus();
                            $modalScope.busy = false;

                            $modalScope.create = function (sendMail) {
                                $modalScope.busy = true;
                                var newUser = {
                                    name: $modalScope.name,
                                    email: $modalScope.email,
                                    createdInId: user.currentOrganizationId
                                };
                                var params = sendMail ? { sendMailOnCreation: sendMail } : null; //set params if sendMail is true

                                var msg = notify.addInfoMessage("Opretter bruger", false);
                                $http.post("api/user", newUser, { handleBusy: true, params: params }).success(function (result, status) {
                                    var userResult = result.response;
                                    if (status == 201) {
                                        msg.toSuccessMessage(userResult.name + " er oprettet i KITOS");
                                    } else {
                                        msg.toInfoMessage("En bruger med den email-adresse fandtes allerede i systemet.");
                                    }

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
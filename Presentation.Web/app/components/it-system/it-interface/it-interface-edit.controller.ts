(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.interface-edit', {
            url: '/edit/{id:[0-9]+}/interface',
            templateUrl: 'app/components/it-system/it-interface/it-interface-edit.view.html',
            controller: 'system.interfaceEditCtrl',
            resolve: {
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ],
                hasWriteAccess: [
                    '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                        var interfaceId = $stateParams.id;
                        return $http.get('api/itInterface/' + interfaceId + '?hasWriteAccess=true&organizationId=' + user.currentOrganizationId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ],
                itInterface: [
                    '$http', '$stateParams', function ($http, $stateParams) {
                        var interfaceId = $stateParams.id;
                        return $http.get('api/itInterface/' + interfaceId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ]
            }
        });
    }]);

    app.controller('system.interfaceEditCtrl', ['$rootScope', '$scope', 'user', 'hasWriteAccess', 'itInterface', '$state', 'notify', '$http', '_',
        function ($rootScope, $scope, user, hasWriteAccess, itInterface, $state, notify, $http, _) {

            if (user.isGlobalAdmin) {
                if (!itInterface.disabled) {
                    var buttonAlreadyExists = _.find($rootScope.page.subnav.buttons, function (o) {
                        return o.text === "Deaktivér snitflade";
                    });

                    if (ng.isUndefined(buttonAlreadyExists)) {
                        $rootScope.page.subnav.buttons.push(
                            { func: disableInterface, text: 'Deaktivér snitflade', style: 'btn-danger', showWhen: 'it-system.interface-edit' }
                        );
                    }
                } else {
                    var buttonAlreadyExists = _.find($rootScope.page.subnav.buttons, function (o) {
                        return o.text === "Aktivér snitflade";
                    });

                    if (ng.isUndefined(buttonAlreadyExists)) {
                        $rootScope.page.subnav.buttons.push(
                            { func: enableInterface, text: 'Aktivér snitflade', style: 'btn-success', showWhen: 'it-system.interface-edit' }
                        );
                    }
                }
            }

            function disableInterface() {
                if (!confirm('Er du sikker på du vil deaktivere snitfladen?')) {
                    return;
                }

                var payload: any = {};
                payload.Disabled = true;

                var msg = notify.addInfoMessage('Deaktiverer snitflade...', false);
                $http.patch('odata/ItInterfaces(' + itInterface.id + ')', payload)
                    .success(function (result) {
                        msg.toSuccessMessage('Snitflade er deaktiveret!');
                        $state.reload();
                    })
                    .error(function (data, status) {
                        msg.toErrorMessage('Fejl! Kunne ikke deaktivere snitflade!');
                    });
            }

            function enableInterface() {
                if (!confirm('Er du sikker på du vil aktivere snitflade?')) {
                    return;
                }
                var payload: any = {};
                payload.Disabled = false;

                var msg = notify.addInfoMessage('Aktiverer snitflade...', false);
                $http.patch('odata/ItInterfaces(' + itInterface.id + ')', payload)
                    .success(function (result) {
                        msg.toSuccessMessage('Snitflade er aktiveret!');
                        $state.reload();
                    })
                    .error(function (data, status) {
                        msg.toErrorMessage('Fejl! Kunne ikke aktivere snitflade!');
                    });
            }
        }
    ]);
})(angular, app);

(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.contracts', {
            url: '/contracts',
            templateUrl: 'partials/it-system/tab-contracts.html',
            controller: 'system.EditContracts',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }],
                
            }
        });
    }]);

    app.controller('system.EditWishes', ['$rootScope', '$scope', '$http', '$state', '$stateParams', 'notify', 'wishes', 'user',
        function ($rootScope, $scope, $http, $state, $stateParams, notify, wishes, user) {
            $scope.user = user;
            $scope.wishes = wishes;

            function clear() {
                $scope.text = '';
                $scope.isPublic = false;
            }

            $scope.save = function () {
                var payload = {
                    text: $scope.text,
                    isPublic: $scope.isPublic,
                    userId: user.id,
                    itSystemUsageId: $stateParams.id
                };

                var msg = notify.addInfoMessage("Gemmer... ");
                $http.post('api/wish', payload).success(function () {
                    msg.toSuccessMessage("Ønsket er gemt!");
                    clear();
                    $state.transitionTo($state.current, $stateParams, {
                        reload: true,
                        inherit: false,
                        notify: true
                    });
                }).error(function () {
                    msg.toErrorMessage("Fejl! Ønsket kunne ikke gemmes!");
                });
            };
        }]);
})(angular, app);
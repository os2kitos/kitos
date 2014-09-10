(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.wishes', {
            url: '/wishes',
            templateUrl: 'partials/it-system/tab-wishes.html',
            controller: 'system.EditWishes',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }],
                wishes: ['$http', '$stateParams', 'userService', function ($http, $stateParams, userService) {
                    return userService.getUser().then(function (user) {
                        return $http.get('api/wish/?userId=' + user.id + '&usageId=' + $stateParams.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    });
                }]
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

            $scope.delete = function(id) {
                var msg = notify.addInfoMessage("Gemmer... ");
                $http.delete('api/wish/' + id).success(function() {
                    msg.toSuccessMessage("Ønsket er slettet!");
                    $state.go('.', null, { reload: true });
                }).error(function() {
                    msg.toErrorMessage("Fejl! Ønsket kunne ikke slettes!");
                });
            }
        }]);
})(angular, app);
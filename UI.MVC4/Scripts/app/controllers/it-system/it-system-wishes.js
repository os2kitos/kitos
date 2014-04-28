(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.wishes', {
            url: '/wishes',
            templateUrl: 'partials/it-system/tab-wishes.html',
            controller: 'system.EditWishes',
            resolve: {
                wishes: ['$rootScope', '$http', '$stateParams', function ($rootScope, $http, $stateParams) {
                    return $http.get('api/wish/?userId='+ $rootScope.user.id + '&usageId=' + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('system.EditWishes', ['$rootScope', '$scope', '$http', '$state', '$stateParams', 'notify', 'wishes', function ($rootScope, $scope, $http, $state, $stateParams, notify, wishes) {
        $scope.user = $rootScope.user;
        $scope.wishes = wishes;

        function clear() {
            $scope.text = '';
            $scope.isPublic = false;
        }

        $scope.save = function () {
            var payload = {
                text: $scope.text,
                isPublic: $scope.isPublic,
                userId: $rootScope.user.id,
                itSystemUsageId: $stateParams.id
            };

            var msg = notify.addInfoMessage("Gemmer... ");
            $http.post('api/wish', payload).success(function() {
                msg.toSuccessMessage("Ønsket er gemt!");
                clear();
                $state.transitionTo($state.current, $stateParams, {
                    reload: true,
                    inherit: false,
                    notify: true
                });
            }).error(function() {
                msg.toErrorMessage("Fejl! Ønsket kunne ikke gemmes!");
            });
        };
    }]);
})(angular, app);
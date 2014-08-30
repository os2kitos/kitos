(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.contracts', {
            url: '/contracts',
            templateUrl: 'partials/it-system/tab-contracts.html',
            controller: 'system.EditContracts',
            resolve: {
            }
        });
    }]);

    app.controller('system.EditContracts', ['$scope', '$state', '$stateParams', '$timeout', 'itSystemUsage',
        function ($scope, $state, $stateParams, $timeout, itSystemUsage) {
            $scope.reload = function() {
                $timeout(reload, 1000); // OMG HACK! TODO refactor! This is to wait for the autosave to finish then reload the view to reflect the change
            }

            // work around for $state.reload() not updating scope
            // https://github.com/angular-ui/ui-router/issues/582
            function reload() {
                return $state.transitionTo($state.current, $stateParams, {
                    reload: true
                }).then(function () {
                    $scope.hideContent = true;
                    return $timeout(function () {
                        return $scope.hideContent = false;
                    }, 1);
                });
            };
        }]);
})(angular, app);
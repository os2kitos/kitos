(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.hierarchy', {
            url: '/hierarchy',
            templateUrl: 'partials/it-system/tab-hierarchy.html',
            controller: 'system.EditHierarchy',
            
        });
    }]);

    app.controller('system.EditHierarchy', ['$rootScope', '$scope', '$http', '$state', '$stateParams', 'notify', 'wishes', function ($rootScope, $scope, $http, $state, $stateParams, notify, wishes) {
        $scope.user = $rootScope.user;
        $scope.wishes = wishes;

        
    }]);
})(angular, app);
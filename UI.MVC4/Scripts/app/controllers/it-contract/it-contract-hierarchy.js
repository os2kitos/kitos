(function(ng, app) {
    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('it-contract.edit.hierarchy', {
            url: '/hierarchy',
            templateUrl: 'partials/it-contract/tab-hierarchy.html',
            controller: 'contract.EditHierarchyCtrl',
            resolve: {
                
            }
        });
    }]);

    app.controller('contract.EditHierarchyCtrl',
        ['$scope', '$http', 'notify',
            function($scope, $http, notify) {

            }
        ]
    );
})(angular, app);
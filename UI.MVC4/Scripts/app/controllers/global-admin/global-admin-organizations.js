(function(ng, app) {
    app.config([
        '$stateProvider', function($stateProvider) {
            $stateProvider.state('global-admin.organisations', {
                url: '/organisations',
                templateUrl: 'partials/global-admin/new-organization.html',
                controller: 'globalAdmin.organizationCtrl',
                authRoles: ['GlobalAdmin']
            });
        }
    ]);

    app.controller('globalAdmin.organizationCtrl', [
        '$rootScope', '$scope', '$http', 'notify', function($rootScope, $scope, $http, notify) {
            $rootScope.page.title = 'Ny organisation';

            function init() {
                $scope.org = {};
                $scope.org.accessModifier = 0;
                $scope.org.type = 1; // set type to municipality by default
            };

            init();

            $scope.submit = function() {
                if ($scope.addForm.$invalid) return;

                var payload = $scope.org;
                $http.post('api/organization', payload)
                    .success(function(result) {
                        notify.addSuccessMessage("Organisationen " + result.response.name + " er blevet oprettet!");
                        delete $scope.org;
                        init();
                    })
                    .error(function(result) {
                        notify.addErrorMessage("Organisationen " + $scope.org.name + " kunne ikke oprettes!");
                    });
            };
        }
    ]);
})(angular, app);

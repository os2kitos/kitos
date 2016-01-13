(function(ng, app) {
    'use strict';

    app.directive('selectAccessModifier', [
        function() {
            return {
                priority: 1,
                replace: true,
                templateUrl: 'app/shared/selectAccessModifier/selectAccessModifier.view.html',
                controller: [
                    '$scope', 'userService', function($scope, userService) {
                        userService.getUser().then(function(user) {
                            $scope.isGlobalAdmin = user.isGlobalAdmin;
                        });
                    }
                ]
            };
        }
    ]);
})(angular, app);

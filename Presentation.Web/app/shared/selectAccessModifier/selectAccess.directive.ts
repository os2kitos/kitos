(function(ng, app) {
    'use strict';

    app.controller("selectAccessController", ["$scope", "select2LoadingService", function ($scope, select2LoadingService) {
        var secondOptionText = $scope.isGlobalAdmin === true || $scope.isGlobalAdmin === 'true' ? "Offentlig" : "Offentlig (kun systemadministrator)";
        var options = [
            { id: "0", text: "Lokal" },
            { id: "1", text: secondOptionText, disabled: $scope.isGlobalAdmin !== 'true' && $scope.isGlobalAdmin !== true }
        ];

        $scope.select2Config = select2LoadingService.select2LocalDataNoSearch(() => options, true);
    }]);

    app.directive('selectAccess', [
        function() {
            return {
                templateUrl: 'app/shared/selectAccessModifier/selectAccess.view.html',
                scope: {
                    id: "@",
                    label: "@",
                    selected: "=ngModel",
                    autoSaveUrl: "@",
                    field: "@",
                    disabled: "=ngDisabled",
                    isGlobalAdmin: "@"
                },
                controller: "selectAccessController"
            };
        }
    ]);
})(angular, app);

(function(ng, app) {
    "use strict";

    app.directive("displayAccessModifier", [
        function() {
            return {
                template: `{{ friendlyName }}`,
                scope: {
                    value: "="
                },
                controller: ["$scope", function ($scope) {
                    switch ($scope.value) {
                        case Kitos.Models.AccessModifier.Local: $scope.friendlyName = "Lokal"; break;
                        case Kitos.Models.AccessModifier.Public: $scope.friendlyName = "Offentlig"; break;
                        default: $scope.friendlyName = "Ukendt";
                    }
                }]
            };
        }
    ]);
})(angular, app);

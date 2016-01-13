(function(ng, app) {
    'use strict';

    app.directive('suggestNewRole', [
        '$http', 'notify', function($http, notify) {
            return {
                scope: {
                    url: '@'
                },
                templateUrl: 'app/components/local-config/suggestNewRole/suggestNewRole.view.html',
                link: function(scope, element, attrs) {
                    scope.suggest = function() {

                        var data = {
                            "isSuggestion": true,
                            "name": scope.suggestion,
                            "hasReadAccess": true,
                            "hasWriteAccess": scope.writeAccess
                        };

                        $http.post(scope.url, data).success(function(result) {
                            notify.addSuccessMessage('Foreslag sendt!');
                            scope.suggestion = "";
                        }).error(function(result) {
                            notify.addErrorMessage('Kunne ikke sende foreslag!');
                        });
                    };
                }
            };
        }
    ]);
})(angular, app);

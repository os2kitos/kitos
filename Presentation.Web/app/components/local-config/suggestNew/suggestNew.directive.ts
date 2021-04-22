(function (ng, app) {
    'use strict';

    app.directive('suggestNew', [
        '$http', 'notify', function ($http, notify) {
            return {
                scope: {
                    url: '@'
                },
                templateUrl: 'app/components/local-config/suggestNew/suggestNew.view.html',
                link: function (scope, element, attrs) {
                    scope.suggest = function () {
                        var data = {
                            "isSuggestion": true,
                            "name": scope.suggestion
                        };

                        $http.post(scope.url, data)
                            .then(function onSuccess(result) {
                                notify.addSuccessMessage('Foreslag sendt!');
                                scope.suggestion = "";
                            }, function onError(result) {
                                notify.addErrorMessage('Kunne ikke sende foreslag!');
                            });
                    };
                }
            };
        }
    ]);
})(angular, app);

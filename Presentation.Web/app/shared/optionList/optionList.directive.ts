(function (ng, app) {
    'use strict';

    app.directive('optionList', [
        '$http', function ($http) {
            return {
                scope: {
                    optionsUrl: '@',
                    title: '@',
                },
                templateUrl: 'app/shared/optionList/optionList.view.html',
                link: function (scope, element, attrs) {

                    scope.list = [];

                    $http.get(scope.optionsUrl + '?nonsuggestions')
                        .then(function onSuccess(result) {
                            _.each(result.data.response, function (v) {
                                scope.list.push({
                                    id: v.id,
                                    name: v.name,
                                    note: v.note
                                });
                            });
                        });
                }
            };
        }
    ]);
})(angular, app);

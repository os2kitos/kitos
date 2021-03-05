(function (ng, app) {
    'use strict';

    app.directive('unique', [
        '$http', 'userService', function ($http, userService) {
            return {
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    var user;
                    userService.getUser().then(function (result) {
                        user = result;
                    });
                    var validateAsync = _.debounce(function (viewValue) {
                        $http.get(attrs.unique + '?checkname=' + viewValue + '&orgId=' + user.currentOrganizationId)
                            .then(function onSuccess(result) {
                                ctrl.$setValidity('available', true);
                                ctrl.$setValidity('lookup', true);
                            }, function onError(result) {
                                // conflict
                                if (result.status == 409) {
                                    ctrl.$setValidity('available', false);
                                } else {
                                    // something went wrong
                                    ctrl.$setValidity('lookup', false);
                                }
                            });
                    }, 500);

                    ctrl.$parsers.unshift(function (viewValue) {
                        validateAsync(viewValue);
                        // async returns breaks the setting of $modelValue so just returning
                        return viewValue;
                    });
                }
            };
        }
    ]
    );
})(angular, app);

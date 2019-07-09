(function (ng, app) {
    'use strict';

    app.directive('uniqueEmailContactperson', [
        '$http', 'userService', '_', function ($http: ng.IHttpService, userService, _: _.LoDashStatic) {
            return {
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    var user;
                    userService.getUser().then(function (result) {
                        user = result;
                    });
                    var validateAsync = _.debounce(function (viewValue) {
                        $http.get<Kitos.Models.IODataResult<boolean>>(`/odata/Users/Users.IsEmailAvailable(email='${viewValue}')`)
                            .then((response) => {
                                if (response.data.value) {
                                    userService.get
                                    // email is available
                                    scope._emailExists = false;
                                    ctrl.$setValidity('available', true);
                                    ctrl.$setValidity('lookup', true);
                                } else {
                                    // email is in use
                                    scope._emailExists = true;
                                }
                            }, () => {
                                // something went wrong
                                ctrl.$setValidity('lookup', false);
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

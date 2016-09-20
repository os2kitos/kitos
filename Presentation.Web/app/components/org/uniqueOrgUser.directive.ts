(function(ng, app) {
    'use strict';

    app.directive('uniqueOrgUser', [
        '$http', 'userService', '_', function ($http: ng.IHttpService, userService, _: _.LoDashStatic) {
                return {
                    require: 'ngModel',
                    link: function(scope, element, attrs, ctrl) {
                        var user;
                        userService.getUser().then(function(result) {
                            user = result;
                        });
                        var validateAsync = _.debounce(function (email) {
                            $http.get<Kitos.Models.IODataResult<Kitos.Models.IOrganizationRight[]>>(`/odata/Organizations(${user.currentOrganizationId})/Rights?$filter=User/Email eq '${email}'&$select=Role`)
                                .then((response) => {
                                    if (_.isEmpty(response.data.value)) {
                                        // user doesn't exist in organization
                                        ctrl.$setValidity('lookup', true);
                                        scope.userExists = false;
                                    } else {
                                        ctrl.$setValidity('lookup', true);
                                        scope.userExists = true;
                                    }
                                }, () => {
                                    // something went wrong
                                    ctrl.$setValidity('lookup', false);
                                });
                        }, 500);

                        ctrl.$parsers.unshift(function(email) {
                            validateAsync(email);
                            // async returns breaks the setting of $modelValue so just returning
                            return email;
                        });
                    }
                };
            }
        ]
    );
})(angular, app);

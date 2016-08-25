(function(ng, app) {
    'use strict';

    app.directive('uniqueOrgUser', [
            '$http', 'userService', function($http, userService) {
                return {
                    require: 'ngModel',
                    link: function(scope, element, attrs, ctrl) {
                        var user;
                        userService.getUser().then(function(result) {
                            user = result;
                        });
                        var validateAsync = _.debounce(function(email) {
                            $http.get(attrs.uniqueOrgUser + '?email=' + email + '&orgId=' + user.currentOrganizationId + '&userExistsWithRole')
                                .success(function() {
                                    ctrl.$setValidity('available', true);
                                    ctrl.$setValidity('lookup', true);
                                    scope.userExists = true;
                                })
                                .error(function(data, status) {
                                    //User dosn't exist in organization
                                    scope.userExists = false;
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

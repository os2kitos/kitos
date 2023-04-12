(function (ng, app) {
    'use strict';

    app.directive('uniqueOrgUser', [
        '$http', 'userService', '_', function ($http: ng.IHttpService, userService, _: _.LoDashStatic) {
            return {
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    var user;
                    userService.getUser().then(function (result) {
                        user = result;
                    });
                    const handleUserDoesNotExist = () => {
                        // user doesn't exist in organization
                        ctrl.$setValidity('lookup', true);
                        scope.userExists = false;
                    }
                    var validateAsync = _.debounce(function (email) {
                        $http.get<Kitos.Models.IUser>('odata/GetUserByEmailAndOrganizationRelationship(email=\'' + email + '\',organizationId=' + user.currentOrganizationId + ')')
                            .then((response) => {
                                const foundUser: Kitos.Models.IUser | undefined | null = response.data;
                                if (foundUser) {
                                    ctrl.$setValidity('lookup', true);
                                    scope.userExists = true;
                                } else {
                                    handleUserDoesNotExist();
                                }
                            }, (error) => {
                                if (error.status === 404) {
                                    handleUserDoesNotExist();
                                } else {
                                    // something went wrong
                                    ctrl.$setValidity('lookup', false);
                                }

                            });
                    }, 500);

                    ctrl.$parsers.unshift(function (email) {
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

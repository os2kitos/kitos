(function (ng, app) {
    'use strict';

    app.directive('uniqueItInterfaceIdName', [
        '$http', 'userService', '$q',
        function ($http, userService, $q) {
            return {
                require: 'ngModel',
                link: function (scope, element, attrs, ngModel) {
                    var user;
                    userService.getUser().then(function (result) {
                        user = result;

                        ngModel.$asyncValidators.uniqueConstraint = function (value) {
                            var deffered = $q.defer();
                            var name = scope.createForm.name.$viewValue;
                            var itInterfaceId = scope.createForm.itInterfaceId.$viewValue;

                            $http.get('/api/itinterface?checkitinterfaceid=' + itInterfaceId + '&checkname=' + name + '&orgId=' + user.currentOrganizationId)
                                .then(function onSuccess(result) {
                                    scope.uniqueConstraintError = false;
                                    deffered.resolve();
                                }, function onError(result) {
                                    scope.uniqueConstraintError = true;
                                    deffered.reject();
                                });
                            return deffered.promise;
                        }
                    });
                }
            }
        }
    ]);
})(angular, app);

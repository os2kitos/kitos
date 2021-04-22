(function (ng, app) {

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('edit-user', {
            url: '/user',
            templateUrl: 'app/components/home/home-edit-user.view.html',
            controller: 'home.EditUserCtrl',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getFreshUser();
                }],
                orgUnits: [
                    '$http', 'user', function ($http, user) {
                        return $http.get('api/organizationUnit?organization=' + user.currentOrganizationId).then(function (result) {
                            var options: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<number>[] = []

                            function visit(orgUnit: Kitos.Models.Api.Organization.OrganizationUnit, indentationLevel: number) {
                                var option = {
                                    id: String(orgUnit.id),
                                    text: orgUnit.name,
                                    indentationLevel: indentationLevel
                                };

                                options.push(option);

                                _.each(orgUnit.children, function (child) {
                                    return visit(child, indentationLevel + 1);
                                });

                            }
                            visit(result.data.response, 0);
                            return options;
                        });
                    }
                ],
            }
        });

    }]);

    app.controller('home.EditUserCtrl', ['$rootScope', '$scope', 'notify', 'userService', 'user', 'orgUnits',
        function ($rootScope, $scope, notify, userService: Kitos.Services.UserService, user: Kitos.Models.Api.IUser, orgUnits: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<number>[]) {
            $rootScope.page.title = 'Profil indstillinger';
            $rootScope.page.subnav = [];

            $scope.orgUnits = orgUnits;
            $scope.allowClear = false;

            var selectedOrgUnit = () => _.find(orgUnits, (orgUnit) => orgUnit.id === String(user.defaultOrganizationUnitId));

            function init(user, orgUnit) {
                $scope.user = {
                    name: user.name,
                    lastName: user.lastName,
                    email: user.email,
                    phoneNumber: user.phoneNumber,
                    defaultUserStartPreference: user.defaultUserStartPreference,
                    defaultOrganizationUnit: orgUnit,
                    isUsingDefaultOrgUnit: user.isUsingDefaultOrgUnit,
                    currentOrganizationName: user.currentOrganizationName,
                    currentOrganizationUnitName: user.currentOrganizationUnitName
                };

            }
            init(user, selectedOrgUnit());

            //can't use autosave - need to patch through userService!
            $scope.patch = function (field, value) {
                var payload = {};
                payload[field] = value;

                userService.patchUser(payload).then(function (newUser: Kitos.Models.Api.IUser) {
                    var orgUnit = _.find(orgUnits, (orgUnit) => orgUnit.id === String(newUser.defaultOrganizationUnitId));
                    init(newUser, orgUnit);
                    notify.addSuccessMessage("Feltet er opdateret!");
                }, function() {
                    notify.addErrorMessage("Fejl! Kunne ikke opdatere feltet!");
                });
            };
            $scope.updateDefaultOrgUnit = function () {
                if ($scope.user.defaultOrganizationUnit === selectedOrgUnit()) {
                    return;
                }

                if ($scope.user.defaultOrganizationUnit === null) {
                    return;
                }

                if ($scope.user.defaultOrganizationUnit.id === undefined) {
                    return;
                }

                userService.updateDefaultOrgUnit($scope.user.defaultOrganizationUnit.id)
                    .then(function onSuccess(newUser) {
                        userService.getFreshUser()
                            .then(
                                data => {
                                    var newOrgUnit = $scope.user.defaultOrganizationUnit;
                                    init(data, newOrgUnit);
                                }
                            );
                        notify.addSuccessMessage("Feltet er opdateret!");
                    }, function onError() {
                        notify.addErrorMessage("Fejl! Kunne ikke opdatere feltet!");
                    });
            };
        }]);

})(angular, app);

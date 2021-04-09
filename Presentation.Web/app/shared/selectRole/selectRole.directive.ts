((ng, app) => {
    'use strict';

    app.controller("selectRoleController", ["$scope", "entityMapper", "select2LoadingService", ($scope, entityMapper, select2LoadingService) => {
        var roles = entityMapper.mapRoleToSelect2ViewModel($scope.roles);

        if ($scope.preSelectedRole != null) {
            var foundSelectedInOptions = _.find(roles, (role: any) => role.id == $scope.preSelectedRole.Id);
            if (roles && $scope.preSelectedRole.Id != null && !foundSelectedInOptions) {
                roles.splice(0, 0, { id: $scope.preSelectedRole.Id, text: $scope.preSelectedRole.Name + " (Udgået)", optionalObjectContext: { hasWriteAccess: $scope.preSelectedRole.HasWriteAccess } });
            }
        }

        roles = _.map(roles, addAccessRights);

        $scope.select2Config = select2LoadingService.select2LocalDataNoSearch(() => roles, true);

        if ($scope.selectedRole == null) {
            $scope.selectedRole = roles[0];
        }
        else {
            $scope.selectedRole = _.find(roles, (role: any) => role.id == $scope.selectedRole);
        }

        function addAccessRights(role) {
            var accessRight = role.optionalObjectContext.hasWriteAccess ? " (skriv)" : " (læs)";
            return { id: role.id, text: role.text + accessRight}
        }

    }]);

    app.directive("selectRole", [
        () => ({
            templateUrl: "app/shared/selectRole/selectRole.view.html",
            scope: {
                id: "@",
                placeholder: "@",
                roles: "=",
                selectedRole: "=ngModel",
                preSelectedRole: "="
            },
            controller: "selectRoleController",
            controllerAs: "SelectRoleVm",
            link(scope, element, attr, ctrl) {

                scope.$watch(attr.disabled, newVal => {
                    element.prop('disabled', newVal);
                });
            }
        })
    ]);
})(angular, app);
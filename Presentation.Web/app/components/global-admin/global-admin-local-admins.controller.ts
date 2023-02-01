((ng, app) => {
    app.config([
        "$stateProvider", $stateProvider => {
            $stateProvider.state("global-admin.local-users", {
                url: "/local-admins",
                templateUrl: "app/components/global-admin/global-admin-local-admins.view.html",
                controller: "globalAdmin.localAdminsCtrl",
                authRoles: ["GlobalAdmin"],
                resolve: {
                    user: [
                        "userService", userService => userService.getUser()
                    ]
                }
            });
        }
    ]);

    app.controller("globalAdmin.localAdminsCtrl", [
        "$rootScope", "$scope", "user",
        ($rootScope, $scope, user: Kitos.Services.IUser) => {
            $rootScope.page.title = "Lokal administratorer";
            $scope.userId = user.id;
        }
    ]);
})(angular, app);

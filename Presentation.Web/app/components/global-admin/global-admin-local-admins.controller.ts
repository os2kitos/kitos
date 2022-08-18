((ng, app) => {
    app.config([
        "$stateProvider", $stateProvider => {
            $stateProvider.state("global-admin.local-users", {
                url: "/local-admins",
                templateUrl: "app/components/global-admin/global-admin-local-admins.view.html",
                controller: "globalAdmin.localAdminsCtrl",
                authRoles: ["GlobalAdmin"],
                resolve: {
                    adminRights: [
                        "$http", $http => $http.get("api/OrganizationRight/?roleName=LocalAdmin&roleWithName").then(result => result.data.response)
                    ],
                    user: [
                        "userService", userService => userService.getUser()
                    ]
                }
            });
        }
    ]);

    app.controller("globalAdmin.localAdminsCtrl", [
        "$rootScope", "$scope", "$http", "$state", "notify", "adminRights", "user", "userService","select2LoadingService",
        ($rootScope, $scope, $http, $state, notify, adminRights, user: Kitos.Services.IUser, userService: Kitos.Services.IUserService, select2LoadingService: Kitos.Services.ISelect2LoadingService) => {
            $rootScope.page.title = "Lokal administratorer";
            $scope.adminRights = adminRights;

            function newLocalAdmin() {
                // select2 changes the value twice, first with invalid values
                // so ignore invalid values
                if (typeof $scope.newUser !== "object") return;
                if (!($scope.newOrg && $scope.newUser)) return;

                var user = $scope.newUser;
                var uId = user.id;
                const oId = $scope.newOrg.id;
                var orgName = $scope.newOrg.text;

                const rId = Kitos.API.Models.OrganizationRole.LocalAdmin;

                if (!(uId && oId && rId)) return;

                const data = {
                    userId: uId,
                    role: rId,
                    organizationId: oId
                };
                var msg = notify.addInfoMessage("Arbejder ...", false);
                $http.post("api/OrganizationRight/" + oId, data, { handleBusy: true })
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage(user.text + " er blevet lokal administrator for " + orgName);
                        if (uId == user.id) {
                            // Reload user
                            userService.reAuthorize();
                        }
                        reload();
                    }, function onError(result) {
                        msg.toErrorMessage("Kunne ikke gøre " + user.text + " til lokal administrator for " + orgName);
                    });
            }

            function reload() {
                $state.go(".", null, { reload: true });
            }

            $scope.$watch("newUser", (newVal, oldVal) => {
                if (newVal === oldVal || !newVal) return;

                newLocalAdmin();
            });

            $scope.$watch("newOrg", (newVal, oldVal) => {
                if (newVal === oldVal || !newVal) return;

                newLocalAdmin();
            });

            $scope.deleteLocalAdmin = right => {
                var oId = right.organizationId;
                var rId = right.role;
                var uId = right.userId;
                var msg = notify.addInfoMessage("Arbejder ...", false);
                $http.delete("api/OrganizationRight/" + oId + "?rId=" + rId + "&uId=" + uId + "&organizationId=" + user.currentOrganizationId)
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage(right.userName + " er ikke længere lokal administrator");
                        if (uId == user.id) {
                            // Reload user
                            userService.reAuthorize();
                        }
                        reload();
                    }, function onError(result) {

                        msg.toErrorMessage("Kunne ikke fjerne " + right.userName + " som lokal administrator");
                    });
            };

            $scope.organizationSelectOptions = select2LoadingService.loadSelect2WithDataHandler("api/organization", true, ["take=100", "orgId=" + user.currentOrganizationId], (item, items) => {
                items.push({
                    id: item.id,
                    text: item.name ? item.name : 'Unavngiven',
                    cvr: item.cvrNumber
                });
            }, "q", Kitos.Helpers.Select2OptionsFormatHelper.formatOrganizationWithCvr);
        }
    ]);
})(angular, app);

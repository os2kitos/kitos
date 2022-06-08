((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("global-admin.misc", {
            url: "/misc",
            templateUrl: "app/components/global-admin/global-admin-misc.view.html",
            controller: "globalAdminMisc",
            authRoles: ["GlobalAdmin"],
            resolve: {
                brokenLinkStatus: [
                    "brokenLinksReportService", (brokenLinksReportService: Kitos.Services.BrokenLinksReport.IBrokenLinksReportService) => brokenLinksReportService.getStatus()
                ],
                usersWithRightsholderAccess: [
                    "userService", (userService: Kitos.Services.IUserService) => userService.getUsersWithRightsholderAccess()
                ],
                usersWithCrossAccess: [
                    "userService", (userService: Kitos.Services.IUserService) => userService.getUsersWithCrossAccess()
                ]
            }
        });
    }]);
    
    app.controller("globalAdminMisc", [
        "$rootScope",
        "$scope",
        "$http",
        "_",
        "notify",
        "kleService",
        "$uibModal",
        "brokenLinkStatus",
        "usersWithRightsholderAccess",
        "usersWithCrossAccess",
        "userService",
        "select2LoadingService",
        "$state",
        (
            $rootScope,
            $scope,
            $http,
            _,
            notify,
            kleService,
            $uibModal: ng.ui.bootstrap.IModalService,
            brokenLinkStatus: Kitos.Models.Api.BrokenLinksReport.IBrokenLinksReportDTO,
            usersWithRightsholderAccess: Kitos.Models.Api.IUserWithOrganizationName[],
            usersWithCrossAccess: Kitos.Models.Api.IUserWithCrossAccess[],
            userService: Kitos.Services.IUserService,
            select2LoadingService: Kitos.Services.ISelect2LoadingService,
            $state) => {
            
            $rootScope.page.title = "Andet";
            $scope.brokenLinksVm = Kitos.Models.ViewModel.GlobalAdmin.BrokenLinks.BrokenLinksViewModelMapper.mapFromApiResponse(brokenLinkStatus);
            $scope.usersWithRightsholderAccess = usersWithRightsholderAccess;
            $scope.usersWithCrossAccess = usersWithCrossAccess;
            $scope.kitosUsers = [];
            $scope.userOrganizations = [];

            $scope.showOrgsWhereUserActive = (activeOrgNames: string[]) => {
                $uibModal.open({
                    templateUrl: "app/components/global-admin/global-admin-organizations-where-user-rights.modal.view.html",
                    windowClass: "modal fade in",
                    resolve: {
                        orgNames: [() => activeOrgNames],
                    },
                    controller: ["$scope", "orgNames", ($scope, orgNames: string[]) => {
                        $scope.orgNames = orgNames;
                    }]
                }).result.then(function onSuccess() {
                }, function onError(error) {
                    // Swallow unhandled rejection errors.
                });
            };

            $scope.userOptions = getAvailableUserOptions();

            getKleStatus();
            function getKleStatus() {
                $scope.KLEUpdateAvailableLabel = "Undersøger om der er en ny version af KLE...";
                toggleKleButtonsClickAbility(false, false);
                kleService.getStatus()
                    .then(function onSuccess(result) {
                        if (result.status !== 200) {
                            notify.addErrorMessage("Der skete en fejl ifm. tjek af ny KLE version");
                            return;
                        }
                        if (!result.data.response.upToDate) {
                            $scope.KLEUpdateAvailableLabel = "Der er en ny version af KLE, udgivet " + result.data.response.version;
                            toggleKleButtonsClickAbility(true, false);
                        }
                        else {
                            $scope.KLEUpdateAvailableLabel = "KITOS baserer sig på den seneste KLE version, udgivet  " + result.data.response.version;
                            toggleKleButtonsClickAbility(false, false);
                        }
                    }, function onSuccess(result) {
                        toggleKleButtonsClickAbility(false, false);
                        notify.addErrorMessage("Der skete en fejl ifm. tjek af ny KLE version");
                    });
            }

            $scope.GetKLEChanges = () => {
                toggleKleButtonsClickAbility(false, false);
                kleService.getChanges()
                    .then(function onSuccess(result) {
                        if (result.status !== 200) {
                            toggleKleButtonsClickAbility(true, false);
                            notify.addErrorMessage("Der skete en fejl under hentning af ændringer");
                            return;
                        }
                        var universalBOM = "\uFEFF";
                        var anchor = angular.element(document.getElementById("KLEDownloadAnchor"));
                        anchor.attr("data-element-type", "KLEDownloadAnchor");
                        anchor.attr({
                            href: 'data:text/csv; charset=utf-8,' + encodeURI(universalBOM + result.data),
                            target: "_blank",
                            download: "KLE-Changes.csv"
                        })[0].click();
                        notify.addSuccessMessage("Download af ændringer færdig");
                        toggleKleButtonsClickAbility(true, true);
                    }, function onError(result) {
                        toggleKleButtonsClickAbility(true, false);
                        notify.addErrorMessage("Der skete en fejl under henting af ændringer");
                    });
            };

            $scope.UpdateKLE = () => {
                toggleKleButtonsClickAbility(false, false);
                if (confirm("Er du sikker på, at du vil opdatere KLE til nyeste version?")) {
                    kleService.applyUpdateKLE().
                        then((response) => {
                            if (response.status !== 200) {
                                toggleKleButtonsClickAbility(true, false);
                                angular.element(document.getElementById("overlay")).remove();
                                notify.addErrorMessage("Der skete en fejl under opdatering af KLE");
                                return;
                            }
                            notify.addSuccessMessage("KLE er opdateret");
                            angular.element(document.getElementById("overlay")).remove();
                            getKleStatus();
                        }).
                        catch(() => {
                            toggleKleButtonsClickAbility(true, false);
                            notify.addErrorMessage("Der skete en fejl under opdatering af KLE");
                            angular.element(document.getElementById("overlay")).remove();
                        });
                } else {
                    notify.addInfoMessage("KLE opdatering stoppet!");
                }
            };

            $scope.$watch("selectedUser", function (newVal, oldVal) {
                if (newVal === oldVal || !newVal) return;

                if (typeof newVal === "object")
                    return;
                $scope.userOrganizations = [];
                userService.getUserOrganizations(newVal).then(res => {
                    $scope.userOrganizations.pushArray(res);
                });
            });

            $scope.removeUser = (id: number) => {
                const nameAndEmail = `${$scope.selectedUser.text}, ${$scope.selectedUser.email}`;

                if (confirm(`Er du sikker på, at du vil slette ${nameAndEmail}`)) {
                    notify.addInfoMessage(`Sletter ${nameAndEmail}`);
                    userService.deleteUser(id)
                        .then(() => {
                                notify.addSuccessMessage(`Sletter ${nameAndEmail}`);
                                $scope.selectedUser = null;
                            }
                        ).catch(ex => {
                            console.log(ex);
                            notify.addErrorMessage(`Fejl ifm. sletning af brugeren Sletter ${nameAndEmail}`);
                        });
                    $state.reload();
                }
            };

            function toggleKleButtonsClickAbility(updateAvailButton: boolean, updateButton: boolean) {
                $scope.KleUpdateAvailableButtonInteraction = updateAvailButton;
                $scope.KleApplyUpdateButtonInteraction = updateButton;
            }

            function getAvailableUserOptions() {
                return select2LoadingService.loadSelect2WithDataSource(
                    (query: string) =>
                        userService.searchUsers(query)
                        .then(
                            results =>
                                results.map(result =>
                                    <Kitos.Models.ViewModel.Generic.Select2OptionViewModel<Kitos.Models.Users.IUserWithEmailDTO>>
                                {
                                    id: result.id,
                                    text: result.name,
                                    email: result.email,
                                    optionalObjectContext: result
                                })
                        )
                    , false
                    , Kitos.Helpers.Select2OptionsFormatHelper.formatUserWithEmail);
            }
        }]);
})(angular, app);
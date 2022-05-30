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
            select2LoadingService: Kitos.Services.ISelect2LoadingService) => {
            
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

            $scope.userOptions = getAvailableUsers();

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

                $scope.userOrganizations = [];
                userService.getUserOrganizations(newVal).then(res => {
                    $scope.userOrganizations.pushArray(res);
                    $scope.userSelected = true;
                });
            });

            $scope.removeUser = (id: number) => {
                userService.deleteUser(id)
                    .then(() => {
                            $scope.userSelected = false;
                            $scope.selectedUser = null;
                        }
                    ).catch(ex => console.log(ex));
            };

            function toggleKleButtonsClickAbility(updateAvailButton: boolean, updateButton: boolean) {
                $scope.KleUpdateAvailableButtonInteraction = updateAvailButton;
                $scope.KleApplyUpdateButtonInteraction = updateButton;
            }
            
            function formatUser(user) {
                var result = '<div>' + user.text + '</div>';
                if (user.email) {
                    result += '<div class="small">' + user.email + '</div>';
                }
                return result;
            }

            function getAvailableUsers() {
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
                    , formatUser);
            }

            function selectLazyLoading(url, format, paramAry) {
                return {
                    minimumInputLength: 1,
                    allowClear: true,
                    placeholder: ' ',
                    formatResult: format,
                    ajax: {
                        data: function (term, page) {
                            return { query: term };
                        },
                        quietMillis: 500,
                        transport: function (queryParams) {
                            var res = $http.get(url + queryParams.data.query).then(queryParams.success);
                            res.abort = function () {
                                return null;
                            };

                            return res;
                        },

                        results: function (data, page) {
                            var results = [];

                            _.each(data.data.response, function (obj: { id: any; name: any; email: any }) {
                                results.push({
                                    id: obj.id,
                                    text: obj.name ? obj.name : 'Unavngiven',
                                    email: obj.email
                                });
                            });

                            return { results: results };
                        }
                    }
                };
            }
        }]);
})(angular, app);
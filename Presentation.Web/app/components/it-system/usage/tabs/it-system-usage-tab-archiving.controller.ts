((ng, app) => {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.usage.archiving", {
            url: "/archiving",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-archiving.view.html",
            controller: "system.EditArchiving",
            resolve: {
                itSystemUsage: [
                    "$http", "$stateParams", ($http, $stateParams) => $http.get("api/itSystemUsage/" + $stateParams.id)
                    .then(result => result.data.response)
                ],
                archiveTypes: [
                    "localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                        localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ArchiveTypes).getAll()
                ],
                archiveLocations: [
                    "localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                        localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ArchiveLocations).getAll()
                ],
                archiveTestLocations: [
                    "localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                        localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ArchiveTestLocations).getAll()
                ],
                systemUsage: [
                    "$http", "$stateParams", ($http, $stateParams) =>
                        $http.get(`odata/itSystemUsages(${$stateParams.id})`)
                            .then(result => result.data)
                ],
                user: ["userService", (userService: Kitos.Services.IUserService) => userService.getUser()
                ],
                archivePeriod: ["$http", "$stateParams", "user", ($http, $stateParams, user: Kitos.Services.IUser) =>
                    $http.get(`odata/Organizations(${user.currentOrganizationId})/ItSystemUsages(${$stateParams.id})/ArchivePeriods?$orderby=StartDate`)
                        .then(result => result.data.value)]
            }
        });
    }]);

    app.controller("system.EditArchiving", ["$scope", "_", "$http", "$state", "$stateParams", "$timeout", "user", "itSystemUsage", "itSystemUsageService", "archiveTypes", "archiveLocations", "archiveTestLocations", "systemUsage", "archivePeriod", "moment", "notify", "select2LoadingService",
        ($scope, _, $http, $state, $stateParams, $timeout, user, itSystemUsage, itSystemUsageService, archiveTypes, archiveLocations, archiveTestLocations, systemUsage, archivePeriod, moment, notify, select2LoadingService: Kitos.Services.ISelect2LoadingService) => {
            $scope.usage = itSystemUsage;
            $scope.archiveTypes = archiveTypes;
            $scope.archiveLocations = archiveLocations;
            $scope.archiveTestLocations = archiveTestLocations;
            $scope.usageId = $stateParams.id;
            $scope.systemUsage = systemUsage;
            $scope.archivePeriods = archivePeriod;
            $scope.hasWriteAccessAndArchived = systemUsage.Archived;
            $scope.ArchiveDuty = $scope.usage.archiveDuty;
            $scope.archiveReadMoreLink = Kitos.Constants.Archiving.ReadMoreUri;
            $scope.translateArchiveDutyRecommendation = (value: number) => Kitos.Models.ItSystem.ArchiveDutyRecommendationFactory.mapFromNumeric(value).text;
            $scope.archiveDutyOptions = Kitos.Models.ItSystemUsage.ArchiveDutyOptions.getAll();

            $scope.autoSaveUrl = 'api/itSystemUsage/' + $stateParams.id;

            if (!systemUsage.Archived) {
                $scope.systemUsage.Archived = false;
            }

            if (!$scope.ArchiveDuty) {
                $scope.ArchiveDuty = $scope.usage.itSystem.archiveDuty;
            }

            if ($scope.archivePeriods) {
                sortDate();
            }
            $scope.$watch("usage.archiveTypeId", (newValue, oldValue) => {
                if (newValue !== oldValue && newValue !== null) {
                    $scope.dirty = true;
                }
            });
            $scope.$watch("usage.archiveLocationId", (newValue, oldValue) => {
                if (newValue !== oldValue && newValue !== null) {
                    $scope.dirty = true;
                }
            });
            $scope.$watch("usage.archiveTestLocationId", (newValue, oldValue) => {
                if (newValue !== oldValue && newValue !== null) {
                    $scope.dirty = true;
                }
            });
            function sortDate() {
                let dateList = [];
                let dateNotList = [];
                _.each($scope.archivePeriods, x => {
                    var formatDateString = Kitos.Constants.DateFormat.EnglishDateFormat;
                    if (moment().isBetween(moment(x.StartDate, [Kitos.Constants.DateFormat.DanishDateFormat, formatDateString]).startOf('day'), moment(x.EndDate, [Kitos.Constants.DateFormat.DanishDateFormat, formatDateString]).endOf('day'), null, '[]')) {
                        dateList.push(x);
                    } else {
                        dateNotList.push(x);
                    }
                });
                _.each(dateNotList, x => {
                    dateList.push(x);
                });
                $scope.archivePeriods = dateList;
            }

            $scope.patch = (field, value) => {
                var payload = {};
                payload[field] = value;
                if (field === "Archived") {
                    $http.patch(`/odata/ItSystemUsages(${$scope.usageId})`, payload)
                        .then(() => {
                            notify.addSuccessMessage("Feltet er opdateret!");
                            reload();
                        },
                            () => notify.addErrorMessage("Fejl! Feltet kunne ikke opdateres!"));
                }
                else if (field === "ArchiveFreq" && (value < 0 || value.length === 0)) {
                    notify.addErrorMessage("Fejl! Feltet kunne ikke opdateres!");
                }
                else {
                    itSystemUsageService.patchSystem($scope.usageId, payload);
                }
            }

            $scope.save = () => {
                $scope.$broadcast("show-errors-check-validity");
                
                var startDate = $scope.archivePeriod.startDate;
                var endDate = $scope.archivePeriod.endDate;

                if (Kitos.Helpers.DateValidationHelper.validateValidityPeriod(startDate, endDate, notify, "Startdato", "Slutdato")) {
                    startDate = Kitos.Helpers.DateStringFormat.fromDanishToEnglishFormat(startDate);
                    endDate= Kitos.Helpers.DateStringFormat.fromDanishToEnglishFormat(endDate);

                    var payload = {};
                    payload["StartDate"] = startDate;
                    payload["EndDate"] = endDate;
                    payload["UniqueArchiveId"] = $scope.archivePeriod.uniqueArchiveId;
                    payload["ItSystemUsageId"] = $stateParams.id;
                    payload["Approved"] = $scope.archivePeriod.approved;

                    $http.post(`odata/ArchivePeriods?organizationId=${user.currentOrganizationId}`, payload).finally(reload);
                }
            };

            function reload() {
                return $state.transitionTo($state.current, $stateParams, {
                    reload: true
                }).then(() => {
                    $scope.hideContent = true;
                    return $timeout(() => $scope.hideContent = false, 1);
                });
            };

            $scope.delete = (id) => {
                $http.delete(`odata/ArchivePeriods(${id})`).finally(reload);
                notify.addSuccessMessage("Slettet!");
            };

            if (!!$scope.usage.archiveSupplierId) {
                $scope.archiveSupplier = {
                    id: $scope.usage.archiveSupplierId,
                    text: $scope.usage.archiveSupplierName
                };
            }

            $scope.suppliersSelectOptions = select2LoadingService.loadSelect2WithDataHandler("api/organization", true, ['take=25', 'orgId=' + user.currentOrganizationId], (item,
                items) => {
                items.push({
                    id: item.id,
                    text: item.name ? item.name : 'Unavngiven',
                    cvr: item.cvrNumber
                });
            }, "q", Kitos.Helpers.Select2OptionsFormatHelper.formatOrganizationWithCvr);

            $scope.patchDatePeriode = (field, value, id) => {
                var dateObject = $scope.archivePeriods.filter(x => x.Id === id);
                if (dateObject.length === 0) {
                    console.log(`Archive period with id: ${id} wasn't found`);
                    notify.addSuccessMessage("Feltet er opdateret!");
                    return;
                }

                var dateStart = dateObject[0].StartDate;
                var dateEnd = dateObject[0].EndDate;

                if (Kitos.Helpers.DateValidationHelper.validateValidityPeriod(dateStart, dateEnd, notify, "Startdato", "Slutdato")) {
                    const dateString = Kitos.Helpers.DateStringFormat.fromDanishToEnglishFormat(value);
                    var payload = {};
                    payload[field] = dateString;
                    sortDate();
                    $http.patch(`odata/ArchivePeriods(${id})`, payload).finally(reload);
                    notify.addSuccessMessage("Datoen er opdateret!");
                }
            }

            $scope.patchPeriode = (field, value, id) => {
                var payload = {};
                payload[field] = value;
                $http.patch(`odata/ArchivePeriods(${id})`, payload);
                notify.addSuccessMessage("Feltet er opdateret!");
            }

            $scope.datepickerOptions = Kitos.Configs.standardKendoDatePickerOptions;
        }]);

})(angular, app);

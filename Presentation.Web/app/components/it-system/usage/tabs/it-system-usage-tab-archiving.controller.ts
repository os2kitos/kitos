((ng, app) => {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.usage.archiving", {
            url: "/archiving",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-archiving.view.html",
            controller: "system.EditArchiving",
            resolve: {
                archiveTypes: [
                    "$http", $http =>
                    $http.get("odata/LocalArchiveTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                            .then(result => result.data.value)
                ],
                archiveLocations: [
                    "$http", $http =>
                    $http.get("odata/LocalArchivelocations?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                            .then(result => result.data.value)
                ],
                systemUsage: [
                    "$http", "$stateParams", ($http, $stateParams) =>
                        $http.get(`odata/itSystemUsages(${$stateParams.id})`)
                            .then(result => result.data)
                ],
                archivePeriod: ["$http", "$stateParams", ($http, $stateParams) =>
                    $http.get(`odata/ArchivePeriods?$filter=ItSystemUsageId eq ${$stateParams.id}&$orderby=StartDate`)
                            .then(result => result.data.value)]
            }
        });
    }]);

    app.controller("system.EditArchiving", ["$scope", "_", "$http", "$state", "$stateParams", "$timeout", "user", "itSystemUsage", "itSystemUsageService", "archiveTypes", "archiveLocations", "systemUsage", "archivePeriod", "moment", "notify",
        ($scope, _,$http, $state, $stateParams, $timeout, user, itSystemUsage, itSystemUsageService, archiveTypes, archiveLocations, systemUsage, archivePeriod, moment, notify) => {
            $scope.usage = itSystemUsage;
            $scope.archiveTypes = archiveTypes;
            $scope.archiveLocations = archiveLocations;
            $scope.usageId = $stateParams.id;
            $scope.systemUsage = systemUsage;
            $scope.ArchivedDate = systemUsage.ArchivedDate;
            $scope.archivePeriods = archivePeriod;
            $scope.hasWriteAccessAndArchived = systemUsage.Archived;
            $scope.ArchiveDuty = systemUsage.ArchiveDuty;

            if (!systemUsage.Archived) {
                $scope.systemUsage.Archived = false;
            }

            if (!systemUsage.ArchiveDuty) {
                $scope.ArchiveDuty = itSystemUsage.itSystem.archiveDuty;
            }

            if ($scope.archivePeriods) {
                sortDate();
            }
            function sortDate() {
                let dateList = [];
                let dateNotList = [];
                _.each($scope.archivePeriods, x => {
                    if (moment().isBetween(moment(x.StartDate), moment(x.EndDate), 'days', '[]')) {
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
                itSystemUsageService.patchSystem($scope.usageId, payload);
            }
            $scope.patchSupplier = (field, value) => {
                var payload = {};
                payload[field] = value.id;
                payload["ArchiveSupplier"] = value.text;
                itSystemUsageService.patchSystem($scope.usageId, payload);
            }
            
            if (systemUsage.SupplierId) {
                $scope.systemUsage.supplier = {
                    id: systemUsage.SupplierId,
                    text: systemUsage.ArchiveSupplier
                };
            }
            
            $scope.save = () => {
                $scope.$broadcast("show-errors-check-validity");
                //if ($scope.archiveForm.$invalid) { return; }

                var startDate = moment($scope.archivePeriod.startDate);
                if (!startDate.isValid() || isNaN(startDate.valueOf()) || startDate.year() < 1000 || startDate.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig."); { return; };
                } else {
                    startDate = startDate.format("YYYY-MM-DD");
                }
                var endDate = moment($scope.archivePeriod.endDate);
                if (!endDate.isValid() || isNaN(endDate.valueOf()) || endDate.year() < 1000 || endDate.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig."); { return; };
                } else {
                    endDate = endDate.format("YYYY-MM-DD");
                }
                var payload = {};
                payload["StartDate"] = startDate;
                payload["EndDate"] = endDate;
                payload["UniqueArchiveId"] = $scope.archivePeriod.uniqueArchiveId;
                payload["ItSystemUsageId"] = $stateParams.id;
                $http.post(`odata/ArchivePeriods`, payload).finally(reload);
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

            $scope.suppliersSelectOptions = selectLazyLoading('api/organization', false, formatSupplier, ['public=true', 'orgId=' + user.currentOrganizationId]);
            function formatSupplier(supplier) {
                var result = '<div>' + supplier.text + '</div>';
                if (supplier.cvr) {
                    result += '<div class="small">' + supplier.cvr + '</div>';
                }
                return result;
            }

            function selectLazyLoading(url, excludeSelf, format, paramAry) {
                return {
                    minimumInputLength: 1,
                    allowClear: true,
                    placeholder: ' ',
                    formatResult: format,
                    initSelection: function (elem, callback) {
                    },
                    ajax: {
                        data: function (term, page) {
                            return { query: term };
                        },
                        quietMillis: 500,
                        transport: function (queryParams) {
                            var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                            var res = $http.get(url + '?q=' + queryParams.data.query + extraParams).then(queryParams.success);
                            res.abort = function () {
                                return null;
                            };

                            return res;
                        },

                        results: function (data, page) {
                            var results = [];

                            _.each(data.data.response, function (obj: { id; name; cvr; }) {
                                results.push({
                                    id: obj.id,
                                    text: obj.name ? obj.name : 'Unavngiven',
                                    cvr: obj.cvr
                                });
                            });
                            return { results: results };
                        }
                    }
                };
            }

            $scope.patchDate = (field, value) => {
                var date = moment(value);

                if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");
                    $scope.ArchivedDate = systemUsage.ArchivedDate;
                } else {
                    date = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = date;
                    itSystemUsageService.patchSystem($scope.usageId, payload);
                    $scope.ArchivedDate = date;
                }
            };
            $scope.patchDatePeriode = (field, value, id) => {
                var date = moment(value);

                if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");
                } else {
                    date = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = date;
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

            $scope.dirty = $scope.systemUsage.ArchiveDuty != null ||
                $scope.systemUsage.ReportedToDPA != null ||
                $scope.systemUsage.ReportedToDPA != null ||
                $scope.systemUsage.DocketNo != null ||
                $scope.usage.archiveTypeId != null ||
                $scope.usage.archiveLocationId != null ||
                $scope.ArchivedDate != null ||
                $scope.systemUsage.ArchiveNotes != null;

            $scope.datepickerOptions = {
                format: "yyyy-MM-dd"
            };
        }]);

})(angular, app);

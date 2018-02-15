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
                    $http.get("api/ArchivePeriod/" + $stateParams.id + "?system=true")
                            .then(result => result.data.response)]
            }
        });
    }]);

    app.controller("system.EditArchiving", ["$scope", "$http", "$state", "$stateParams", "$timeout", "user", "itSystemUsage", "itSystemUsageService", "archiveTypes", "archiveLocations", "systemUsage", "archivePeriod", "moment", "notify",
        ($scope, $http, $state, $stateParams, $timeout, user, itSystemUsage, itSystemUsageService, archiveTypes, archiveLocations, systemUsage, archivePeriod, moment, notify) => {
            $scope.usage = itSystemUsage;
            $scope.archiveTypes = archiveTypes;
            $scope.archiveLocations = archiveLocations;
            $scope.usageId = $stateParams.id;
            $scope.systemUsage = systemUsage;
            $scope.ArchivedDate = systemUsage.ArchivedDate;
            $scope.archivePeriod = archivePeriod;
            $scope.hasWriteAccessAndArchived = systemUsage.Archived;
            console.log(itSystemUsage);
            if (itSystemUsage.itSystem.ArchiveDuty !== 0) {
                $scope.ArchiveDuty = itSystemUsage.itSystem.ArchiveDuty;
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
                console.log($scope);
                //if ($scope.archiveForm.$invalid) { return; }

                var startDate = moment($scope.archivePeriod.startDate, "DD-MM-YYYY");
                if (startDate.isValid()) {
                    $scope.archivePeriod.startDate = startDate.format("YYYY-MM-DD");
                } else {
                    $scope.archivePeriod.startDate = null;
                }
                var endDate = moment($scope.archivePeriod.endDate, "DD-MM-YYYY");
                if (endDate.isValid()) {
                    $scope.archivePeriod.endDate = endDate.format("YYYY-MM-DD");
                } else {
                    $scope.archivePeriod.endDate = null;
                }
                var payload = {};
                payload["StartDate"] = $scope.archivePeriod.startDate;
                payload["EndDate"] = $scope.archivePeriod.endDate;
                payload["UniqueArchiveId"] = $scope.archivePeriod.uniqueArchiveId;
                payload["ItSystemUsageId"] = $stateParams.id;
                $http.post("odate/ArchivePeriods", payload).finally(reload);
            };

            function reload() {
                return $state.transitionTo($state.current, $stateParams, {
                    reload: true
                }).then(() => {
                    $scope.hideContent = true;
                    return $timeout(() => $scope.hideContent = false, 1);
                });
            };

            $scope.delete = id => {
                $http.delete("api/ArchivePeriod/" + id + "?organizationId=" + user.currentOrganizationId).finally(reload);
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

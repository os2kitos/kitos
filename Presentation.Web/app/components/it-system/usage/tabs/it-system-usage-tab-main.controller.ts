((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.main", {
            url: "/main",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-main.view.html",
            controller: "system.EditMain",
            resolve: {
                businessTypes: [
                    "$http", $http => $http.get("odata/LocalBusinessTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                    .then(result => result.data.value)
                ],
                archiveTypes: [
                    "$http", $http => $http.get("odata/LocalArchiveTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                    .then(result => result.data.value)
                ],
                sensitiveDataTypes: [
                    "$http", $http => $http.get("odata/LocalSensitiveDataTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                    .then(result => result.data.value)
                ],
                systemCategories: [
                    "$http", $http => $http.get("odata/LocalItSystemCategories?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                    .then(result => result.data.value)
                ]
            }
        });
    }]);

    app.controller("system.EditMain", ["$rootScope", "$scope", "$http", "notify", "user", "systemCategories",
        "businessTypes", "archiveTypes", "sensitiveDataTypes", "autofocus",
        ($rootScope, $scope, $http, notify, user, systemCategories, businessTypes, archiveTypes, sensitiveDataTypes, autofocus) => {
            $rootScope.page.title = "IT System - Anvendelse";
            $scope.autoSaveUrl = `api/itSystemUsage/${$scope.usage.id}`;
            $scope.usageId = $scope.usage.id;
            $scope.businessTypes = businessTypes;
            $scope.archiveTypes = archiveTypes;
            $scope.sensitiveDataTypes = sensitiveDataTypes;
            $scope.hasViewAccess = user.currentOrganizationId == $scope.usage.organizationId;
            $scope.systemCategories = systemCategories;
            $scope.system = new Kitos.Models.ViewModel.ItSystem.SystemViewModel($scope.usage.itSystem);
            autofocus();


            var today = new Date();
            if (!$scope.usage.active) {
                if ($scope.usage.concluded < today && today < $scope.usage.expirationDate) {
                    $scope.displayActive = true;
                } else {
                    $scope.displayActive = false;
                }
            } else {
                $scope.displayActive = false;
            }

            if ($scope.usage.overviewId) {
                $scope.usage.overview = {
                    id: $scope.usage.overviewId,
                    text: $scope.usage.overviewItSystemName
                };
            }
            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            $scope.orgUnits = $scope.usage.usedBy;

            $scope.itSytemUsagesSelectOptions = selectLazyLoading("api/itSystemUsage", false, ["organizationId=" + $scope.usage.organizationId]);

            function selectLazyLoading(url: any, excludeSelf: any, paramAry: any);
            function selectLazyLoading(url, excludeSelf, paramAry) {
                return {
                    minimumInputLength: 1,
                    allowClear: true,
                    placeholder: " ",
                    initSelection(elem, callback) {
                    },
                    ajax: {
                        data(term, page) {
                            return { query: term };
                        },
                        quietMillis: 500,
                        transport(queryParams) {
                            var extraParams = paramAry ? "&" + paramAry.join("&") : "";
                            var res = $http.get(url + "?q=" + queryParams.data.query + extraParams).then(queryParams.success);
                            res.abort = () => null;

                            return res;
                        },

                        results(data, page) {
                            var results = [];

                            _.each(data.data.response, (obj: { id; itSystem; cvr; }) => {
                                if (excludeSelf && obj.id == $scope.usageId)
                                    return; // don't add self to result

                                results.push({
                                    id: obj.id,
                                    text: obj.itSystem.name,
                                    cvr: obj.cvr
                                });
                            });

                            return { results: results };
                        }
                    }
                };
            }
            $scope.patchDate = (field, value) => {
                var expirationDate = $scope.usage.expirationDate;
                var concluded = $scope.usage.concluded;
                var formatString = "DD-MM-YYYY";
                var formatDateString = "YYYY-MM-DD";
                var fromDate = moment(concluded, [formatString, formatDateString]).startOf("day");
                var endDate = moment(expirationDate, [formatString, formatDateString]).endOf("day");
                var date = moment(value, "DD-MM-YYYY");
                var today = moment();
                if (value === "" || value == undefined) {
                    var payload = {};
                    payload[field] = null;
                    patch(payload, $scope.autosaveUrl2 + "?organizationId=" + user.currentOrganizationId);
                } else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");
                }
                else if (fromDate != null && endDate != null && fromDate >= endDate) {
                    notify.addErrorMessage("Den indtastede slutdato er før startdatoen.");
                }
                else {
                    if (today.isBetween(moment($scope.usage.concluded, "DD-MM-YYYY").startOf("day"), moment($scope.usage.expirationDate, "DD-MM-YYYY").endOf("day"), null, "[]") ||
                        (today.isSameOrAfter(moment($scope.usage.concluded, "DD-MM-YYYY").startOf("day")) && $scope.usage.expirationDate == null) ||
                        (today.isSameOrBefore(moment($scope.usage.expirationDate, "DD-MM-YYYY").endOf("day")) && $scope.usage.concluded == null) ||
                        ($scope.usage.expirationDate == null && $scope.usage.concluded == null)) {
                        $scope.usage.isActive = true;
                    }
                    else {
                        if ($scope.usage.active) {
                            $scope.usage.isActive = true;
                        }
                        else {
                            $scope.usage.isActive = false;
                        }
                    }
                    var dateString = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = dateString;
                    patch(payload, $scope.autosaveUrl2 + "?organizationId=" + user.currentOrganizationId);
                }
            }

            function patch(payload: any, url: any);
            function patch(payload, url) {
                var msg = notify.addInfoMessage("Gemmer...", false);
                $http({ method: "PATCH", url: url, data: payload })
                    .success(() => {
                        msg.toSuccessMessage("Feltet er opdateret.");
                    })
                    .error(() => {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                    });
            }

            $scope.checkSystemValidity = () => {
                var today = moment();

                if (today.isBetween(moment($scope.usage.concluded, "DD-MM-YYYY").startOf("day"), moment($scope.usage.expirationDate, "DD-MM-YYYY").endOf("day"), null, "[]") ||
                    (today > moment($scope.usage.concluded, "DD-MM-YYYY").startOf("day") && $scope.usage.expirationDate == null) ||
                    (today < moment($scope.usage.expirationDate, "DD-MM-YYYY").endOf("day") && $scope.usage.concluded == null) ||
                    ($scope.usage.expirationDate == null && $scope.usage.concluded == null)) {
                    $scope.usage.isActive = true;
                }
                else {
                    if ($scope.usage.active) {
                        $scope.usage.isActive = true;
                    }
                    else {
                        $scope.usage.isActive = false;
                    }
                }
            }
        }
    ]);
})(angular, app);

(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.main', {
            url: '/main',
            templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-main.view.html',
            controller: 'system.EditMain',
            resolve: {
                businessTypes: [
                    '$http', function ($http) {
                        return $http.get("odata/LocalBusinessTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                archiveTypes: [
                    '$http', function ($http) {
                        return $http.get('odata/LocalArchiveTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc')
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                sensitiveDataTypes: [
                    '$http', function ($http) {
                        return $http.get('odata/LocalSensitiveDataTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc')
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                appTypeOptions: [
                    '$http', function ($http) {
                        return $http.get("odata/LocalItSystemTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(function (result) {
                            return result.data.value;
                        });
                    }
                ]
            }
        });
    }]);

    app.controller('system.EditMain', ['$rootScope', '$scope', '$http', '$stateParams', 'notify', 'itSystemUsage','user',
        'businessTypes', 'archiveTypes', 'sensitiveDataTypes', 'autofocus', 'hasWriteAccess', 'appTypeOptions',
        function ($rootScope, $scope, $http, $stateParams, notify, itSystemUsage,user, businessTypes, archiveTypes, sensitiveDataTypes, autofocus, hasWriteAccess, appTypeOptions) {
            $rootScope.page.title = 'IT System - Anvendelse';
            $scope.autoSaveUrl = 'api/itsystemusage/' + $stateParams.id;
            $scope.autosaveUrl2 = 'api/itsystemusage/' + $scope.usage.id;
            $scope.usage = itSystemUsage;
            $scope.usageId = $stateParams.id;
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.businessTypes = businessTypes;
            $scope.archiveTypes = archiveTypes;
            $scope.sensitiveDataTypes = sensitiveDataTypes;
            $scope.appTypeOptions = appTypeOptions;
            $scope.hasViewAccess = user.currentOrganizationId == itSystemUsage.organizationId;
            autofocus();
            var today = new Date();

            if (!itSystemUsage.active) {
                if (itSystemUsage.concluded < today && today < itSystemUsage.expirationDate) {
                    $scope.displayActive = true;
                } else {
                    $scope.displayActive = false;
                }
            } else {
                $scope.displayActive = false;
            }

            if (itSystemUsage.overviewId) {
                $scope.usage.overview = {
                    id: itSystemUsage.overviewId,
                    text: itSystemUsage.overviewItSystemName
                };
            }
            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            $scope.orgUnits = itSystemUsage.usedBy;

            $scope.itSytemUsagesSelectOptions = selectLazyLoading('api/itSystemUsage', false, ['organizationId=' + itSystemUsage.organizationId]);

            function selectLazyLoading(url, excludeSelf, paramAry) {
                return {
                    minimumInputLength: 1,
                    allowClear: true,
                    placeholder: ' ',
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

                            _.each(data.data.response, function (obj: { id; itSystem; cvr; }) {
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
                var date = moment(value, "DD-MM-YYYY");
                var today = moment();

                if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");
                }
                else {
                    if (today.isBetween(moment($scope.usage.concluded, "DD-MM-YYYY").startOf('day'), moment($scope.usage.expirationDate, "DD-MM-YYYY").endOf('day'), null, '[]') ||
                        (today.isSameOrAfter(moment($scope.usage.concluded, "DD-MM-YYYY").startOf('day')) && $scope.usage.expirationDate == null) ||
                        (today.isSameOrBefore(moment($scope.usage.expirationDate, "DD-MM-YYYY").endOf('day')) && $scope.usage.concluded == null) ||
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
                    patch(payload, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId);
                }
            }
            function patch(payload, url) {
                var msg = notify.addInfoMessage("Gemmer...", false);
                $http({ method: 'PATCH', url: url, data: payload })
                    .success(function () {
                        msg.toSuccessMessage("Feltet er opdateret.");
                    })
                    .error(function () {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                    });
            }

            $scope.checkSystemValidity = () => {
                var today = moment();

                if (today.isBetween(moment($scope.usage.concluded, "DD-MM-YYYY").startOf('day'), moment($scope.usage.expirationDate, "DD-MM-YYYY").endOf('day'), null, '[]') ||
                    (today > moment($scope.usage.concluded, "DD-MM-YYYY").startOf('day') && $scope.usage.expirationDate == null) ||
                    (today < moment($scope.usage.expirationDate, "DD-MM-YYYY").endOf('day') && $scope.usage.concluded == null) ||
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

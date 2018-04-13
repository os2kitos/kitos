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
            console.log($scope.usage);
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

            $scope.checkSystemValidity = () => {
                var expirationDateObject, concludedObject;
                var expirationDate = $scope.usage.expirationDate;
                var concluded = $scope.usage.concluded;
                var overrule = $scope.usage.active;

                var today = new Date();


                if (expirationDate) {
                    if (expirationDate.length > 10) {
                        //ISO format
                        expirationDateObject = new Date(expirationDate);

                    } else {
                        var splitArray = expirationDate.split("-");
                        expirationDateObject = new Date(splitArray[2], parseInt(splitArray[1], 10) - 1, splitArray[0]);
                    }
                }

                if (concluded) {
                    if (concluded.length > 10) {
                        //ISO format
                        concludedObject = new Date(concluded);

                    } else {
                        var splitArray = concluded.split("-");
                        concludedObject = new Date(splitArray[2], parseInt(splitArray[1], 10) - 1, splitArray[0]);
                    }
                }

                if (concluded && expirationDate) {

                    var isTodayBetween = (today > concludedObject.setHours(0, 0, 0, 0) && today < expirationDateObject.setHours(23, 59, 59, 999));

                }
                else if (concluded && !expirationDate) {

                    var isTodayBetween = (today > concludedObject.setHours(0, 0, 0, 0));

                }
                else if (!concluded && !expirationDate) {
                    isTodayBetween = true;

                }
                else if (!concluded && expirationDate) {

                    var isTodayBetween = (today < expirationDateObject.setHours(23, 59, 59, 999));

                }

                var isSystemActive = (isTodayBetween || overrule);

                $scope.usage.isActive = isSystemActive;
            }
        }
    ]);
})(angular, app);

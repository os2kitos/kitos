((ng, app) => {
    app.config([
        "$stateProvider", $stateProvider => {
            $stateProvider.state("it-system.edit.main", {
                url: "/main",
                templateUrl: "app/components/it-system/edit/tabs/it-system-edit-tab-main.view.html",
                controller: "system.SystemMainCtrl",
                resolve: {
                    businessTypes: [
                        "$http", $http => $http.get("odata/LocalBusinessTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                    ],
                    itSystem: ["$http", "$stateParams", ($http, $stateParams) => $http.get(`api/itsystem/${$stateParams.id}`)
                        .then(result => result.data.response)]
                }
            });
        }
    ]);

    app.controller("system.SystemMainCtrl",
        [
            "$rootScope", "$scope", "$http", "notify", "itSystem", "businessTypes", "user", "autofocus", "hasWriteAccess",
            ($rootScope, $scope, $http, notify, itSystem, businessTypes, user, autofocus, hasWriteAccess) => {
                $rootScope.page.title = "IT System - Rediger system";
                autofocus();

                itSystem.updateUrl = `api/itsystem/${itSystem.id}`;
                itSystem.belongsTo = (!itSystem.belongsToId) ? null : { id: itSystem.belongsToId, text: itSystem.belongsToName };
                itSystem.parent = (!itSystem.parentId) ? null : { id: itSystem.parentId, text: itSystem.parentName };

                $scope.select2AllowClearOpt = {
                    allowClear: true
                };
                
                $scope.system = itSystem;
                $scope.businessTypes = businessTypes.data.value;
                $scope.itSystemsSelectOptions = selectLazyLoading("api/itsystem", true, [`excludeId=${itSystem.id}`, `orgId=${user.currentOrganizationId}`]);
                $scope.organizationSelectOptions = selectLazyLoading("api/organization", true, [`orgId=${user.currentOrganizationId}`]);
            
                $scope.hasWriteAccess = hasWriteAccess;


                function selectLazyLoading(url: any, allowClear: any, paramAry: any);
                function selectLazyLoading(url, allowClear, paramAry) {
                    return {
                        minimumInputLength: 1,
                        initSelection: (elem, callback) => {
                        },
                        allowClear: allowClear,
                        ajax: {
                            data: (term, page) => ({ query: term }),
                            quietMillis: 500,
                            transport: (queryParams) => {
                                var extraParams = paramAry ? `&${paramAry.join("&")}` : "";
                                var res = $http.get(url + "?q=" + queryParams.data.query + extraParams).then(queryParams.success);
                                res.abort = () => null;
                                return res;
                            },
                            
                            results: (data, page) => {
                                var results = [];
                                if (url === "api/itsystem") {
                                    _.each(data.data.response, (obj: { id; name; disabled; }) => {
                                        if (obj.disabled === false) {
                                            results.push({
                                                id: obj.id,
                                                text: obj.name
                                            });
                                        }
                                    });
                                }
                                else {
                                    _.each(data.data.response, (obj: { id; name; }) => {
                                        results.push({
                                            id: obj.id,
                                            text: obj.name
                                        });
                                    }); 
                                }
                                return { results: results };
                            }
                        }
                    };
                }
            }
        ]);
})(angular, app);

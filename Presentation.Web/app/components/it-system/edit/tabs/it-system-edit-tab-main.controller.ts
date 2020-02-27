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
            "$rootScope", "$scope", "itSystem", "businessTypes", "user", "autofocus", "hasWriteAccess", "select2LoadingService",
            ($rootScope, $scope, itSystem, businessTypes, user, autofocus, hasWriteAccess, select2LoadingService) => {

                $rootScope.page.title = "IT System - Rediger system";
                autofocus();
                $scope.readMoreArchiveLinkUrl = Kitos.Constants.Archiving.ReadMoreUri;

                itSystem.updateUrl = `api/itsystem/${itSystem.id}`;
                itSystem.belongsTo = (!itSystem.belongsToId) ? null : { id: itSystem.belongsToId, text: itSystem.belongsToName };
                itSystem.parent = (!itSystem.parentId) ? null : { id: itSystem.parentId, text: itSystem.parentName };

                $scope.system = itSystem;
                $scope.businessTypes = businessTypes.data.value;

                $scope.itSystemsSelectOptions = select2LoadingService.loadSelect2(
                    "api/itsystem",
                    true,
                    [`excludeId=${itSystem.id}`, `orgId=${user.currentOrganizationId}`],
                    true);
                $scope.organizationSelectOptions = select2LoadingService.loadSelect2(
                    "api/organization",
                    true,
                    [`orgId=${user.currentOrganizationId}`],
                    true);
            
                $scope.hasWriteAccess = hasWriteAccess;

                $scope.isValidUrl = (ref: string) => {
                    if(ref !== null) {
                        return Kitos.Utility.Validation.validateUrl(ref);
                    }
                    return false;
                }
            }
        ]);
})(angular, app);

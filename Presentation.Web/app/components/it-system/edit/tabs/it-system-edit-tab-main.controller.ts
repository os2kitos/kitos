((ng, app) => {
    app.config([
        "$stateProvider", $stateProvider => {
            $stateProvider.state("it-system.edit.main", {
                url: "/main",
                templateUrl: "app/components/it-system/edit/tabs/it-system-edit-tab-main.view.html",
                controller: "system.SystemMainCtrl",
                resolve: {
                    businessTypes: [
                        "localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                        localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.BusinessTypes).getAll()
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
            ($rootScope, $scope, itSystem, businessTypes, user, autofocus, hasWriteAccess, select2LoadingService: Kitos.Services.ISelect2LoadingService) => {

                itSystem.accessModifier = String(itSystem.accessModifier); // Small fix to allow select2 to read a selected 0. Since it understands the string "0" but not the number 0. https://github.com/select2/select2/issues/4052. 

                $rootScope.page.title = "IT System - Rediger system";
                autofocus();
                $scope.readMoreArchiveLinkUrl = Kitos.Constants.Archiving.ReadMoreUri;

                $scope.autosaveUrl = `api/itsystem/${itSystem.id}`;
                itSystem.belongsTo = (!itSystem.belongsToId) ? null : { id: itSystem.belongsToId, text: itSystem.belongsToName };
                itSystem.parent = (!itSystem.parentId) ? null : { id: itSystem.parentId, text: Kitos.Helpers.SystemNameFormat.apply(itSystem.parentName, itSystem.parentDisabled), disabled: itSystem.parentDisabled };
                $scope.archiveRecommendations = Kitos.Models.ItSystem.ArchiveDutyRecommendationOptions.getAll();
                $scope.system = itSystem;
                $scope.businessTypes = businessTypes;
                $scope.isGlobalAdmin = user.isGlobalAdmin;

                $scope.itSystemsSelectOptions = select2LoadingService.loadSelect2(
                    "api/itsystem",
                    true,
                    [`excludeId=${itSystem.id}`, `orgId=${user.currentOrganizationId}`, `take=25`],
                    false);
                $scope.organizationSelectOptions = select2LoadingService.loadSelect2(
                    "api/organization",
                    true,
                    [`orgId=${user.currentOrganizationId}`, 'take=25'],
                    false);

                $scope.hasWriteAccess = hasWriteAccess;

                $scope.isValidUrl = (ref: string) => {
                    if (ref !== null) {
                        return Kitos.Utility.Validation.isValidExternalReference(ref);
                    }
                    return false;
                }
            }
        ]);
})(angular, app);

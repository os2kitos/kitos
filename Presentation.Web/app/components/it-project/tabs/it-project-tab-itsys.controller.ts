module Kitos.ItProject.Edit.ItSystems {
    "use strict";

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("it-project.edit.itsys", {
                url: "/itsys",
                templateUrl: "app/shared/select-it-systems/generic-tab-it-systems.view.html",
                controller: Shared.GenericTabs.SelectItSystems.SelectItSystemsController,
                controllerAs: "vm",
                resolve: {
                    usages: ["$http", "$stateParams", ($http, $stateParams) => $http.get(`api/itproject/${$stateParams.id}?usages=true`)
                        .then(result => result.data.response)],
                    systemSelectionOptions: [
                        "hasWriteAccess", "project", "$http", "usages", "user",
                        (hasWriteAccess: boolean, project, $http, usages, user: Services.IUser) =>
                            <Shared.GenericTabs.SelectItSystems.IGenericItSystemsSelectionConfiguration>
                            {
                                ownerName: project.name,
                                overviewHeader: "Projektet vedrører følgende IT Systemer",
                                searchFunction: (query: string) => {
                                    return $http
                                        .get(`api/itSystemUsage?organizationId=${user.currentOrganizationId}&q=${query}&take=25`)
                                        .then(
                                            result => result.data.response.map((usage: { id; itSystemName; itSystemDisabled; }) =>
                                                <Models.ViewModel.Generic.Select2OptionViewModel<any>>
                                                {
                                                    id: usage.id,
                                                    text: Helpers.SystemNameFormat.apply(usage.itSystemName, usage.itSystemDisabled)
                                                }),
                                            _ => []
                                        );
                                },
                                assignedSystems: (usages || []).map(usage => <Shared.GenericTabs.SelectItSystems.ISystemViewModel>
                                //Map to view model
                                    {
                                        id: usage.id,
                                        name: usage.itSystem.name,
                                        disabled: usage.itSystem.disabled
                                    }),
                                hasWriteAccess: hasWriteAccess,
                                assign: (systemId: number) => $http.post(`api/itproject/${project.id}?usageId=${systemId}&organizationId=${project.organizationId}`),
                                remove: (systemId: number) => $http.delete(`api/itproject/${project.id}?usageId=${systemId}&organizationId=${project.organizationId}`)
                            }
                    ]
                }
            });
        }]);
}

﻿module Kitos.LocalAdmin.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                organizationUuid: "@"
            },
            controller: FkOrganizationImportChangeLogLogController,
            controllerAs: "ctrl",
            templateUrl: `app/components/local-config/import/fk-organization-import-change-log.view.html`
        };
    }
    
    interface IFkOrganizationImportChangeLogController extends ng.IComponentController, Utility.KendoGrid.IGridViewAccess<IConsequenceRow> {
        organizationUuid: string;
        //consequences: Array<Models.Api.Organization.ConnectionUpdateOrganizationUnitConsequenceDTO>
    }

    class FkOrganizationImportChangeLogLogController implements IFkOrganizationImportChangeLogController {
        organizationUuid: string | null = null;
        //consequences: Array<Models.Api.Organization.ConnectionUpdateOrganizationUnitConsequenceDTO> | null = null;
        mainGrid: IKendoGrid<IConsequenceRow>;
        mainGridOptions: IKendoGridOptions<IConsequenceRow>;

        static $inject: string[] = ["kendoGridLauncherFactory", "$scope", "userService"];
        constructor(
            private readonly kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory,
            private readonly $scope: ng.IScope,
            private readonly userService: Kitos.Services.IUserService) {
        }

        $onInit() {
            if (!this.organizationUuid) {
                console.error("Missing parameter 'organizationUuid'");
                return;
            }

            //this.loadGrid();
        }
/*
        private loadGrid() {
            const kendoOptions = [
                Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory.Added,
                Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory.Renamed,
                Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory.Moved,
                Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory.Converted,
                Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory.Deleted
            ].map(optionType => {
                return <Kitos.Utility.KendoGrid.IKendoParameter>{
                    textValue: Kitos.Models.ViewModel.Organization.ConnectionUpdateOrganizationUnitChangeCategoryOptions.getText(optionType)
                };
            });

            this.userService.getUser().then(user => {
                this.kendoGridLauncherFactory
                    .create<IConsequenceRow>()
                    .withUser(user)
                    .withGridBinding(this)
                    .withFlexibleWidth()
                    .withArrayDataSource(this.consequences.map(row => {
                        return <IConsequenceRow>{
                            category: Kitos.Models.ViewModel.Organization.ConnectionUpdateOrganizationUnitChangeCategoryOptions.getText(row.category),
                            description: row.description,
                            name: row.name
                        };
                    }))
                    .withEntityTypeName("FK Organisation - Konsekvenser ved opdatering")
                    .withStorageKey("fkOrgConsequences")
                    .withScope(this.$scope)
                    .withoutPersonalFilterOptions()
                    .withoutGridResetControls()
                    .withDefaultPageSize(10)
                    .withColumn(builder => builder
                        .withId("name")
                        .withDataSourceName("name")
                        .withTitle("Organisationsenhed")
                        .withSourceValueEchoRendering()
                        .withFilteringOperation(Kitos.Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    )
                    .withColumn(builder => builder
                        .withId("category")
                        .withDataSourceName("category")
                        .withTitle("Ændring")
                        .withSourceValueEchoRendering()
                        .withFilteringOperation(Kitos.Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(kendoOptions, false)
                    )
                    .withColumn(builder => builder
                        .withId("description")
                        .withDataSourceName("description")
                        .withTitle("Beskrivelse")
                        .withSourceValueEchoRendering()
                        .withFilteringOperation(Kitos.Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    )
                    .resetAnySavedSettings()
                    .launch();
            });
        }*/
    }

    angular.module("app")
        .component("fkOrganizationImportChangeLog", setupComponent());
}
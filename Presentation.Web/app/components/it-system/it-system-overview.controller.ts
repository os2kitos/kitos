module Kitos.ItSystem.Overview {
    "use strict";

    export interface IOverviewController {
        mainGrid: IKendoGrid<IItSystemUsageOverview>;
        mainGridOptions: kendo.ui.GridOptions;
    }

    export interface IItSystemUsageOverview extends Models.ItSystemUsage.IItSystemUsageOverviewReadModel {
        roles: Array<string>;
    }

    export class OverviewController implements IOverviewController {
        private storageKey = "it-system-overview-options";

        mainGrid: IKendoGrid<IItSystemUsageOverview>;
        mainGridOptions: IKendoGridOptions<IItSystemUsageOverview>;
        static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "user",
            "kendoGridLauncherFactory",
            "needsWidthFixService",
            "overviewOptions",
            "_"
        ];

        constructor(
            $rootScope: IRootScope,
            $scope: any,
            user,
            kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory,
            needsWidthFixService: any,
            overviewOptions: Models.ItSystemUsage.IItSystemUsageOverviewOptionsDTO,
            _) {
            $rootScope.page.title = "IT System - Overblik";
            const orgUnits: Array<Models.Generic.Hierarchy.HierarchyNodeDTO> = _.addHierarchyLevelOnFlatAndSort(overviewOptions.organizationUnits, "id", "parentId");
            //Lookup maps
            var roleIdToUserNamesMap = {};

            //Helper functions
            const getRoleKey = (role: Models.Generic.Roles.BusinessRoleDTO) => `role${role.id}`;

            const replaceRoleQuery = (filterUrl, roleName, roleId) => {
                var pattern = new RegExp(`(\\w+\\()${roleName}(,.*?\\))`, "i");
                return filterUrl.replace(pattern, `RoleAssignments/any(c: $1c/UserFullName$2 and c/RoleId eq ${roleId})`);
            };

            const replaceOrderByProperty = (orderBy, fromProperty, toProperty) => {
                if (orderBy) {
                    var pattern = new RegExp(`(${fromProperty})(.*)`, "i");
                    return orderBy.replace(pattern, `${toProperty}$2`);
                }
                return orderBy;
            };

            //Build and launch kendo grid
            var launcher = kendoGridLauncherFactory
                .create<Models.ItSystemUsage.IItSystemUsageOverviewReadModel>()
                .withScope($scope)
                .withGridBinding(this)
                .withUser(user)
                .withEntityTypeName("IT System")
                .withExcelOutputName("IT Systemer Overblik")
                .withStorageKey(this.storageKey)
                .withFixedSourceUrl(
                    `/odata/Organizations(${user.currentOrganizationId
                    })/ItSystemUsageOverviewReadModels?$expand=RoleAssignments`)
                .withParameterMapping((options, type) => {
                    // get kendo to map parameters to an odata url
                    var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);
                    if (parameterMap.$filter) {
                        overviewOptions.systemRoles.forEach(role => {
                            parameterMap.$filter =
                                replaceRoleQuery(parameterMap.$filter, getRoleKey(role), role.id);
                        });
                    }

                    //In terms of ordering user will expect ordering by name on these columns, so we switch it around
                    parameterMap.$orderby = replaceOrderByProperty(parameterMap.$orderby, "ResponsibleOrganizationUnitId", "ResponsibleOrganizationUnitName");
                    parameterMap.$orderby = replaceOrderByProperty(parameterMap.$orderby, "ItSystemBusinessTypeId", "ItSystemBusinessTypeName");

                    if (parameterMap.$filter) {
                        //Redirect consolidated field search towards optimized search targets
                        parameterMap.$filter = parameterMap.$filter.replace(/(\w+\()ItSystemKLEIdsAsCsv(.*\))/, "ItSystemTaskRefs/any(c: $1c/KLEId$2)");
                        parameterMap.$filter = parameterMap.$filter.replace(/(\w+\()ItSystemKLENamesAsCsv(.*\))/, "ItSystemTaskRefs/any(c: $1c/KLEName$2)");
                        parameterMap.$filter = parameterMap.$filter.replace(/(\w+\()ItProjectNamesAsCsv(.*\))/, "ItProjects/any(c: $1c/ItProjectName$2)");
                        parameterMap.$filter = parameterMap.$filter.replace(new RegExp(`SensitiveDataLevelsAsCsv eq ('\\w+')`, "i"), "SensitiveDataLevels/any(c: c/SensitivityDataLevel eq $1)");
                    }

                    return parameterMap;
                })
                .withResponseParser(response => {
                    //Reset all response state
                    roleIdToUserNamesMap = {};

                    //Build lookups/mutations
                    response.forEach(systemUsage => {
                        roleIdToUserNamesMap[systemUsage.Id] = {};

                        //Update the role assignment map
                        if (systemUsage.RoleAssignments) {
                            systemUsage.RoleAssignments.forEach(assignment => {
                                if (!roleIdToUserNamesMap[systemUsage.Id][assignment.RoleId])
                                    roleIdToUserNamesMap[systemUsage.Id][assignment.RoleId] = assignment.UserFullName;
                                else {
                                    roleIdToUserNamesMap[systemUsage.Id][assignment.RoleId] += `, ${assignment.UserFullName}`;
                                }
                            });
                        }
                    });
                    return response;
                })
                .withToolbarEntry({
                    id: "roleSelector",
                    title: "Vælg systemrolle...",
                    color: Utility.KendoGrid.KendoToolbarButtonColor.Grey,
                    position: Utility.KendoGrid.KendoToolbarButtonPosition.Left,
                    implementation: Utility.KendoGrid.KendoToolbarImplementation.DropDownList,
                    enabled: () => true,
                    dropDownConfiguration: {
                        selectedOptionChanged: newItem => {
                            // hide all roles column
                            overviewOptions.systemRoles.forEach(role => {
                                this.mainGrid.hideColumn(getRoleKey(role));
                            });

                            //Only show the selected role
                            var gridFieldName = getRoleKey(newItem.originalObject);
                            this.mainGrid.showColumn(gridFieldName);
                            needsWidthFixService.fixWidth();
                        },
                        availableOptions: overviewOptions.systemRoles.map(role => {
                            return {
                                id: `${role.id}`,
                                text: role.name,
                                originalObject: role
                            };
                        })
                    }
                } as Utility.KendoGrid.IKendoToolbarEntry)
                .withToolbarEntry({
                    id: "gdprExportAnchor",
                    title: "Exportér GPDR data til Excel",
                    color: Utility.KendoGrid.KendoToolbarButtonColor.Grey,
                    position: Utility.KendoGrid.KendoToolbarButtonPosition.Right,
                    implementation: Utility.KendoGrid.KendoToolbarImplementation.Link,
                    enabled: () => true,
                    link: `api/v1/gdpr-report/csv/${user.currentOrganizationId}`
                } as Utility.KendoGrid.IKendoToolbarEntry)
                .withColumn(builder =>
                    builder
                        .withDataSourceName("IsActive")
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Boolean)
                        .withTitle("Gyldig/Ikke gyldig")
                        .withId("isActive")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange([
                            {
                                textValue: "Gyldig",
                                remoteValue: true
                            },
                            {
                                textValue: "Ikke gyldig",
                                remoteValue: false
                            }
                        ],
                            false)
                        .withRendering(dataItem => dataItem.IsActive ? '<span class="fa fa-file text-success" aria-hidden="true"></span>' : '<span class="fa fa-file-o text-muted" aria-hidden="true"></span>')
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withExcelOutput(dataItem => dataItem.IsActive ? "Gyldig" : "Ikke gyldig"))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LocalSystemId")
                        .withTitle("Lokal System ID")
                        .withId("localid")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withInitialVisibility(false))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ItSystemUuid")
                        .withTitle("UUID")
                        .withId("uuid")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withInitialVisibility(false))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ParentItSystemName")
                        .withTitle("Overordnet IT System")
                        .withId("parentsysname")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-parent-system-rendering`, "it-system.edit.main", dataItem.ParentItSystemId, Helpers.SystemNameFormat.apply(dataItem.ParentItSystemName, false))) //TODO: Missing property from backend
                        .withSourceValueEchoExcelOutput()
                        .withInitialVisibility(false))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Name")
                        .withTitle("IT System")
                        .withId("sysname")
                        .withStandardWidth(320)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-system-usage-rendering`, "it-system.usage.main", dataItem.SourceEntityId, Helpers.SystemNameFormat.apply(dataItem.Name, dataItem.ItSystemDisabled)))
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Version")
                        .withTitle("Version")
                        .withId("version")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withInitialVisibility(false))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LocalCallName")
                        .withTitle("Lokal kaldenavn")
                        .withId("localname")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withInitialVisibility(false))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ResponsibleOrganizationUnitId") //Using org unit id for better search performance and org unit name is used during sorting (in the parameter mapper)
                        .withTitle("Ansv. organisationsenhed")
                        .withId("orgunit")
                        .withStandardWidth(190)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Number)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            orgUnits.map((unit) => {
                                return {
                                    textValue: unit.name,
                                    remoteValue: unit.id,
                                    optionalContext: unit
                                };
                            }),
                            false,
                            dataItem => "&nbsp;&nbsp;&nbsp;&nbsp;".repeat(dataItem.optionalContext.$level) + dataItem.optionalContext.name)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(dataItem.ResponsibleOrganizationUnitName))
                        .withExcelOutput(dataItem => Helpers.RenderFieldsHelper.renderString(dataItem.ResponsibleOrganizationUnitName)))
                .withStandardSorting("Name");

            overviewOptions.systemRoles.forEach(role =>
                launcher = launcher.withColumn(builder =>
                    builder
                        .withDataSourceName(getRoleKey(role))
                        .withTitle(role.name)
                        .withId(`systemUsage${getRoleKey(role)}`)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withoutSorting() //Sorting is not possible on expressions which are required on role columns since they are generated in the UI as a result of content of a complex typed child collection
                        .withContentOverflow()
                        .withInitialVisibility(false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-system-usage-${getRoleKey(role)}-rendering`, "it-system.usage.roles", dataItem.SourceEntityId, roleIdToUserNamesMap[dataItem.Id][role.id]))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(roleIdToUserNamesMap[dataItem.Id][role.id])))
            );

            launcher = launcher
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ItSystemBusinessTypeId") //Using type id for better search performance and type id is used during sorting (in the parameter mapper)
                        .withTitle("Forretningstype")
                        .withId("busitype")
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Number)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            overviewOptions.businessTypes.map((unit: any) => {
                                return {
                                    textValue: unit.name,
                                    remoteValue: unit.id,
                                };
                            }),
                            false)
                        .withContentOverflow()
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(dataItem.ItSystemBusinessTypeName))
                        .withExcelOutput(dataItem => Helpers.RenderFieldsHelper.renderString(dataItem.ItSystemBusinessTypeName)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ItSystemKLEIdsAsCsv") //Using csv for rendering and sorting and the collection for indexed search
                        .withTitle("KLE ID")
                        .withId("taskkey")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.StartsWith)
                        .withInitialVisibility(false)
                        .withContentOverflow()
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ItSystemKLENamesAsCsv") //Using csv for rendering and sorting and the collection for indexed search
                        .withTitle("KLE navn")
                        .withId("klename")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withContentOverflow()
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LocalReferenceTitle")
                        .withTitle("Lokal Reference")
                        .withId("ReferenceId")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Left)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderReference(dataItem.LocalReferenceTitle, dataItem.LocalReferenceUrl))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderReference(dataItem.LocalReferenceTitle, dataItem.LocalReferenceUrl)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LocalReferenceDocumentId")
                        .withTitle("Dokument ID / Sagsnr.")
                        .withId("folderref")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withInitialVisibility(false)
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput())

                .withColumn(builder =>
                    builder
                        .withDataSourceName("SensitiveDataLevelsAsCsv")
                        .withTitle("Datatype")
                        .withId("dataLevel")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange
                        (
                            [
                                Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels.none,
                                Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels.personal,
                                Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels.sensitive,
                                Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels.legal
                            ]
                                .map(option => {
                                    return {
                                        textValue: option.text,
                                        remoteValue: option.value
                                    };
                                }
                                ),
                            false)
                        .withInitialVisibility(false)
                        .withContentOverflow()
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("HasMainContract")
                        .withTitle("Kontrakt")
                        .withId("contract")
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Boolean)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange
                        (
                            [
                                {
                                    textValue: "Har kontrakt",
                                    remoteValue: true
                                },
                                {
                                    textValue: "Ingen kontrakt",
                                    remoteValue: false
                                }
                            ]
                            ,
                            false)
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withRendering(dataItem => {

                            if (dataItem.MainContractIsActive == null) {
                                return "";
                            }
                            const decorationClass = dataItem.MainContractIsActive
                                ? "fa-file text-success"
                                : "fa-file-o text-muted";
                            return `<a data-ui-sref="it-system.usage.contracts({id: ${dataItem.Id}})"><span class="fa ${decorationClass}" aria-hidden="true"></span></a>`;
                        })
                        .withExcelOutput(dataItem => dataItem.MainContractIsActive ? "True" : ""))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("MainContractSupplierName")
                        .withTitle("Leverandør")
                        .withId("supplier")
                        .withStandardWidth(175)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withContentOverflow()
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ItSystemRightsHolderName")
                        .withTitle("Rettighedshaver")
                        .withId("belongsto")
                        .withStandardWidth(210)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withContentOverflow()
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ItProjectNamesAsCsv")
                        .withTitle("IT Projekt")
                        .withId("sysusage")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withInitialVisibility(false)
                        .withContentOverflow()
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ObjectOwnerName")
                        .withTitle("Taget i anvendelse af")
                        .withId("ownername")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withInitialVisibility(false)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LastChangedByName")
                        .withTitle("Sidst redigeret: Bruger")
                        .withId("lastchangedname")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withInitialVisibility(false)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LastChanged")
                        .withTitle("Sidste redigeret: Dato")
                        .withId("changed")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withInitialVisibility(false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.LastChanged))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderDate(dataItem.LastChanged)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Concluded")
                        .withTitle("Ibrugtagningsdato")
                        .withId("concludedSystemFrom")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.Concluded))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderDate(dataItem.Concluded)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ArchiveDuty")
                        .withTitle("Arkiveringspligt")
                        .withId("ArchiveDuty")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange([
                            Models.ViewModel.ItSystemUsage.ArchiveDutyViewModel.archiveDuties.Undecided,
                            Models.ViewModel.ItSystemUsage.ArchiveDutyViewModel.archiveDuties.B,
                            Models.ViewModel.ItSystemUsage.ArchiveDutyViewModel.archiveDuties.K,
                            Models.ViewModel.ItSystemUsage.ArchiveDutyViewModel.archiveDuties.Unknown
                        ].map(value => {
                            return {
                                textValue: value.text,
                                remoteValue: value.textValue
                            }
                        })
                            , false)
                        .withRendering(dataItem => Models.Odata.ItSystemUsage.ArchiveDutyMapper.map(dataItem.ArchiveDuty))
                        .withExcelOutput(dataItem => Models.Odata.ItSystemUsage.ArchiveDutyMapper.map(dataItem.ArchiveDuty)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("IsHoldingDocument")
                        .withTitle("Er dokumentbærende")
                        .withId("Registertype")
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Boolean)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange([
                            {
                                textValue: "Ja",
                                remoteValue: true
                            },
                            {
                                textValue: "Nej",
                                remoteValue: false
                            }
                        ]
                            , false)
                        .withRendering(dataItem => dataItem.IsHoldingDocument ? "Ja" : "Nej")
                        .withExcelOutput(dataItem => dataItem.IsHoldingDocument ? "Ja" : "Nej"));

            //Launch kendo grid
            launcher.launch();
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-system.overview", {
                    url: "/overview",
                    templateUrl: "app/components/it-system/it-system-overview.view.html",
                    controller: OverviewController,
                    controllerAs: "systemOverviewVm",
                    resolve: {
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        overviewOptions: [
                            "itSystemUsageOptionsService",
                            (itSystemUsageOptionsService: Services.ItSystemUsage.IItSystemUsageOptionsService) => itSystemUsageOptionsService.getOptions()
                        ]
                    }
                });
            }
        ]);
}
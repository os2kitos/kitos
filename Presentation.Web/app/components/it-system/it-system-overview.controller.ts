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
            "_",
            "gridStateService"
        ];

        constructor(
            $rootScope: IRootScope,
            $scope: any,
            user,
            kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory,
            needsWidthFixService: any,
            overviewOptions: Models.ItSystemUsage.IItSystemUsageOverviewOptionsDTO,
            _,
            gridStateService: Services.IGridStateFactory //TODO: JMO - fjern denne + fra injection array
        ) {
            $rootScope.page.title = "IT System - Overblik";
            const orgUnits: Array<Models.Generic.Hierarchy.HierarchyNodeDTO> = _.addHierarchyLevelOnFlatAndSort(overviewOptions.organizationUnits, "id", "parentId");
            const itSystemUsageOverviewType = Models.Generic.OverviewType.ItSystemUsage;
            //Lookup maps
            var roleIdToUserNamesMap = {};
            var roleIdToEmailMap = {};
            //Helper functions
            const agreementConcludedIsDefined =
                (registration: Models.ItSystemUsage.IItSystemUsageOverviewDataProcessingRegistrationReadModel) =>
                    registration.IsAgreementConcluded !== null &&
                    registration.IsAgreementConcluded !==
                    Models.Api.Shared.YesNoIrrelevantOption[Models.Api.Shared.YesNoIrrelevantOption.UNDECIDED];
            const getRoleKey = (role: Models.Generic.Roles.BusinessRoleDTO) => `role${role.id}`;
            // Re-enable as part of: https://os2web.atlassian.net/browse/KITOSUDV-1674
            //var gridState = gridStateService.getService(this.storageKey, user, itSystemUsageOverviewType);
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
                .withOverviewType(itSystemUsageOverviewType)
                .withStorageKey(this.storageKey)
                .withUrlFactory(options => {
                    const commonQuery =
                        "?$expand=RoleAssignments,DataProcessingRegistrations,DependsOnInterfaces,IncomingRelatedItSystemUsages,OutgoingRelatedItSystemUsages";
                    const baseUrl =
                        `/odata/Organizations(${user.currentOrganizationId})/ItSystemUsageOverviewReadModels${commonQuery}`;
                    var additionalQuery = "";
                    const selectedOrgId: number | null = options.currentOrgUnit;
                    if (selectedOrgId !== null) {
                        additionalQuery = `&responsibleOrganizationUnitId=${selectedOrgId}`;
                    }
                    return `${baseUrl}${additionalQuery}`;
                })
                .withParameterMapping((options, type) => {
                    //Defaults
                    var activeOrgUnit: number | null = null;

                    // get kendo to map parameters to an odata url
                    var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);
                    if (parameterMap.$filter) {
                        overviewOptions.systemRoles.forEach(role => {
                            parameterMap.$filter =
                                replaceRoleQuery(parameterMap.$filter, getRoleKey(role), role.id);
                        });
                    }

                    //In terms of ordering user will expect ordering by name on these columns, so we switch it around
                    parameterMap.$orderby = replaceOrderByProperty(parameterMap.$orderby,
                        "ResponsibleOrganizationUnitId",
                        "ResponsibleOrganizationUnitName");
                    parameterMap.$orderby = replaceOrderByProperty(parameterMap.$orderby,
                        "ItSystemBusinessTypeId",
                        "ItSystemBusinessTypeName");

                    if (parameterMap.$filter) {
                        //Redirect consolidated field search towards optimized search targets
                        parameterMap.$filter = parameterMap.$filter
                            .replace(/(\w+\()ItSystemKLEIdsAsCsv(.*\))/, "ItSystemTaskRefs/any(c: $1c/KLEId$2)")
                            .replace(/(\w+\()ItSystemKLENamesAsCsv(.*\))/, "ItSystemTaskRefs/any(c: $1c/KLEName$2)")
                            .replace(/(\w+\()ItProjectNamesAsCsv(.*\))/, "ItProjects/any(c: $1c/ItProjectName$2)")
                            .replace(new RegExp(`SensitiveDataLevelsAsCsv eq ('\\w+')`, "i"),
                                "SensitiveDataLevels/any(c: c/SensitivityDataLevel eq $1)")
                            .replace(/(\w+\()DataProcessingRegistrationNamesAsCsv(.*\))/,
                                "DataProcessingRegistrations/any(c: $1c/DataProcessingRegistrationName$2)")
                            .replace(/(\w+\()DependsOnInterfacesNamesAsCsv(.*\))/,
                                "DependsOnInterfaces/any(c: $1c/InterfaceName$2)")
                            .replace(/(\w+\()IncomingRelatedItSystemUsagesNamesAsCsv(.*\))/,
                                "IncomingRelatedItSystemUsages/any(c: $1c/ItSystemUsageName$2)");

                        //Concluded has a special case for UNDECIDED | NULL which must be treated the same, so first we replace the expression to point to the collection and then we redefine it
                        const dprUndecidedQuery = `DataProcessingRegistrations/any(c: c/IsAgreementConcluded eq '${Models.Api.Shared.YesNoIrrelevantOption.UNDECIDED}' or c/IsAgreementConcluded eq null) or (DataProcessingRegistrations/any() eq false)`;
                        parameterMap.$filter = parameterMap.$filter
                            .replace(new RegExp(`DataProcessingRegistrationsConcludedAsCsv eq ('\\w+')`, "i"), "DataProcessingRegistrations/any(c: c/IsAgreementConcluded eq $1)")
                            .replace(new RegExp(`DataProcessingRegistrations\\/any\\(c: c\\/IsAgreementConcluded eq '${Models.Api.Shared.YesNoIrrelevantOption.UNDECIDED}'\\)`, "i"), dprUndecidedQuery);

                        // Org unit is stripped from the odata query and passed on to the url factory!
                        const captureOrgUnit = new RegExp(`ResponsibleOrganizationUnitId eq (\\d+)`, "i");
                        if (captureOrgUnit.test(parameterMap.$filter) === true) {
                            activeOrgUnit = parseInt(captureOrgUnit.exec(parameterMap.$filter)[1]);
                        }
                        parameterMap.$filter =
                            parameterMap.$filter.replace(captureOrgUnit,
                                ""); //Org unit id is handled by the url factory since it is not a regular odata query

                        //Cleanup broken queries due to stripping
                        parameterMap.$filter = parameterMap.$filter
                            .replace("and  and", "and") //in the middle of other criteria
                            .replace(/\( and /, "(") //First criteria removed
                            .replace(/ and \)/, ")"); // Last criteria removed

                        //Cleanup filter if invalid
                        if (parameterMap.$filter === "") {
                            delete parameterMap.$filter;
                        }
                    }

                    //Making sure orgunit is set
                    (options as any).currentOrgUnit = activeOrgUnit;

                    return parameterMap;
                })
                .withResponseParser(response => {
                    //Reset all response state
                    roleIdToUserNamesMap = {};
                    roleIdToEmailMap = {};

                    //Build lookups/mutations
                    response.forEach(systemUsage => {
                        roleIdToUserNamesMap[systemUsage.Id] = {};
                        roleIdToEmailMap[systemUsage.Id] = {};

                        //Update the role assignment map
                        if (systemUsage.RoleAssignments) {
                            systemUsage.RoleAssignments.forEach(assignment => {
                                //Patch names
                                if (!roleIdToUserNamesMap[systemUsage.Id][assignment.RoleId])
                                    roleIdToUserNamesMap[systemUsage.Id][assignment.RoleId] = assignment.UserFullName;
                                else {
                                    roleIdToUserNamesMap[systemUsage.Id][assignment.RoleId] += `, ${assignment
                                        .UserFullName}`;
                                }
                                //Patch emails
                                if (!roleIdToEmailMap[systemUsage.Id][assignment.RoleId])
                                    roleIdToEmailMap[systemUsage.Id][assignment.RoleId] = assignment.Email;
                                else {
                                    roleIdToEmailMap[systemUsage.Id][assignment.RoleId] += `, ${assignment.Email}`;
                                }
                            });
                        }
                    });
                    return response;
                })
                //TODO JMO: remove this
                // This part should not be visible for anyone just yet. Will be reintroduced in: https://os2web.atlassian.net/browse/KITOSUDV-1674

                //.withToolbarEntry({
                //    id: "filterOrg",
                //    title: "Gem filter for organisation",
                //    color: Utility.KendoGrid.KendoToolbarButtonColor.Grey,
                //    position: Utility.KendoGrid.KendoToolbarButtonPosition.Left,
                //    implementation: Utility.KendoGrid.KendoToolbarImplementation.Button,
                //    enabled: () => true,
                //    onClick: () => {
                //        if (confirm('Er du sikker på at du vil gemme nuværende filtre, sorteringer og opsætning af felter som standard til ' + user.currentOrganizationName)) {
                //            gridState.saveGridProfileForOrg(this.mainGrid, itSystemUsageOverviewType);
                //        }

                //    },
                //    show: user.isLocalAdmin,
                //} as Utility.KendoGrid.IKendoToolbarEntry)
                //.withToolbarEntry({
                //    id: "removeFilterOrg",
                //    title: "Slet filter for organisation",
                //    color: Utility.KendoGrid.KendoToolbarButtonColor.Grey,
                //    position: Utility.KendoGrid.KendoToolbarButtonPosition.Left,
                //    implementation: Utility.KendoGrid.KendoToolbarImplementation.Button,
                //    enabled: () => true,
                //    onClick: () => {
                //        if (confirm('Er du sikker på at du vil slette standard opsætningen af felter til ' + user.currentOrganizationName)) {
                //            gridState.deleteGridProfileForOrg(itSystemUsageOverviewType);
                //        }
                //    },
                //    show: user.isLocalAdmin,
                //} as Utility.KendoGrid.IKendoToolbarEntry)


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
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-parent-system-rendering`, "it-system.edit.main", dataItem.ParentItSystemId, Helpers.SystemNameFormat.apply(dataItem.ParentItSystemName, dataItem.ParentItSystemDisabled)))
                        .withExcelOutput(dataItem => Helpers.SystemNameFormat.apply(dataItem.ParentItSystemName, dataItem.ParentItSystemDisabled))
                        .withInitialVisibility(false))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("SystemName")
                        .withTitle("IT System")
                        .withId("sysname")
                        .withStandardWidth(320)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-system-usage-rendering`, "it-system.usage.main", dataItem.SourceEntityId, Helpers.SystemNameFormat.apply(dataItem.SystemName, dataItem.ItSystemDisabled)))
                        .withExcelOutput(dataItem => Helpers.SystemNameFormat.apply(dataItem.SystemName, dataItem.ItSystemDisabled)))
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
                .withStandardSorting("SystemName");

            overviewOptions.systemRoles.forEach(role =>
                launcher = launcher
                    .withColumn(builder =>
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
                    .withExcelOnlyColumn(builder =>
                        builder
                            .withId(`systemUsage${getRoleKey(role)}_emails`)
                            .withDataSourceName(getRoleKey(role))
                            .withTitle(`${role.name} Email"`)
                            .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(roleIdToEmailMap[dataItem.Id][role.id]))
                    )
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
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderUrlWithTitle(dataItem.LocalReferenceTitle, dataItem.LocalReferenceUrl))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderUrlWithOptionalTitle(dataItem.LocalReferenceTitle, dataItem.LocalReferenceUrl)))
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
                            return `<a data-ui-sref="it-system.usage.contracts({id: ${dataItem.SourceEntityId}})"><span class="fa ${decorationClass}" aria-hidden="true"></span></a>`;
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
                        .withDataSourceName("LastChangedAt")
                        .withTitle("Sidste redigeret: Dato")
                        .withId("changed")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withInitialVisibility(false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.LastChangedAt))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderDate(dataItem.LastChangedAt)))
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
                        .withExcelOutput(dataItem => dataItem.IsHoldingDocument ? "Ja" : "Nej"))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ActiveArchivePeriodEndDate")
                        .withTitle("Journalperiode slutdato")
                        .withId("ArchivePeriodsEndDate")
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withoutSorting()   //NOTICE: NO sorting OR filtering on computed field!
                        .withInitialVisibility(false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.ActiveArchivePeriodEndDate))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderDate(dataItem.ActiveArchivePeriodEndDate)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("RiskSupervisionDocumentationName")
                        .withTitle("Risikovurdering")
                        .withId("riskSupervisionDocumentationUrlName")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withInitialVisibility(false)
                        .withContentOverflow()
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderUrlWithTitle(dataItem.RiskSupervisionDocumentationName, dataItem.RiskSupervisionDocumentationUrl))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderUrlOrFallback(dataItem.RiskSupervisionDocumentationUrl, dataItem.RiskSupervisionDocumentationName)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LinkToDirectoryName")
                        .withTitle("Fortegnelse")
                        .withId("LinkToDirectoryUrlName")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withInitialVisibility(false)
                        .withContentOverflow()
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderReference(dataItem.LinkToDirectoryName, dataItem.LinkToDirectoryUrl))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderUrlOrFallback(dataItem.LinkToDirectoryUrl, dataItem.LinkToDirectoryName)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("HostedAt")
                        .withTitle("IT systemet driftes")
                        .withId("HostedAt")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Models.ViewModel.ItSystemUsage.HostedAtOptions.options.map(value => {
                                return {
                                    textValue: value.text,
                                    remoteValue: value.id
                                }
                            })
                            , false)
                        .withRendering(dataItem => Models.Odata.ItSystemUsage.HostedAtMapper.map(dataItem.HostedAt))
                        .withExcelOutput(dataItem => Models.Odata.ItSystemUsage.HostedAtMapper.map(dataItem.HostedAt)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("GeneralPurpose")
                        .withTitle("Systemets overordnede formål")
                        .withId("GeneralPurpose")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withContentOverflow()
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("DataProcessingRegistrationsConcludedAsCsv")
                        .withTitle("Databehandleraftale er indgået")
                        .withId("dataProcessingAgreementConcluded")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            [
                                Models.Api.Shared.YesNoIrrelevantOption.YES,
                                Models.Api.Shared.YesNoIrrelevantOption.NO,
                                Models.Api.Shared.YesNoIrrelevantOption.IRRELEVANT,
                                Models.Api.Shared.YesNoIrrelevantOption.UNDECIDED
                            ].map(value => {
                                return {
                                    textValue: Models.ViewModel.Shared.YesNoIrrelevantOptions.getText(value),
                                    remoteValue: value
                                };
                            }),
                            false
                        )
                        .withInitialVisibility(false)
                        .withContentOverflow()
                        .withRendering(dataItem => dataItem
                            .DataProcessingRegistrations
                            .filter(registration => agreementConcludedIsDefined(registration))
                            .map(registration => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-dpr-link`, "data-processing.edit-registration.main", registration.DataProcessingRegistrationId, Models.ViewModel.Shared.YesNoIrrelevantOptions.getText(Models.Api.Shared.YesNoIrrelevantOption[registration.IsAgreementConcluded])))
                            .reduce((combined: string, next: string, __) => combined.length === 0 ? next : `${combined}, ${next}`, ""))
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("DataProcessingRegistrationNamesAsCsv")
                        .withTitle("Databehandling")
                        .withId("dataProcessingRegistrations")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withInitialVisibility(false)
                        .withContentOverflow()
                        .withRendering(dataItem => dataItem
                            .DataProcessingRegistrations
                            .map(registration => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-dpr-link`, "data-processing.edit-registration.main", registration.DataProcessingRegistrationId, registration.DataProcessingRegistrationName))
                            .reduce((combined: string, next: string, __) => combined.length === 0 ? next : `${combined}, ${next}`, ""))
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("OutgoingRelatedItSystemUsagesNamesAsCsv")
                        .withTitle("Anvendte systemer")
                        .withId("outgoingRelatedItSystemUsages")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withInitialVisibility(false)
                        .withContentOverflow()
                        .withRendering(dataItem => dataItem
                            .OutgoingRelatedItSystemUsages
                            .map(relatedItSystemUsage => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-system-usage-link`, "it-system.usage.main", relatedItSystemUsage.ItSystemUsageId, relatedItSystemUsage.ItSystemUsageName))
                            .reduce((combined: string, next: string, __) => combined.length === 0 ? next : `${combined}, ${next}`, ""))
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("DependsOnInterfacesNamesAsCsv")
                        .withTitle("Anvendte snitflader")
                        .withId("dependsOnInterfaces")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withInitialVisibility(false)
                        .withContentOverflow()
                        .withRendering(dataItem => dataItem
                            .DependsOnInterfaces
                            .map(dependsOnInterface => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-interface-link`, "it-system.interface-edit.main", dependsOnInterface.InterfaceId, dependsOnInterface.InterfaceName))
                            .reduce((combined: string, next: string, __) => combined.length === 0 ? next : `${combined}, ${next}`, ""))
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("IncomingRelatedItSystemUsagesNamesAsCsv")
                        .withTitle("Systemer der anvender systemet")
                        .withId("incomingRelatedItSystemUsages")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withInitialVisibility(false)
                        .withContentOverflow()
                        .withRendering(dataItem => dataItem
                            .IncomingRelatedItSystemUsages
                            .map(relatedItSystemUsage => Helpers.RenderFieldsHelper.renderInternalReference(`kendo-system-usage-link`, "it-system.usage.main", relatedItSystemUsage.ItSystemUsageId, relatedItSystemUsage.ItSystemUsageName))
                            .reduce((combined: string, next: string, __) => combined.length === 0 ? next : `${combined}, ${next}`, ""))
                        .withSourceValueEchoExcelOutput());

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
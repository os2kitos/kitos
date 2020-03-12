module Kitos.ItSystem.Catalog {
    "use strict";

    export interface ICatalogController {
        mainGrid: IKendoGrid<Models.ItSystem.IItSystem>;
        mainGridOptions: kendo.ui.GridOptions;
        usageDetailsGrid: kendo.ui.GridOptions;
        usageGrid: kendo.ui.Grid;
        modal: kendo.ui.Window;
        modalMigration: kendo.ui.Window;
        modalMigrationConsequence: kendo.ui.Window;
        saveGridProfile(): void;
        loadGridProfile(): void;
        clearGridProfile(): void;
        doesGridProfileExist(): boolean;
        clearOptions(): void;
        enableUsage(dataItem): void;
        removeUsage(dataItem): void;
        allowToggleUsage: boolean;
    }

    export interface ISelect2Scope extends ng.IScope {
        mySelectOptions: any;
    }

    export class CatalogController implements ICatalogController {
        private storageKey = "it-system-catalog-options";
        private gridState = this.gridStateService.getService(this.storageKey, this.user.id);
        public mainGrid: IKendoGrid<Models.ItSystem.IItSystem>;
        public mainGridOptions: IKendoGridOptions<Models.ItSystem.IItSystem>;
        public usageGrid: kendo.ui.Grid;
        public modalMigrationConsequence: kendo.ui.Window;
        public modal: kendo.ui.Window;
        public modalMigration: kendo.ui.Window;
        public canCreate = this.userAccessRights.canCreate;
        public canMigrate = this.userMigrationRights.canExecuteMigration;
        public migrationReportDTO: Models.ItSystemUsage.Migration.IItSystemUsageMigrationDTO;
        public allowToggleUsage = false;

        public static $inject:
            Array<string> = [
                "$rootScope",
                "$scope",
                "$http",
                "$timeout",
                "$state",
                "$sce",
                "$",
                "_",
                "moment",
                "notify",
                "user",
                "userAccessRights",
                "userMigrationRights",
                "gridStateService",
                "$uibModal",
                "exportGridToExcelService",
                "systemUsageUserAccessRights"
            ];

        constructor(
            private $rootScope: IRootScope,
            private $scope: ISelect2Scope,
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private $state: ng.ui.IStateService,
            private $sce: ng.ISCEService,
            private $: JQueryStatic,
            private _: ILoDashWithMixins,
            private moment: moment.MomentStatic,
            private notify,
            private user,
            private userAccessRights,
            private userMigrationRights,
            private gridStateService: Services.IGridStateFactory,
            private $uibModal,
            private exportGridToExcelService,
            private systemUsageUserAccessRights: Models.Api.Authorization.EntitiesAccessRightsDTO,
            private oldItSystemName,
            private oldItSystemId,
            private oldItSystemUsageId,
            private newItSystemObject,
            private municipalityId,
            private municipalityName,
            public migrationConsequenceText) {
            this.allowToggleUsage = systemUsageUserAccessRights.canCreate;
            $rootScope.page.title = "IT System - Katalog";
            $scope.$on("kendoWidgetCreated", (event, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.mainGrid) {
                    this.loadGridOptions();

                    this.mainGrid.dataSource.read();

                    // find the access modifier filter row section
                    var accessModifierFilterRow = $(".k-filter-row [data-field='AccessModifier']");
                    // find the access modifier kendo widget
                    var accessModifierFilterWidget = accessModifierFilterRow.find("input").data("kendoDropDownList");
                    // attach a click event to the X (remove filter) button
                    accessModifierFilterRow.find("button").on("click", () => {
                        // set the selected filter to none, because clicking the button removes the filter
                        accessModifierFilterWidget.select(0);
                    });

                    // show loadingbar when export to excel is clicked
                    // hidden again in method exportToExcel callback
                    $(".k-grid-excel").click(() => {
                        kendo.ui.progress(this.mainGrid.element, true);
                    });

                }

            });

            $scope.mySelectOptions = {
                minimumInputLength: 1,
                dropdownCss: { 'z-index': 100000, },
                ajax: {
                    data: (term, _) => { return { query: term } },
                    quietMillis: 500,
                    transport: (queryParams) => {
                        var request = $http.get(
                            "api/v1/ItSystemUsageMigration/UnusedItSystems?" +
                            `organizationId=${this.municipalityId}` +
                            `&nameContent=${queryParams.data.query}` +
                            "&numberOfItSystems=25" +
                            "&getPublicFromOtherOrganizations=true");
                        request.then(queryParams.success);

                        request.catch(() => {
                            this.closeSelectionDropdown();
                            this.notify.addErrorMessage("Der skete en fejl. Kunne ikke hente it systemer.");
                        });
                        return request;
                    },

                    results: (data, page) => {
                        var results = [];

                        //for each system usages
                        _.each(data.data.response, (system: { id; name; }) => {
                            results.push({
                                //the id of the system is the id, that is selected
                                id: system.id,
                                //but the name of the system is the label for the select2
                                text: system.name,
                                //saving the system for later use
                                system: system
                            });

                        });

                        return { results: results };
                    }
                }
            };

            var itSystemBaseUrl: string;
            if (user.isGlobalAdmin) {
                // global admin should see all it systems everywhere with all levels of access
                itSystemBaseUrl = "/odata/ItSystems";
            } else {
                // everyone else are limited to within organizationnal context
                itSystemBaseUrl = `/odata/Organizations(${user.currentOrganizationId})/ItSystems`;
            }
            var itSystemUrl = itSystemBaseUrl + "?$expand=BusinessType,BelongsTo,TaskRefs,Parent,Organization,Usages($expand=Organization),LastChangedByUser,Reference";
            // catalog grid
            this.mainGridOptions = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: itSystemUrl,
                            dataType: "json"
                        },
                        parameterMap: (options, type) => {
                            var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                            if (parameterMap.$filter) {
                                // replaces 'Kitos.AccessModifier0' with Kitos.AccessModifier'0'
                                parameterMap.$filter = parameterMap.$filter.replace(/('Kitos\.AccessModifier([0-9])')/,
                                    "Kitos.AccessModifier'$2'");
                                // replaces "startswith(TaskKey,'11')" with "TaskRefs/any(c: startswith(c/TaskKey),'11')"
                                parameterMap.$filter =
                                    parameterMap.$filter.replace(/(\w+\()(TaskKey.*\))/, "TaskRefs/any(c: $1c/$2)");
                                // replaces "startswith(TaskName,'11')" with "TaskRefs/any(c: startswith(c/Description),'11')"
                                parameterMap.$filter = parameterMap.$filter.replace(/(\w+\()TaskName(.*\))/,
                                    "TaskRefs/any(c: $1c/Description$2)");
                                // replaces "contains(Uuid,'11')" with "contains(CAST(Uuid, 'Edm.String'),'11')"
                                parameterMap.$filter = parameterMap.$filter.replace(/contains\(Uuid,/,
                                    "contains(CAST(Uuid, 'Edm.String'),");

                                //if (user.isGlobalAdmin) {
                                //    parameterMap.$filter = parameterMap.$filter + "and Disabled eq false";
                                //}
                            }

                            return parameterMap;
                        }
                    },
                    sort: {
                        field: "Name",
                        dir: "asc"
                    },
                    schema: {
                        model: {
                            fields: {
                                LastChanged: { type: "date" },
                                Disabled: { type: "boolean" }
                            }
                        },
                        parse: response => {
                            // iterrate each usage
                            this._.forEach(response.value,
                                system => {
                                    if (!system.Reference) {
                                        system.Reference = { Title: "", ExternalReferenceId: "" };
                                    }
                                    if (!system.Parent) {
                                        system.Parent = { Name: "" };
                                    }
                                    if (!system.BusinessType) {
                                        system.BusinessType = { Name: "" };
                                    }
                                    if (!system.BelongsTo) {
                                        system.BelongsTo = { Name: "" };
                                    }
                                    if (!system.Usages) {
                                        system.Usages = [];
                                    }
                                    if (!system.Organization) {
                                        system.Organization = { Name: "" };
                                    }
                                });
                            return response;
                        }
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true
                },
                toolbar: [
                    {
                        name: "createITSystem",
                        text: "Opret IT System",
                        template:
                            "<button ng-click='systemCatalogVm.createITSystem()' data-element-type='createITSystemButton' class='btn btn-success pull-right' data-ng-disabled=\"!systemCatalogVm.canCreate\">#: text #</button>"
                    },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template:
                            "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='systemCatalogVm.clearOptions()' data-element-type='resetFilterButton'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template:
                            '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="systemCatalogVm.saveGridProfile()">#: text #</button>'
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template:
                            '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="systemCatalogVm.loadGridProfile()" data-ng-disabled="!systemCatalogVm.doesGridProfileExist()">#: text #</button>'
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template:
                            "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='systemCatalogVm.clearGridProfile()' data-ng-disabled='!systemCatalogVm.doesGridProfileExist()'>#: text #</button>"
                    }
                ],
                excel: {
                    fileName: "IT System Katalog.xlsx",
                    filterable: true,
                    allPages: true
                },
                pageable: {
                    refresh: true,
                    pageSizes: [10, 25, 50, 100, 200, "all"],
                    buttonCount: 5
                },
                sortable: {
                    mode: "single"
                },
                reorderable: true,
                resizable: true,
                filterable: {
                    mode: "row"
                },
                groupable: false,
                columnMenu: true,
                height: window.innerHeight - 200,
                dataBound: this.saveGridOptions,
                columnResize: this.saveGridOptions,
                columnHide: this.saveGridOptions,
                columnShow: this.saveGridOptions,
                columnReorder: this.saveGridOptions,
                excelExport: this.exportToExcel,
                page: this.onPaging,
                columns: [
                    {
                        field: "Usages", title: "Anvendes", width: 40,
                        persistId: "command", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            // true if system is being used by system within current context, else false
                            var systemHasUsages = this._.find(dataItem.Usages, (d: any) => (d.OrganizationId == this.user.currentOrganizationId));

                            if (systemHasUsages)
                                return `<div class="text-center"><button ng-disabled="!systemCatalogVm.allowToggleUsage" type="button" data-element-type="toggleActivatingSystem" class="btn btn-link" data-ng-click="systemCatalogVm.removeUsage(${dataItem.Id})"><span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span></button></div>`;

                            if (dataItem.Disabled)
                                return `<div class="text-center"><button type="button" data-element-type="toggleActivatingSystem" class="btn btn-link" disabled><span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span></button></div>`;

                            return `<div class="text-center"><button ng-disabled="!systemCatalogVm.allowToggleUsage" type="button" data-element-type="toggleActivatingSystem" class="btn btn-link " data-ng-click="systemCatalogVm.enableUsage(dataItem)"><span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span></button></div>`;
                        },
                        attributes: {
                            "data-element-type": "catalogUsageObject"
                        },
                        headerAttributes: {
                            "data-element-type": "catalogUsageHeader"
                        },
                        excelTemplate: dataItem => {
                            // true if system is being used by system within current context, else false
                            var systemHasUsages = dataItem ? this._.find(dataItem.Usages, (d: any) => (d.OrganizationId == this.user.currentOrganizationId)) : false;
                            return systemHasUsages ? "Anvendt" : "Ikke anvendt";
                        },
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "Parent.Name", title: "Overordnet IT System", width: 150,
                        persistId: "parentname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.Parent ? dataItem.Parent.Name : "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "PreviousName", title: "Tidligere Systemnavn", width: 285,
                        persistId: "previousname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.PreviousName != null ? dataItem.PreviousName : "",
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Name", title: "It System", width: 285,
                        persistId: "name", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.Disabled)
                                return `<a data-ui-sref='it-system.edit.main({id: ${dataItem.Id}})'>${dataItem.Name} (Slettes) </a>`;
                            else
                                return `<a data-ui-sref='it-system.edit.main({id: ${dataItem.Id}})'>${dataItem.Name}</a>`;
                        },
                        attributes: {
                            "data-element-type": "catalogNameObject"
                        },
                        headerAttributes: {
                            "data-element-type": "catalogNameHeader"
                        },
                        excelTemplate: dataItem => {
                            if (dataItem && dataItem.Name) {
                                if (dataItem.Disabled)
                                    return dataItem.Name + " (Slettes)";
                                else
                                    return dataItem.Name;
                            } else {
                                return "";
                            }
                        },
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Disabled", title: "Slettes", width: 120,
                        persistId: "Disabled", // DON'T YOU DARE RENAME!
                        template: dataItem => { return dataItem.Disabled ? "Ja" : "Nej"; },
                        hidden: false,
                        filterable: {
                            cell: {
                                template: function (args) {
                                    args.element.kendoDropDownList({
                                        dataSource: [{ type: "Ja", value: true }, { type: "Nej", value: false }],
                                        dataTextField: "type",
                                        dataValueField: "value",
                                        valuePrimitive: true
                                    });
                                },
                                showOperators: false
                            }
                        }
                    },
                    {
                        field: "AccessModifier", title: "Synlighed", width: 120,
                        persistId: "accessmod", // DON'T YOU DARE RENAME!
                        template: `<display-access-modifier value="dataItem.AccessModifier"></display-access-modifier>`,
                        excelTemplate: dataItem => dataItem && dataItem.AccessModifier.toString() || "",
                        hidden: true,
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: this.accessModFilter
                            }
                        }
                    },
                    {
                        field: "BusinessType.Name", title: "Forretningstype", width: 150,
                        persistId: "busitype", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.BusinessType ? dataItem.BusinessType.Name : "",
                        attributes: { "class": "might-overflow" },
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "BelongsTo.Name", title: "Rettighedshaver", width: 210,
                        persistId: "belongsto", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.BelongsTo ? dataItem.BelongsTo.Name : "",
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "TaskKey", title: "KLE ID", width: 150,
                        persistId: "taskkey", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.TaskRefs.length > 0 ? this._.map(dataItem.TaskRefs, "TaskKey").join(", ") : "",
                        attributes: { "class": "might-overflow" },
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "startswith"
                            }
                        },
                        sortable: false
                    },
                    {
                        field: "TaskName", title: "KLE Navn", width: 155,
                        persistId: "taskname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.TaskRefs.length > 0 ? this._.map(dataItem.TaskRefs, "Description").join(", ") : "",
                        attributes: { "class": "might-overflow" },
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "startswith"
                            }
                        },
                        sortable: false
                    },
                    {
                        field: "Usages.length", title: "IT System: Anvendes af", width: 95,
                        persistId: "usages", // DON'T YOU DARE RENAME!
                        template: dataItem => this.showUsagesAsNumberOrNothing(dataItem),
                        excelTemplate: dataItem => dataItem && dataItem.Usages && dataItem.Usages.length.toString() || "",
                        filterable: false,
                        sortable: false,
                        attributes: {
                            "data-element-type": "usedByNameObject"
                        },
                        headerAttributes: {
                            "data-element-type": "usedByNameHeader"
                        },
                    },
                    {
                        field: "Organization.Name", title: "Oprettet af: Organisation", width: 150,
                        persistId: "orgname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.Organization ? dataItem.Organization.Name : "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "LastChangedByUser.Name", title: "Sidst redigeret: Bruger", width: 150,
                        persistId: "lastchangedname", // DON'T YOU DARE RENAME!
                        template: dataItem => `${dataItem.LastChangedByUser.Name} ${dataItem.LastChangedByUser.LastName}`,
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "LastChanged", title: "Sidst redigeret: Dato", format: "{0:dd-MM-yyyy}", width: 130,
                        persistId: "lastchangeddate", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            // handles null cases
                            if (!dataItem || !dataItem.LastChanged || this.moment(dataItem.LastChanged).format("DD-MM-YYYY") === "01-01-0001") {
                                return "";
                            }
                            return this.moment(dataItem.LastChanged).format("DD-MM-YYYY");
                        },
                        excelTemplate: dataItem => {
                            // handles null cases
                            if (!dataItem || !dataItem.LastChanged || this.moment(dataItem.LastChanged).format("DD-MM-YYYY") === "01-01-0001") {
                                return "";
                            }
                            return this.moment(dataItem.LastChanged).format("DD-MM-YYYY");
                        },
                        attributes: { "class": "text-center" },
                        filterable: {
                            cell: {
                                showOperators: false,
                                operator: "gte"
                            }
                        }
                    },
                    {
                        field: "Reference.Title",
                        title: "Reference",
                        width: 150,
                        persistId: "ReferenceId", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            var reference = dataItem.Reference;
                            if (reference != null) {
                                var url = reference.URL;
                                if (Utility.Validation.validateUrl(url)) {
                                    return "<a target=\"_blank\" style=\"float:left;\" href=\"" + url + "\">" + reference.Title + "</a>";
                                }
                                else {
                                    return reference.Title;
                                }
                            }
                            return "";
                        },
                        excelTemplate: dataItem => {
                            return Helpers.ExcelExportHelper.renderReferenceUrl(dataItem.Reference);
                        },
                        attributes: { "class": "text-left" },
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Reference.ExternalReferenceId", title: "Mappe ref", width: 150,
                        persistId: "folderref", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            var reference = dataItem.Reference;
                            if (reference != null) {
                                if (Utility.Validation.validateUrl(reference.ExternalReferenceId)) {
                                    return "<a target=\"_blank\" style=\"float:left;\" href=\"" +
                                        reference.ExternalReferenceId +
                                        "\">" +
                                        reference.Title +
                                        "</a>";
                                } else {
                                    return reference.Title;
                                }
                            }
                            return "";
                        },
                        excelTemplate: dataItem => {
                            return Helpers.ExcelExportHelper.renderExternalReferenceId(dataItem.Reference);
                        },
                        attributes: { "class": "text-center" },
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Uuid", title: "UUID", width: 150,
                        persistId: "uuid", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem.Uuid,
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    }
                ]
            };
            function customFilter(args) {
                args.element.kendoAutoComplete({
                    noDataTemplate: ""
                });
            }
        }

        public createITSystem() {
            var self = this;

            var modalInstance = this.$uibModal.open({
                // fade in instead of slide from top, fixes strange cursor placement in IE
                // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                windowClass: "modal fade in",
                templateUrl: "app/components/it-system/it-system-modal-create.view.html",
                controller: ["$scope", "$uibModalInstance", function ($scope, $modalInstance) {
                    $scope.formData = {};
                    $scope.type = "IT System";
                    $scope.checkAvailableUrl = "api/itSystem/";

                    $scope.saveAndProceed = function () {
                        var payload = {
                            name: $scope.formData.name,
                            belongsToId: self.user.currentOrganizationId,
                            organizationId: self.user.currentOrganizationId,
                            taskRefIds: []
                        };

                        var msg = self.notify.addInfoMessage("Opretter system...", false);
                        self.$http.post("api/itsystem", payload)
                            .success(function (result: any) {
                                msg.toSuccessMessage("Et nyt system er oprettet!");
                                var systemId = result.response.id;
                                $modalInstance.close(systemId);
                                if (systemId) {
                                    self.$state.go("it-system.edit.main", { id: systemId });
                                }
                            }).error(function () {
                                msg.toErrorMessage("Fejl! Kunne ikke oprette et nyt system!");
                            });
                    };

                    $scope.save = function () {
                        var payload = {
                            name: $scope.formData.name,
                            belongsToId: self.user.currentOrganizationId,
                            organizationId: self.user.currentOrganizationId,
                            taskRefIds: []
                        };

                        var msg = self.notify.addInfoMessage("Opretter system...", false);
                        self.$http.post("api/itsystem", payload)
                            .success(function (result: any) {
                                msg.toSuccessMessage("Et nyt system er oprettet!");
                                var systemId = result.response.id;
                                $modalInstance.close(systemId);
                                if (systemId) {
                                    self.$state.reload();
                                }

                            }).error(function () {
                                msg.toErrorMessage("Fejl! Kunne ikke oprette et nyt system!");
                            });
                    };
                }]
            });

        };

        // saves grid state to localStorage
        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.mainGrid);
        }

        // Resets the scrollbar position
        private onPaging = () => {
            Utility.KendoGrid.KendoGridScrollbarHelper.resetScrollbarPosition(this.mainGrid);
        }

        // loads kendo grid options from localstorage
        private loadGridOptions() {
            this.mainGrid.options.toolbar.push({ name: "excel", text: "Eksportér til Excel", className: "pull-right" });
            this.gridState.loadGridOptions(this.mainGrid);
        }

        public saveGridProfile = () => {
            this.gridState.saveGridProfile(this.mainGrid);
            this.notify.addSuccessMessage("Filtre og sortering gemt");
        }

        public loadGridProfile() {
            this.gridState.loadGridProfile(this.mainGrid);
            this.mainGrid.dataSource.read();
            this.notify.addSuccessMessage("Anvender gemte filtre og sortering");
        }

        public clearGridProfile() {
            this.gridState.removeProfile();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Filtre og sortering slettet");
            this.reload();
        };

        public doesGridProfileExist = () => {
            return this.gridState.doesGridProfileExist();
        };

        public clearOptions() {
            this.gridState.removeProfile();
            this.gridState.removeLocal();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Sortering, filtering og kolonnevisning, -bredde og –rækkefølge nulstillet");
            this.reload();
        };

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        public isValidUrl(Url) {
            var regexp = /(http || https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
            return regexp.test(Url.toLowerCase());
        };

        public showUsageDetails = (systemId, systemName) => {
            this.resetMigrationFlow();
            this.oldItSystemName = systemName;
            this.oldItSystemId = systemId;
            this.usageGrid.dataSource.fetch(() => {
                this.modal.setOptions({
                    close: (_) => true,
                    resizable: false,
                    title: `Anvendelse af ${systemName}`
                });
                this.modal.center().open();
            });
            this.modal.center().open();
        }

        private resetMigrationFlow = () => {
            this.newItSystemObject = null;
            this.municipalityId = null;
            this.municipalityName = null;
            this.oldItSystemUsageId = null;
            this.oldItSystemName = null;
            this.oldItSystemId = null;
            this.resetItSystemSelection();
        }

        private resetItSystemSelection = () => {
            this.getItSystemSelectionControl().select2("data", null);
        }

        private getItSystemSelection = () => {
            return this.getItSystemSelectionControl().select2("data");
        }

        private getSelectionDropdown = () => {
            return this.convertToJQueryLocator("#select2-drop");
        }
        private getItSystemSelectionControl = () => {
            return this.convertToJQueryLocator("#new-system-usage");
        }

        private closeSelectionDropdown = () => {
            var dropdown = this.getSelectionDropdown();
            if (dropdown != null) {
                dropdown.select2("close");
            }
        }

        private convertToJQueryLocator = (name: string) => {
            return this.$(name) as any;
        }

        public beginItSystemMigration = (municipalityId: number, municipalityName: string, usageId: number) => {
            this.modal.close();
            this.municipalityId = municipalityId;
            this.municipalityName = municipalityName;
            this.oldItSystemUsageId = usageId;
            this.modalMigration.setOptions({
                close: (e) => {
                    this.closeSelectionDropdown();
                },
                resizable: false,
                title: `Flyt af relation for ${this.municipalityName}`
            });
            this.modalMigration.center().open();
        }

        public onNewTargetSystemSelected = () => {
            this.newItSystemObject = this.getItSystemSelection();
            if (this.newItSystemObject != null) {
                this.getMigrationReport(this.oldItSystemUsageId, this.newItSystemObject.id)
                    .success(dto => {
                        this.migrationReportDTO = dto.response;

                        this.modalMigrationConsequence.setOptions({
                            close: (_) => true,
                            resizable: false,
                            title: `Flytning af it-system `,
                        });
                        this.modalMigration.close();
                        this.modalMigrationConsequence.center().open();
                    })
                    .error(() => {
                        this.notify.addErrorMessage("Kunne ikke oprette konsekvens-rapport for flytningen");
                    });
            }
        }

        private getMigrationReport: any = (usageId, toSystemId) => {
            var url = `api/v1/ItSystemUsageMigration?usageId=${usageId}&toSystemId=${toSystemId}`;

            return this.$http({ method: "GET", url: url, });
        }

        private executeMigration: any = (usageId, toSystemId) => {
            var url = `api/v1/ItSystemUsageMigration?usageId=${usageId}&toSystemId=${toSystemId}`;

            return this.$http({ method: "POST", url: url, });
        }

        private showUsagesAsNumberOrNothing(dataItem): string {
            if (dataItem.Usages.length > 0) {
                return `<a class="col-xs-7 text-center" data-element-type="usagesLinkText" data-ng-click="systemCatalogVm.showUsageDetails(${
                    dataItem.Id},'${this.$sce.getTrustedHtml(dataItem.Name)}')">${dataItem.Usages.length
                    }</a>`;
            }
            else {
                return ``;
            }
        };

        public performMigration = () => {
            if (this.oldItSystemName != null || this.newItSystemObject != null) {
                this.executeMigration(this.oldItSystemUsageId, this.newItSystemObject.system.id)
                    .success(() => {
                        this.modalMigrationConsequence.close();
                        this.mainGrid.dataSource.fetch();
                        this.notify.addSuccessMessage("Flytning af system anvendelse lykkedes");
                    })
                    .error(() => {
                        this.notify.addErrorMessage("Flytning af system anvendelse fejlede");
                    });
            }
        }

        public copyToClipBoard() {
            window.getSelection().selectAllChildren(document.getElementById("copyPasteConsequence"));
            document.execCommand("Copy");
            window.getSelection().removeAllRanges();
            this.notify.addSuccessMessage("Flytning rapport er blevet kopieret");

        }

        public cancelMigration() {
            this.modalMigrationConsequence.close();
        }

        public usageDetailsGrid: kendo.ui.GridOptions = {
            dataSource: {
                transport:
                {
                    read: {
                        url: () => `/api/v1/ItSystem/${this.oldItSystemId}/usingOrganizations`,
                        dataType: "json"
                    }
                },
                schema: {
                    data: (response) => this._.orderBy(response.response, (dto: any) => dto.organization.name.toLowerCase())
                },
                serverPaging: true,
                serverSorting: true,
                serverFiltering: true,
            },
            autoBind: false,
            columns: [
                {
                    field: "organization.name", title: "Organisation",
                    template: dataItem => {
                        var orgId = dataItem.organization.id;
                        var orgName = dataItem.organization.name;
                        var usageId = dataItem.systemUsageId;
                        if (this.canMigrate) {
                            return `<p data-element-type='MigrationMoveOrgName'>${orgName}</p> <button ng-click='systemCatalogVm.beginItSystemMigration(${orgId}, "${orgName}", ${usageId})' data-element-type='migrateItSystem' class='k-button pull-right'>Flyt</button>`;
                        } else {
                            return `<p data-element-type='MigrationMoveOrgName'>${orgName}</p>`;
                        }
                    },
                }
            ],
        };

        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.ItSystem.IItSystem>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.mainGrid);
        }

        // adds usage at selected system within current context
        public enableUsage(dataItem) {
            this.addUsage(dataItem).then(() => this.mainGrid.dataSource.fetch());
        }

        // removes usage at selected system within current context
        public removeUsage(dataItem) {
            var sure = confirm("Er du sikker på at du vil fjerne den lokale anvendelse af systemet? Dette sletter ikke systemet, men vil slette alle lokale detaljer vedrørende anvendelsen.");
            if (sure)
                this.deleteUsage(dataItem).then(() => this.mainGrid.dataSource.fetch());
        }

        // adds system to usage within the current context
        private addUsage(dataItem) {
            return this.$http.post("api/itSystemUsage", {
                itSystemId: dataItem.Id,
                organizationId: this.user.currentOrganizationId
            })
                .success(() => this.notify.addSuccessMessage("Systemet er taget i anvendelse"))
                .error(() => this.notify.addErrorMessage("Systemet kunne ikke tages i anvendelse!"));
        }

        // removes system from usage within the current context
        private deleteUsage(systemId) {
            var url = `api/itSystemUsage?itSystemId=${systemId}&organizationId=${this.user.currentOrganizationId}`;

            return this.$http.delete(url)
                .success(() => this.notify.addSuccessMessage("Anvendelse af systemet er fjernet"))
                .error(() => this.notify.addErrorMessage("Anvendelse af systemet kunne ikke fjernes!"));
        }

        private accessModFilter = (args) => {
            var self = this;
            var gridDataSource = args.dataSource;

            function setSelected() {
                var kendoElem = this;
                var currentFilter = gridDataSource.filter();
                var filterObj = self._.findKeyDeep(currentFilter, { field: "AccessModifier" });

                switch (filterObj.value) {
                    case "Kitos.AccessModifier0":
                        kendoElem.select(1);
                        break;
                    case "Kitos.AccessModifier1":
                        kendoElem.select(2);
                        break;
                    default:
                        kendoElem.select(0); // select placeholder
                }
            }

            function applyFilter() {
                var kendoElem = this;
                // can't use args.dataSource directly,
                // if we do then the view doesn't update.
                // So have to go through $scope - sadly :(
                var dataSource = self.mainGrid.dataSource;
                var selectedValue = kendoElem.value();
                var field = "AccessModifier";
                var currentFilter = dataSource.filter();
                // remove old value first
                var newFilter = self._.removeFiltersForField(currentFilter, field);

                if (selectedValue) {
                    newFilter = self._.addFilter(newFilter, field, "eq", selectedValue, "and");
                }
                dataSource.filter(newFilter);
            }



            // http://dojo.telerik.com/ODuDe/5
            args.element.removeAttr("data-bind");
            args.element.kendoDropDownList({
                dataSource: [
                    { value: "Kitos.AccessModifier0", text: "Lokal" },
                    { value: "Kitos.AccessModifier1", text: "Offentlig" }
                ],
                dataTextField: "text",
                dataValueField: "value",
                optionLabel: "Vælg filter...",
                dataBound: setSelected,
                change: applyFilter
            });
        }

    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-system.catalog", {
                    url: "/catalog",
                    templateUrl: "app/components/it-system/it-system-catalog.view.html",
                    controller: CatalogController,
                    controllerAs: "systemCatalogVm",
                    resolve: {
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        userAccessRights: ["authorizationServiceFactory", (authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory) =>
                            authorizationServiceFactory
                                .createSystemAuthorization()
                                .getOverviewAuthorization()
                        ],
                        systemUsageUserAccessRights: ["authorizationServiceFactory", (authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory) =>
                            authorizationServiceFactory
                                .createSystemUsageAuthorization()
                                .getOverviewAuthorization()
                        ],
                        userMigrationRights: ["$http", $http => $http.get("api/v1/ItSystemUsageMigration/Accessibility")
                            .then(result => result.data.response)
                        ],
                    }
                });

            }
        ]);
}

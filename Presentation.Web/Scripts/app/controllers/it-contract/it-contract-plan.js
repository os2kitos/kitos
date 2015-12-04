(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.plan', {
            url: '/plan',
            templateUrl: 'partials/it-contract/it-contract-plan.html',
            controller: 'contract.PlanCtrl',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }],
                itContractRoles: [
                    '$http', function ($http) {
                        return $http.get("/odata/ItContractRoles").then(function (result) {
                            return result.data.value;
                        });
                    }
                ],
                orgUnits: [
                    '$http', 'user', '_', function ($http, user, _) {
                        return $http.get("/odata/Organizations(" + user.currentOrganizationId + ")/OrganizationUnits").then(function (result) {
                            return _.addHierarchyLevelOnFlatAndSort(result.data.value, "Id", "ParentId");
                        });
                    }
                ],
            }
        });
    }]);

    app.controller('contract.PlanCtrl', ['$scope', '$http', '$state', '_', 'moment', 'notify', 'user', 'gridStateService', 'itContractRoles', 'orgUnits',
            function ($scope, $http, $state, _, moment, notify, user, gridStateService, itContractRoles, orgUnits) {
                var storageKey = "it-contract-plan-options";
                var orgUnitStorageKey = "it-contract-plan-orgunit";
                var gridState = gridStateService.getService(storageKey);

                // saves grid state to localStorage
                function saveGridOptions() {
                    gridState.saveGridOptions($scope.mainGrid);
                }

                // replaces "anything({roleName},'foo')" with "Rights/any(c: anything(c/User/Name,'foo') and c/RoleId eq {roleId})"
                function fixRoleFilter(filterUrl, roleName, roleId) {
                    var pattern = new RegExp("(\\w+\\()" + roleName + "(.*?\\))", "i");
                    return filterUrl.replace(pattern, "Rights/any(c: $1c/User/Name$2 and c/RoleId eq " + roleId + ")");
                }

                function fixProcurmentFilter(filterUrl) {
                    return filterUrl.replace(/ProcurementPlanYear/i, "cast($&, Edm.String)");
                }

                // loads kendo grid options from localstorage
                function loadGridOptions() {
                    var selectedOrgUnitId = sessionStorage.getItem(orgUnitStorageKey);
                    var selectedOrgUnit = _.find(orgUnits, function (orgUnit) {
                        return orgUnit.Id == selectedOrgUnitId;
                    });

                    var filter = undefined;
                    // if selected is a root then no need to filter as it should display everything anyway
                    if (selectedOrgUnit && selectedOrgUnit.$level != 0) {
                        filter = getFilterWithOrgUnit({}, selectedOrgUnitId, selectedOrgUnit.childIds);
                    }

                    gridState.loadGridOptions($scope.mainGrid, filter);
                }

                $scope.saveGridProfile = function () {
                    // the stored org unit id must be the current
                    var currentOrgUnitId = sessionStorage.getItem(orgUnitStorageKey);
                    localStorage.setItem(orgUnitStorageKey + "-profile", currentOrgUnitId);

                    gridState.saveGridProfile($scope.mainGrid);
                    notify.addSuccessMessage("Filtre og sortering gemt");
                };

                $scope.loadGridProfile = function () {
                    gridState.loadGridProfile($scope.mainGrid);

                    var orgUnitId = localStorage.getItem(orgUnitStorageKey + "-profile");
                    // update session
                    sessionStorage.setItem(orgUnitStorageKey, orgUnitId);
                    // find the org unit filter row section
                    var orgUnitFilterRow = $(".k-filter-row [data-field='ResponsibleUsage.OrganizationUnit.Name']");
                    // find the access modifier kendo widget
                    var orgUnitFilterWidget = orgUnitFilterRow.find("input").data("kendoDropDownList");
                    orgUnitFilterWidget.select(function (dataItem) {
                        return dataItem.Id == orgUnitId;
                    });

                    $scope.mainGrid.dataSource.read();
                    notify.addSuccessMessage("Anvender gemte filtre og sortering");
                };

                $scope.clearGridProfile = function () {
                    sessionStorage.removeItem(orgUnitStorageKey);
                    gridState.removeProfile();
                    gridState.removeSession();
                    notify.addSuccessMessage("Filtre og sortering slettet");
                    reload();
                };

                $scope.doesGridProfileExist = function () {
                    return gridState.doesGridProfileExist();
                };

                // clears grid filters by removing the localStorageItem and reloading the page
                $scope.clearOptions = function () {
                    localStorage.removeItem(orgUnitStorageKey + "-profile");
                    sessionStorage.removeItem(orgUnitStorageKey);
                    gridState.removeProfile();
                    gridState.removeLocal();
                    gridState.removeSession();
                    notify.addSuccessMessage("Sortering, filtering og kolonnevisning, -bredde og –rækkefølge nulstillet");
                    // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
                    reload();
                };

                function reload() {
                    $state.go('.', null, { reload: true });
                }

                $scope.$on("kendoWidgetCreated", function (event, widget) {
                    // the event is emitted for every widget; if we have multiple
                    // widgets in this controller, we need to check that the event
                    // is for the one we're interested in.
                    if (widget === $scope.mainGrid) {
                        loadGridOptions();
                        $scope.mainGrid.dataSource.read();
                    }
                });

                var mainGridOptions = {
                    autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                    dataSource: {
                        type: "odata-v4",
                        transport: {
                            read: {
                                url: "/odata/Organizations(" + user.currentOrganizationId + ")/ItContracts?$expand=Parent,ResponsibleOrganizationUnit,Rights($expand=User,Role),Supplier,ContractTemplate,ContractType,PurchaseForm,OptionExtend,TerminationDeadline,ProcurementStrategy,Advices",
                                dataType: "json"
                            },
                            parameterMap: function (options, type) {
                                // get kendo to map parameters to an odata url
                                var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                                if (parameterMap.$orderBy) {
                                    if (parameterMap.$orderBy === "ProcurementPlanYear") {
                                        parameterMap.$orderBy = "ProcurementPlanYear,ProcurementPlanHalf";
                                    }
                                    if (parameterMap.$orderBy === "ProcurementPlanYear desc") {
                                        parameterMap.$orderBy = "ProcurementPlanYear desc,ProcurementPlanHalf desc";
                                    }
                                }

                                if (parameterMap.$filter) {
                                    _.forEach(itContractRoles, function (role) {
                                        parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "role" + role.Id, role.Id);
                                    });

                                    parameterMap.$filter = fixProcurmentFilter(parameterMap.$filter);
                                }

                                return parameterMap;
                            }
                        },
                        sort: {
                            field: "Name",
                            dir: "asc"
                        },
                        pageSize: 100,
                        serverPaging: true,
                        serverSorting: true,
                        serverFiltering: true,
                        schema: {
                            model: {
                                fields: {
                                    OperationRemunerationBegun: { type: "date" },
                                    LastChanged: { type: "date" },
                                    Concluded: { type: "date" },
                                    ExpirationDate: { type: "date" },
                                    IrrevocableTo: { type: "date" },
                                    Terminated: { type: "date" },
                                    Duration: { type: "number" },
                                }
                            },
                            parse: function (response) {
                                // iterrate each contract
                                _.forEach(response.value, function (contract) {
                                    // HACK to flattens the Rights on usage so they can be displayed as single columns
                                    contract.roles = [];
                                    // iterrate each right
                                    _.forEach(contract.Rights, function (right) {
                                        // init an role array to hold users assigned to this role
                                        if (!contract.roles[right.RoleId])
                                            contract.roles[right.RoleId] = [];

                                        // push username to the role array
                                        contract.roles[right.RoleId].push([right.User.Name, right.User.LastName].join(" "));
                                    });
                                });
                                return response;
                            }
                        }
                    },
                    toolbar: [
                        { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
                        {
                            name: "clearFilter",
                            text: "Nulstil",
                            template: "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='clearOptions()'>#: text #</button>"
                        },
                        {
                            name: "saveFilter",
                            text: "Gem filter",
                            template: '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="saveGridProfile()">#: text #</button>'
                        },
                        {
                            name: "useFilter",
                            text: "Anvend filter",
                            template: '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="loadGridProfile()" data-ng-disabled="!doesGridProfileExist()">#: text #</button>'
                        },
                        {
                            name: "deleteFilter",
                            text: "Slet filter",
                            template: "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='clearGridProfile()' data-ng-disabled='!doesGridProfileExist()'>#: text #</button>"
                        },
                        {
                            template: kendo.template($("#role-selector").html())
                        }
                    ],
                    excel: {
                        fileName: "IT Kontrakt Overblik.xlsx",
                        filterable: true,
                        allPages: true
                    },
                    pageable: {
                        refresh: true,
                        pageSizes: [10, 25, 50, 100, 200],
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
                    columnMenu: {
                        filterable: false
                    },
                    dataBound: saveGridOptions,
                    columnResize: saveGridOptions,
                    columnHide: saveGridOptions,
                    columnShow: saveGridOptions,
                    columnReorder: saveGridOptions,
                    excelExport: exportToExcel,
                    error: function (e) {
                        console.log(e);
                    },
                    columns: [
                        {
                            field: "", title: "Aktiv", width: 45,
                            persistId: "active", // DON'T YOU DARE RENAME!
                            template: activeStatusTemplate,
                            attributes: { "class": "text-center" },
                            sortable: false,
                            filterable: false,
                        },
                        {
                            field: "ItContractId", title: "KontraktID", width: 150,
                            persistId: "contractid", // DON'T YOU DARE RENAME!
                            hidden: true,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "Parent.Name", title: "Overordnet kontrakt", width: 150,
                            persistId: "parentname", // DON'T YOU DARE RENAME!
                            template: "#= Parent ? '<a data-ui-sref=\"it-contract.edit.systems({id:' + Parent.Id + '})\">' + Parent.Name + '</a>' : '' #",
                            hidden: true,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains"
                                }
                            }
                        },
                        {
                            field: "Name", title: "IT Kontrakt", width: 190,
                            persistId: "name", // DON'T YOU DARE RENAME!
                            template: "<a data-ui-sref='it-contract.edit.systems({id: #: Id #})'>#: Name #</a>",
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains"
                                }
                            }
                        },
                        {
                            field: "ResponsibleOrganizationUnit.Name", title: "Ansv. organisationsenhed", width: 245,
                            persistId: "orgunit", // DON'T YOU DARE RENAME!
                            template: "#: ResponsibleOrganizationUnit ? ResponsibleOrganizationUnit.Name : '' #",
                            hidden: true,
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    template: orgUnitDropDownList
                                }
                            }
                        },
                        //{
                        //    field: "AssociatedSystemUsages", title: "IT System", width: 150,
                        //    persistId: "itsys", // DON'T YOU DARE RENAME!
                        //    template: "#: AssociatedSystemUsages.length > 0 ? _.first(AssociatedSystemUsages).ItSystemUsage.ItSystem.Name : '' #" +
                        //        "#= AssociatedSystemUsages.length > 1 ? ' (' + AssociatedSystemUsages.length + ')' : '' #",
                        //    filterable: {
                        //        cell: {
                        //            dataSource: [],
                        //            showOperators: false,
                        //            operator: "contains"
                        //        }
                        //    }
                        //},
                        {
                            field: "Supplier.Name", title: "Leverandør", width: 140,
                            persistId: "suppliername", // DON'T YOU DARE RENAME!
                            template: "#: Supplier ? Supplier.Name : '' #",
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains"
                                }
                            }
                        },
                        {
                            field: "Esdh", title: "ESDH ref", width: 150,
                            persistId: "esdh", // DON'T YOU DARE RENAME!
                            hidden: true,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "Folder", title: "Mappe ref", width: 150,
                            persistId: "folderref", // DON'T YOU DARE RENAME!
                            template: folderTemplate,
                            attributes: { "class": "text-center" },
                            hidden: true,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ContractType.Name", title: "Kontrakttype", width: 120,
                            persistId: "contracttype", // DON'T YOU DARE RENAME!
                            template: "#: ContractType ? ContractType.Name : '' #",
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ContractTemplate.Name", title: "Kontraktskabelon", width: 150,
                            persistId: "contracttmpl", // DON'T YOU DARE RENAME!
                            template: "#: ContractTemplate ? ContractTemplate.Name : '' #",
                            hidden: true,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "PurchaseForm.Name", title: "Indkøbsform", width: 120,
                            persistId: "purchaseform", // DON'T YOU DARE RENAME!
                            template: "#: PurchaseForm ? PurchaseForm.Name : '' #",
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "Concluded", title: "Indgået", format: "{0:dd-MM-yyyy}", width: 90,
                            persistId: "concluded", // DON'T YOU DARE RENAME!
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    operator: "gte"
                                }
                            }
                        },
                        {
                            field: "Duration", title: "Varighed", width: 95,
                            persistId: "duration", // DON'T YOU DARE RENAME!
                            template: "#: Duration ? Duration + ' md' : '' #",
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "eq",
                                }
                            }
                        },
                        {
                            field: "ExpirationDate", title: "Udløbsdato", format: "{0:dd-MM-yyyy}", width: 90,
                            headerTemplate: '<div style="word-wrap: break-word;">Udløbsdato</div>',
                            persistId: "expirationDate", // DON'T YOU DARE RENAME!
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    operator: "gte"
                                }
                            }
                        },
                        {
                            field: "OptionExtend", title: "Option", width: 150,
                            persistId: "option", // DON'T YOU DARE RENAME!
                            hidden: true,
                            template: "#: OptionExtend ? OptionExtend.Name : '' # #: OptionExtend ? '(' + ExtendMultiplier + ')' : '' #",
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "TerminationDeadline.Name", title: "Opsigelse", width: 100,
                            persistId: "terminationDeadline", // DON'T YOU DARE RENAME!
                            template: "#: TerminationDeadline ? TerminationDeadline.Name + ' md' : '' #",
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    operator: "gte"
                                }
                            }
                        },
                        {
                            field: "IrrevocableTo", title: "Uopsigelig til", format: "{0:dd-MM-yyyy}", width: 150,
                            headerTemplate: '<div style="word-wrap: break-word;">Uopsigelig til</div>',
                            persistId: "irrevocableTo", // DON'T YOU DARE RENAME!
                            hidden: true,
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    operator: "gte"
                                }
                            }
                        },
                        {
                            field: "Terminated", title: "Opsagt", format: "{0:dd-MM-yyyy}", width: 150,
                            persistId: "terminated", // DON'T YOU DARE RENAME!
                            hidden: true,
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    operator: "gte"
                                }
                            }
                        },
                        {
                            field: "ProcurementStrategy", title: "Udbudsstrategi", width: 150,
                            persistId: "procurementStrategy", // DON'T YOU DARE RENAME!
                            hidden: true,
                            template: "#: ProcurementStrategy ? ProcurementStrategy.Name : '' #",
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ProcurementPlanYear", title: "Udbudsplan", width: 90,
                            headerTemplate: '<div style="word-wrap: break-word;">Udbudsplan</div>',
                            persistId: "procurementPlan", // DON'T YOU DARE RENAME!
                            template: "#: ProcurementPlanHalf && ProcurementPlanYear ? ProcurementPlanYear + ' | ' + ProcurementPlanHalf : ''#",
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "Advices.AlarmDate", title: "Dato for næste advis", width: 150,
                            persistId: "nextadvis", // DON'T YOU DARE RENAME!
                            template: nextAdviceTemplate,
                            hidden: true,
                            sortable: false,
                            filterable: false,
                        },
                    ]
                };

                function activate() {
                    // find the index of column where the role columns should be inserted
                    var insertIndex = _.findIndex(mainGridOptions.columns, "persistId", "orgunit") + 1;

                    // add a role column for each of the roles
                    // note iterating in reverse so we don't have to update the insert index
                    _.forEachRight(itContractRoles, function (role) {
                        var roleColumn = {
                            field: "role" + role.Id,
                            title: role.Name,
                            persistId: "role" + role.Id,
                            template: function (dataItem) {
                                return roleTemplate(dataItem, role.Id);
                            },
                            width: 170,
                            hidden: role.Name == "Kontraktejer" ? false : true, // hardcoded role name :(
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        };

                        // insert the generated column at the correct location
                        mainGridOptions.columns.splice(insertIndex, 0, roleColumn);
                    });

                    // assign the generated grid options to the scope value, kendo will do the rest
                    $scope.mainGridOptions = mainGridOptions;
                }
                activate();

                var exportFlag = false;
                function exportToExcel(e) {
                    var columns = e.sender.columns;

                    if (!exportFlag) {
                        e.preventDefault();
                        _.forEach(columns, function (column) {
                            if (column.hidden) {
                                column.tempVisual = true;
                                e.sender.showColumn(column);
                            }
                        });
                        $timeout(function () {
                            exportFlag = true;
                            e.sender.saveAsExcel();
                        });
                    } else {
                        exportFlag = false;
                        _.forEach(columns, function (column) {
                            if (column.tempVisual) {
                                delete column.tempVisual;
                                e.sender.hideColumn(column);
                            }
                        });
                    }
                }

                function roleTemplate(dataItem, roleId) {
                    var roles = "";

                    if (dataItem.roles[roleId] === undefined)
                        return roles;

                    // join the first 5 username together
                    if (dataItem.roles[roleId].length > 0)
                        roles = dataItem.roles[roleId].slice(0, 4).join(", ");

                    // if more than 5 then add an elipsis
                    if (dataItem.roles[roleId].length > 5)
                        roles += ", ...";

                    var link = "<a data-ui-sref='it-contract.edit.roles({id: " + dataItem.Id + "})'>" + roles + "</a>";

                    return link;
                }

                function folderTemplate(dataItem) {
                    if (dataItem.Folder)
                        return '<a href="' + dataItem.Folder + '" target="_blank"><i class="fa fa-link"></i></a>';
                    return "";
                }

                function nextAdviceTemplate(dataItem) {
                    if (dataItem.Advices.length > 0)
                        return moment(_.chain(dataItem.Advices).sortBy("AlarmDate").first().AlarmDate).format("DD-MM-YYYY");
                    return "";
                }

                function activeStatusTemplate(dataItem) {
                    var isActive = isContractActive(dataItem);

                    if (isActive)
                        return '<span class="fa fa-file text-success" aria-hidden="true"></span>';
                    return '<span class="fa fa-file-o text-muted" aria-hidden="true"></span>';
                }

                function isContractActive(dataItem) {
                    var today = moment();
                    var startDate = dataItem.Concluded ? moment(dataItem.Concluded) : today;
                    var endDate = dataItem.ExpirationDate ? moment(dataItem.ExpirationDate) : moment("9999-12-30");

                    if (dataItem.Terminated) {
                        var terminationDate = moment(dataItem.Terminated);
                        if (dataItem.TerminationDeadline) {
                            terminationDate.add(dataItem.TerminationDeadline.Name, "months");
                        }
                        // indgået-dato <= dags dato <= opsagt-dato + opsigelsesfrist
                        return today >= startDate && today <= terminationDate;
                    }

                    // indgået-dato <= dags dato <= udløbs-dato
                    return today >= startDate && today <= endDate;
                }

                function orgUnitDropDownList(args) {
                    function indent(dataItem) {
                        var htmlSpace = "&nbsp;&nbsp;&nbsp;&nbsp;";
                        return htmlSpace.repeat(dataItem.$level) + dataItem.Name;
                    }

                    function setDefaultOrgUnit() {
                        var kendoElem = this;
                        var idTofind = sessionStorage.getItem(orgUnitStorageKey);

                        if (!idTofind) {
                            // if no id was found then do nothing
                            return;
                        }

                        // find the index of the org unit that matches the users default org unit
                        var index = _.findIndex(kendoElem.dataItems(), function (item) {
                            return item.Id == idTofind;
                        });

                        // -1 = no match
                        //  0 = root org unit, which should display all. So remove org unit filter
                        if (index > 0) {
                            // select the users default org unit
                            kendoElem.select(index);
                        }
                    }

                    function orgUnitChanged() {
                        var kendoElem = this;
                        // can't use args.dataSource directly,
                        // if we do then the view doesn't update.
                        // So have to go through $scope - sadly :(
                        var dataSource = $scope.mainGrid.dataSource;
                        var currentFilter = dataSource.filter();
                        var selectedIndex = kendoElem.select();
                        var selectedId = _.parseInt(kendoElem.value());
                        var childIds = kendoElem.dataItem().childIds;

                        sessionStorage.setItem(orgUnitStorageKey, selectedId);

                        if (selectedIndex > 0) {
                            // filter by selected
                            dataSource.filter(getFilterWithOrgUnit(currentFilter, selectedId, childIds));
                        } else {
                            // else clear filter because the 0th element should act like a placeholder
                            dataSource.filter(getFilterWithOrgUnit(currentFilter));
                        }
                    }

                    // http://dojo.telerik.com/ODuDe/5
                    args.element.removeAttr("data-bind");
                    args.element.kendoDropDownList({
                        dataSource: orgUnits,
                        dataValueField: "Id",
                        dataTextField: "Name",
                        template: indent,
                        dataBound: setDefaultOrgUnit,
                        change: orgUnitChanged
                    });
                }

                function getFilterWithOrgUnit(currentFilter, selectedId, childIds) {
                    var field = "ResponsibleOrganizationUnit.Id";
                    // remove old values first
                    var newFilter = _.removeFiltersForField(currentFilter, field);

                    // is selectedId a number?
                    if (!isNaN(selectedId)) {
                        newFilter = _.addFilter(newFilter, field, "eq", selectedId, "or");
                        // add children to filters
                        _.forEach(childIds, function (id) {
                            newFilter = _.addFilter(newFilter, field, "eq", id, "or");
                        });
                    }
                    return newFilter;
                }

                $scope.roleSelectorOptions = {
                    autoBind: false,
                    dataSource: itContractRoles,
                    dataTextField: "Name",
                    dataValueField: "Id",
                    optionLabel: "Vælg kontraktrolle...",
                    change: function (e) {
                        // hide all roles column
                        _.forEach(itContractRoles, function (role) {
                            $scope.mainGrid.hideColumn("role" + role.Id);
                        });

                        var selectedId = e.sender.value();
                        var gridFieldName = "role" + selectedId;
                        // show only the selected role column
                        $scope.mainGrid.showColumn(gridFieldName);
                    }
                };
            }]);
})(angular, app);

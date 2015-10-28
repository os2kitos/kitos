(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.catalog', {
            url: '/catalog',
            templateUrl: 'partials/it-system/it-system-catalog.html',
            controller: 'system.CatalogCtrl',
            resolve: {
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ]
            }
        });
    }]);

    app.controller('system.CatalogCtrl',
    [
        '$rootScope', '$scope', '$http', 'notify', '$state', 'user', '$timeout', 'gridStateService',
        function ($rootScope, $scope, $http, notify, $state, user, $timeout, gridStateService) {
            $rootScope.page.title = 'IT System - Katalog';

            // usagedetails grid empty-grid handling
            function detailsBound(e) {
                var grid = e.sender;
                if (grid.dataSource.total() == 0) {
                    var colCount = grid.columns.length;
                    $(e.sender.wrapper)
                        .find('tbody')
                        .append('<tr class="kendo-data-row"><td colspan="' + colCount + '" class="no-data text-muted">System anvendens ikke</td></tr>');
                }
            };

            var storageKey = "it-system-catalog-options";
            var gridState = gridStateService.getService(storageKey);

            // saves grid state to localStorage
            function saveGridOptions() {
                gridState.saveGridOptions($scope.mainGrid);
            }

            // loads kendo grid options from localstorage
            function loadGridOptions() {
                gridState.loadGridOptions($scope.mainGrid);
            }

            $scope.saveGridProfile = function () {
                gridState.saveGridProfile($scope.mainGrid);
                notify.addSuccessMessage("Filtre og sortering gemt");
            }

            $scope.clearGridProfile = function () {
                gridState.clearGridProfile($scope.mainGrid);
                notify.addSuccessMessage("Filtre og sortering slettet");
            }

            $scope.doesGridProfileExist = function () {
                return gridState.doesGridProfileExist();
            }

            $scope.$on("kendoWidgetCreated", function (event, widget) {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === $scope.mainGrid) {
                    loadGridOptions();
                    $scope.mainGrid.dataSource.read();

                    // find the access modifier filter row section
                    var accessModifierFilterRow = $(".k-filter-row [data-field='AccessModifier']");
                    // find the access modifier kendo widget
                    var accessModifierFilterWidget = accessModifierFilterRow.find("input").data("kendoDropDownList");
                    // attach a click event to the X (remove filter) button
                    accessModifierFilterRow.find("button").on("click", function () {
                        // set the selected filter to none, because clicking the button removes the filter
                        accessModifierFilterWidget.select(0);
                    });
                }
            });

            // clears grid filters by removing the localStorageItem and reloading the page
            $scope.clearOptions = function () {
                gridState.clearOptions();
                notify.addSuccessMessage("Nulstiller tilbage til standard sortering, viste kolonner, kolonne vide og kolonne rækkefølge samt fjerner filtre");
                // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
                reload();
            }

            function reload() {
                $state.go('.', null, { reload: true });
            }

            // show usageDetailsGrid - takes a itSystemUsageId for data and systemName for modal title
            $scope.showUsageDetails = function (usageId, systemName) {
                //Filter by usageId
                usageDetailDataSource.filter({ field: "ItSystemId", operator: "eq", value: usageId });
                //Set modal title
                $scope.modal.setOptions({ title: "Anvendelse af " + systemName });
                //Open modal
                $scope.modal.center().open();
            }

            var usageDetailDataSource = new kendo.data.DataSource({
                type: "odata-v4",
                transport:
                {
                    read: {
                        url: "/odata/ItSystemUsages?$expand=Organization",
                        dataType: "json"
                    },
                },
                serverPaging: true,
                serverSorting: true,
                serverFiltering: true
            });

            // usagedetails grid
            $scope.usageDetailsGrid = {
                dataSource: usageDetailDataSource,
                autoBind: false,
                columns: [
                    {
                        field: "Organization.Name",
                        title: "Organisation"
                    }
                ],
                dataBound: detailsBound
            };

            var itSystemBaseUrl;
            if (user.isGlobalAdmin) {
                // global admin should see all it systems everywhere with all levels of access
                itSystemBaseUrl = "/odata/ItSystems";
            } else {
                // everyone else are limited to within organizationnal context
                itSystemBaseUrl = "/odata/Organizations(" + user.currentOrganizationId + ")/ItSystems";
            }

            var itSystemUrl = itSystemBaseUrl + "?$expand=AppTypeOption,BusinessType,BelongsTo,TaskRefs,Parent,Organization,ObjectOwner,Usages($expand=Organization),LastChangedByUser";


            // catalog grid
            $scope.itSystemCatalogueGrid = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: itSystemUrl,
                            dataType: "json"
                        },
                        parameterMap: function (options, type) {
                            var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                            if (parameterMap.$filter) {
                                // replaces 'Kitos.AccessModifier0' with Kitos.AccessModifier'0'
                                parameterMap.$filter = parameterMap.$filter.replace(/('Kitos\.AccessModifier([0-9])')/, "Kitos.AccessModifier'$2'");
                                // replaces "startswith(TaskKey,'11')" with "TaskRefs/any(c: startswith(c/TaskKey),'11')"
                                parameterMap.$filter = parameterMap.$filter.replace(/(\w+\()(TaskKey.*\))/, "TaskRefs/any(c: $1c/$2)");
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
                                LastChanged: { type: "date" }
                            }
                        }
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true
                },
                toolbar: [
                    { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template: "<button class='k-button k-button-icontext' data-ng-click='clearOptions()' title='Nulstiller tilbage til standard sortering, filter, kolonne vide og kolonne rækkefølge'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template: "<button class='k-button k-button-icontext' data-ng-click='saveGridProfile()' title='Gemmer sortering og filtre'>#: text #</button>"
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template: "<button class='k-button k-button-icontext' data-ng-click='clearGridProfile()' data-ng-disabled='!doesGridProfileExist()'>#: text #</button>"
                    }
                ],
                excel: {
                    fileName: "Snitflade Katalog.xlsx",
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
                error: function(e) {
                    console.log(e);
                },
                columns: [
                    {
                        field: "Usages", title: "Anvend/Fjern anvendelse", width: 110,
                        persistId: "command", // DON'T YOU DARE RENAME!
                        template: usageButtonTemplate,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "Parent.Name", title: "Overordnet IT System", width: 150,
                        persistId: "parentname", // DON'T YOU DARE RENAME!
                        template: "#: Parent ? Parent.Name : '' #",
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
                        field: "Name", title: "It System", width: 220,
                        persistId: "name", // DON'T YOU DARE RENAME!
                        template: '<a data-ui-sref="it-system.edit.interfaces({id: #: Id #})">#: Name #</a>',
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "AccessModifier", title: "Synlighed", width: 100,
                        persistId: "accessmod", // DON'T YOU DARE RENAME!
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: accessModFilter,
                            }
                        }
                    },
                    {
                        field: "BusinessType.Name", title: "Forretningstype", width: 150,
                        persistId: "busitype", // DON'T YOU DARE RENAME!
                        template: "#: BusinessType ? BusinessType.Name : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "AppTypeOption.Name", title: "Applikationstype", width: 150,
                        persistId: "apptype", // DON'T YOU DARE RENAME!
                        template: "#: AppTypeOption ? AppTypeOption.Name : '' #",
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
                        field: "BelongsTo.Name", title: "Rettighedshaver", width: 150,
                        persistId: "belongsto", // DON'T YOU DARE RENAME!
                        template: "#: BelongsTo ? BelongsTo.Name : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "TaskKey", title: "KLE ID", width: 150,
                        persistId: "taskkey", // DON'T YOU DARE RENAME!
                        template: "#: TaskRefs.length > 0 ? _.pluck(TaskRefs.slice(0,4), 'TaskKey').join(', ') : '' ##: TaskRefs.length > 5 ? ', ...' : '' #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "startswith",
                            }
                        },
                        sortable: false
                    },
                    {
                        field: "TaskName", title: "KLE Navn", width: 150,
                        persistId: "taskname", // DON'T YOU DARE RENAME!
                        template: "#: TaskRefs.length > 0 ? _.pluck(TaskRefs.slice(0,4), 'Description').join(', ') : '' ##: TaskRefs.length > 5 ? ', ...' : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "startswith",
                            }
                        },
                        sortable: false
                    },
                    {
                        field: "Url", title: "Link til yderligere beskrivelse", width: 75,
                        persistId: "link", // DON'T YOU DARE RENAME!
                        template: linkTemplate,
                        attributes: { "class": "text-center" },
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "Usages.length", title: "IT System: Anvendes af", width: 95,
                        persistId: "usages", // DON'T YOU DARE RENAME!
                        template: '<a class="col-md-7 text-center" data-ng-click="showUsageDetails(#: Id #,\'#: Name #\')">#: Usages.length #</a>',
                        filterable: false,
                        sortable: false
                    },
                    //{
                    //    field: "", title: "Snitflader: Udstilles globalt", width: 95,
                    //    persistId: "globalexpsure", // DON'T YOU DARE RENAME!
                    //    template: "TODO",
                    //    hidden: true,
                    //    filterable: false,
                    //    sortable: false
                    //},
                    //{
                    //    field: "", title: "Snitflader: Anvendes globalt", width: 95,
                    //    persistId: "globalusage", // DON'T YOU DARE RENAME!
                    //    template: "TODO",
                    //    hidden: true,
                    //    filterable: false,
                    //    sortable: false
                    //},
                    {
                        field: "Organization.Name", title: "Oprettet af: Organisation", width: 150,
                        persistId: "orgname", // DON'T YOU DARE RENAME!
                        template: "#: Organization ? Organization.Name : '' #",
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
                        field: "ObjectOwner.Name", title: "Oprettet af: Bruger", width: 150,
                        persistId: "ownername", // DON'T YOU DARE RENAME!
                        template: "#: ObjectOwner.Name + ' ' + ObjectOwner.LastName #",
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
                        field: "LastChangedByUser.Name", title: "Sidst redigeret: Bruger", width: 150,
                        persistId: "lastchangedname", // DON'T YOU DARE RENAME!
                        template: "#: LastChangedByUser.Name + ' ' + LastChangedByUser.LastName #",
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
                        field: "LastChanged", title: "Sidst redigeret: Dato", format: "{0:dd-MM-yyyy}", width: 75,
                        persistId: "lastchangeddate", // DON'T YOU DARE RENAME!
                        filterable: {
                            cell: {
                                showOperators: false,
                                operator: "gte"
                            }
                        }
                    }
                ]
            };

            function usageButtonTemplate(dataItem) {
                // true if system is being used by system within current context, else false
                var systemHasUsages = _.find(dataItem.Usages, function(d) { return d.OrganizationId == user.currentOrganizationId; });

                if (systemHasUsages)
                    return '<button class="btn btn-danger col-md-7" data-ng-click="removeUsage(' + dataItem.Id + ')">Fjern anv.</button>';

                return '<button class="btn btn-success col-md-7" data-ng-click="enableUsage(' + dataItem.Id + ')">Anvend</button>';
            }

            // adds usage at selected system within current context
            $scope.enableUsage = function (dataItem) {
                addUsage(dataItem).then(function() {
                    $scope.mainGrid.dataSource.fetch();
                });
            }

            // removes usage at selected system within current context
            $scope.removeUsage = function (dataItem) {
                var sure = confirm("Er du sikker på at du vil fjerne den lokale anvendelse af systemet? Dette sletter ikke systemet, men vil slette alle lokale detaljer vedrørende anvendelsen.");
                if (sure)
                    deleteUsage(dataItem).then(function() {
                        $scope.mainGrid.dataSource.fetch();
                    });
            }

            // adds system to usage within the current context
            function addUsage(systemId) {
                return $http.post('api/itsystemusage', {
                    itSystemId: systemId,
                    organizationId: user.currentOrganizationId
                }).success(function () {
                    notify.addSuccessMessage('Systemet er taget i anvendelse');
                }).error(function () {
                    notify.addErrorMessage('Systemet kunne ikke tages i anvendelse!');
                });
            }

            // removes system from usage within the current context
            function deleteUsage(systemId) {
                var url = 'api/itsystemusage?itSystemId=' + systemId + '&organizationId=' + user.currentOrganizationId;

                return $http.delete(url).success(function() {
                    notify.addSuccessMessage('Anvendelse af systemet er fjernet');
                }).error(function() {
                    notify.addErrorMessage('Anvendelse af systemet kunne ikke fjernes!');
                });
            }

            function linkTemplate(dataItem) {
                if (dataItem.Url)
                    return '<a href="' + dataItem.Url + '" title="Link til yderligere..." target="_blank"><i class="fa fa-link"></i></a>';
                return "";
            }

            function accessModFilter(args) {
                var gridDataSource = args.dataSource;

                function setSelected() {
                    var kendoElem = this;
                    var currentFilter = gridDataSource.filter();
                    var filterObj = _.findKeyDeep(currentFilter, { field: "AccessModifier" });

                    switch (filterObj.value) {
                        case "Kitos.AccessModifier0":
                            kendoElem.select(1);
                            break;
                        case "Kitos.AccessModifier1":
                            kendoElem.select(2);
                            break;
                        case "Kitos.AccessModifier2":
                            kendoElem.select(3);
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
                    var dataSource = $scope.mainGrid.dataSource;
                    var selectedValue = kendoElem.value();
                    var field = "AccessModifier";
                    var currentFilter = dataSource.filter();
                    // remove old value first
                    var newFilter = _.removeFiltersForField(currentFilter, field);

                    if (selectedValue) {
                        newFilter = _.addFilter(newFilter, field, "eq", selectedValue, "and");
                    }

                    dataSource.filter(newFilter);
                }

                // http://dojo.telerik.com/ODuDe/5
                args.element.removeAttr("data-bind");
                args.element.kendoDropDownList({
                    dataSource: [
                        { value: "Kitos.AccessModifier0", text: "Normal" },
                        { value: "Kitos.AccessModifier1", text: "Public" },
                        { value: "Kitos.AccessModifier2", text: "Private" }
                    ],
                    dataTextField: "text",
                    dataValueField: "value",
                    optionLabel: "Vælg filter...",
                    dataBound: setSelected,
                    change: applyFilter
                });
            }
        }
    ]);
})(angular, app);

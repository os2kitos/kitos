(function (ng, app) {
    app.config([
        '$stateProvider', function($stateProvider) {
            $stateProvider.state('it-system.interfaceCatalog', {
                url: '/interface-catalog',
                templateUrl: 'partials/it-system/it-interface-catalog.html',
                controller: 'system.interfaceCatalogCtrl',
                resolve: {
                    user: [
                        'userService', function (userService) {
                            return userService.getUser();
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('system.interfaceCatalogCtrl',
    [
        '$rootScope', '$scope', '$timeout', '$state', 'user', 'gridStateService',
        function ($rootScope, $scope, $timeout, $state, user, gridStateService) {
            $rootScope.page.title = 'Snitflade - Katalog';

            $scope.itInterfaceOptions = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: "/odata/Organizations(" + user.currentOrganizationId + ")/ItInterfaces?$expand=Interface,InterfaceType,ObjectOwner,BelongsTo,Organization,Tsa,ExhibitedBy($expand=ItSystem),Method,LastChangedByUser,DataRows($expand=DataType),InterfaceLocalUsages",
                            dataType: "json"
                        },
                        parameterMap: function (options, type) {
                            var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                            if (parameterMap.$filter) {
                                // replaces 'Kitos.AccessModifier0' with Kitos.AccessModifier'0'
                                parameterMap.$filter = parameterMap.$filter.replace(/('Kitos\.AccessModifier([0-9])')/, "Kitos.AccessModifier'$2'");
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
                    serverFiltering: true,
                },
                toolbar: [
                    { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template: "<a class='k-button k-button-icontext' data-ng-click='clearOptions()'>#: text #</a>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template: "<a class='k-button k-button-icontext' data-ng-click='saveGridProfile()'>#: text #</a>"
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template: "<a class='k-button k-button-icontext' data-ng-click='clearGridProfile()'>#: text #</a>"
                    }
                ],
                excel: {
                    fileName: "IT System Katalog.xlsx",
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
                columns: [
                    {
                        field: "ItInterfaceId", title: "Snidtflade ID", width: 150,
                        persistId: "infid", // DON'T YOU DARE RENAME!
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "Name", title: "Snitflade", width: 150,
                        persistId: "name", // DON'T YOU DARE RENAME!
                        template: "<a data-ui-sref='it-system.interface-edit.interface-details({id: #: Id #})'>#: Name #</a>",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "Version", title: "Version", width: 150,
                        persistId: "version", // DON'T YOU DARE RENAME!
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
                        field: "AccessModifier", title: "Synlighed", width: 80,
                        persistId: "accessmod", // DON'T YOU DARE RENAME!
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: accessModFilter,
                            }
                        }
                    },
                    {
                        field: "InterfaceType.Name", title: "Snitfladetype", width: 150,
                        persistId: "inftype", // DON'T YOU DARE RENAME!
                        template: "#: InterfaceType ? InterfaceType.Name : '' #",
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
                        persistId: "belongs", // DON'T YOU DARE RENAME!
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "Url", title: "Link til yderligere beskrivelse", width: 100,
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
                        field: "ExhibitedBy.ItSystem.Name", title: "Udstillet af", width: 150,
                        persistId: "exhibit", // DON'T YOU DARE RENAME!
                        template: "#: ExhibitedBy ? ExhibitedBy.ItSystem.Name : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        },
                    },
                    {
                        field: "", title: "Snitflader: Anvendes globalt", width: 150,
                        persistId: "infglobalusage", // DON'T YOU DARE RENAME!
                        template: "#: InterfaceLocalUsages.length #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        },
                    },
                    {
                        field: "Tsa.Name", title: "TSA", width: 150,
                        persistId: "tsa", // DON'T YOU DARE RENAME!
                        template: "#: Tsa ? Tsa.Name : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        },
                    },
                    {
                        field: "Interface.Name", title: "Grænseflade", width: 150,
                        persistId: "infname", // DON'T YOU DARE RENAME!
                        template: "#: Interface ? Interface.Name : '' #",
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
                        field: "Method.Name", title: "Metode", width: 150,
                        persistId: "method", // DON'T YOU DARE RENAME!
                        template: "#: Method ? Method.Name : '' #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        },
                    },
                    {
                        field: "", title: "Datatype", width: 150,
                        persistId: "datatypes", // DON'T YOU DARE RENAME!
                        template: "#: DataRows.length > 0 ? _.pluck(DataRows.slice(0,4), 'DataType.Name').join(', ') : '' ##: DataRows.length > 5 ? ', ...' : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        },
                    },
                    {
                        field: "Organization.Name", title: "Oprettet af: Organisation", width: 150,
                        persistId: "orgname", // DON'T YOU DARE RENAME!
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
                        field: "LastChanged", title: "Sidst redigeret: Dato", format: "{0:dd-MM-yyyy}", width: 150,
                        persistId: "lastchangeddate", // DON'T YOU DARE RENAME!
                        hidden: true,
                        filterable: {
                            cell: {
                                showOperators: false,
                                operator: "gte"
                            }
                        }
                    }
                ],
                dataBound: saveGridOptions,
                columnResize: saveGridOptions,
                columnHide: saveGridOptions,
                columnShow: saveGridOptions,
                columnReorder: saveGridOptions,
                error: function (e) {
                    console.log(e);
                }
            };

            var storageKey = "it-interface-catalog-options";
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
            }

            $scope.clearGridProfile = function () {
                gridState.clearGridProfile($scope.mainGrid);
            }

            // fires when kendo is finished rendering all its goodies
            $scope.$on("kendoRendered", function () {
                loadGridOptions();
                $scope.mainGrid.dataSource.read();
            });

            // clears grid filters by removing the localStorageItem and reloading the page
            $scope.clearOptions = function () {
                gridState.clearOptions();
                // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
                reload();
            }

            function reload() {
                $state.go('.', null, { reload: true });
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

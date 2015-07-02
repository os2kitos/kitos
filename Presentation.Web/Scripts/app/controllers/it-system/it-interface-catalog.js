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

            var itInterfaceCatalogDataSource = new kendo.data.DataSource({
                type: "odata-v4",
                transport: {
                    read: {
                        url: "/odata/Organizations(" + user.currentOrganizationId + ")/ItInterfaces?$expand=Interface,InterfaceType,ObjectOwner,BelongsTo,Organization,Tsa,ExhibitedBy($expand=ItSystem),Method",
                        dataType: "json"
                    }
                },
                sort: {
                    field: "Name",
                    dir: "asc"
                },
                pageSize: 10,
                serverPaging: true,
                serverSorting: true,
                serverFiltering: true,
            });

            $scope.itInterfaceOptions = {
                autoBind: false,
                dataSource: itInterfaceCatalogDataSource,
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
                    pageSizes: [10, 20, 50, 100, 200],
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
                        field: "ItInterfaceId", title: "Snidtflade ID", width: 150, persistId: "infid",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "Name", title: "Snitflade", width: 150, persistId: "name",
                        template: "<a data-ui-sref='it-system.interface-edit.interface-details({id: #: Id #})'>#: Name #</a>",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "AccessModifier", title: "Tilgængelighed", width: 80, persistId: "accessmod",
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "InterfaceType.Name", title: "Snitfladetype", width: 150, persistId: "inftype",
                        template: "#: InterfaceType ? InterfaceType.Name : '' #",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "Interface.Name", title: "Grænseflade", width: 150, persistId: "infname",
                        template: "#: Interface ? Interface.Name : '' #",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "Method.Name", title: "Metode", width: 150, persistId: "method",
                        template: "#: Method ? Method.Name : '' #",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        },
                    },
                    {
                        field: "Tsa.Name", title: "TSA", width: 150, persistId: "tsa",
                        template: "#: Tsa ? Tsa.Name : '' #",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        },
                    },
                    {
                        field: "ExhibitedBy.ItSystem.Name", title: "Udstillet af", width: 150, persistId: "exhibit",
                        template: "#: ExhibitedBy ? ExhibitedBy.ItSystem.Name : '' #",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        },
                    },
                    {
                        field: "BelongsTo.Name", title: "Rettighedshaver", width: 150, persistId: "belongs",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "Organization.Name", title: "Oprettet i", width: 150, persistId: "orgname",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "ObjectOwner.Name", title: "Oprettet af", width: 150, persistId: "owername",
                        template: "#: ObjectOwner.Name + ' ' + ObjectOwner.LastName #",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
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
            $scope.$on("kendoRendered", function (e) {
                loadGridOptions();
                $scope.mainGrid.dataSource.fetch();
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
        }
    ]);
})(angular, app);

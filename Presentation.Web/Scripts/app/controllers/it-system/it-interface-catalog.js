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
        '$rootScope', '$scope', '$timeout', 'user',
        function ($rootScope, $scope, $timeout, user) {
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
                dataSource: itInterfaceCatalogDataSource,
                toolbar: [
                    { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template: "<a class='k-button k-button-icontext' data-ng-click='clearOptions()'>#: text #</a>"
                    }
                ],
                excel: {
                    fileName: "IT System Katalog.xlsx",
                    filterable: true,
                    allPages: true
                },
                pageable: {
                    refresh: true,
                    pageSizes: true,
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
                        locked: true,
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "Name", title: "Snitflade", width: 150,
                        template: "<a data-ui-sref='it-system.interface-edit.interface-details({id: #: Id #})'>#: Name #</a>",
                        locked: true,
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "AccessModifier", title: "Tilgængelighed", width: 80,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "InterfaceType.Name", title: "Snitfladetype", width: 150,
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
                        field: "Interface.Name", title: "Grænseflade", width: 150,
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
                        field: "Method.Name", title: "Metode", width: 150,
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
                        field: "Tsa.Name", title: "TSA", width: 150,
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
                        field: "ExhibitedBy.ItSystem.Name", title: "Udstillet af", width: 150,
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
                        field: "BelongsTo.Name", title: "Rettighedshaver", width: 150,
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "Organization.Name", title: "Oprettet i", width: 150,
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "ObjectOwner.Name", title: "Oprettet af", width: 150,
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

            var localStorageKey = "kendo-grid-it-interface-catalog-options";

            // saves grid state to localStorage
            function saveGridOptions(e) {
                if ($scope.mainGrid) {
                    // timeout fixes columnReorder saves before the column is actually reordered 
                    // http://stackoverflow.com/questions/21270748/kendo-grid-saving-state-in-columnreorder-event
                    $timeout(function () {
                        var options = $scope.mainGrid.getOptions();
                        var pickedOptions = {}; // _.pick(options, 'columns'); BUG disabled for now as saving column data overwrites source changes - FUBAR!
                        pickedOptions.dataSource = _.pick(options.dataSource, ['filter', 'sort', 'page', 'pageSize']);
                        localStorage[localStorageKey] = kendo.stringify(pickedOptions);
                    });
                }
            }

            // loads kendo grid options from localstorage
            function loadOptions() {
                var options = localStorage[localStorageKey];
                if (options) {
                    $scope.mainGrid.setOptions(JSON.parse(options));
                }
            }

            // fires when kendo is finished rendering all its goodies
            $scope.$on("kendoRendered", function (e) {
                loadOptions();
            });

            // clears grid filters by removing the localStorageItem and reloading the page
            $scope.clearOptions = function () {
                localStorage.removeItem(localStorageKey);
                itInterfaceCatalogDataSource.read();
            }
        }
    ]);
})(angular, app);

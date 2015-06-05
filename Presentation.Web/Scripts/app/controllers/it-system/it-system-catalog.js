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
        '$rootScope', '$scope', '$http', 'notify', '$state', 'user', '$timeout',
        function ($rootScope, $scope, $http, notify, $state, user, $timeout) {
            $rootScope.page.title = 'IT System - Katalog';
            
            // adds system to usage within the current context
            function addODataUsage(systemId) {
                return $http.post('api/itsystemusage', {
                    itSystemId: systemId,
                    organizationId: user.currentOrganizationId
                }).success(function(result) {
                    notify.addSuccessMessage('Systemet er taget i anvendelse');
                    $scope.mainGrid.dataSource.read();
                }).error(function(result) {
                    notify.addErrorMessage('Systemet kunne ikke tages i anvendelse!');
                });
            }

            // removes system from usage within the current context
            function deleteODataUsage(systemId) {
                var url = 'api/itsystemusage?itSystemId=' + systemId + '&organizationId=' + user.currentOrganizationId;

                return $http.delete(url).success(function(result) {
                    notify.addSuccessMessage('Anvendelse af systemet er fjernet');
                    $scope.mainGrid.dataSource.read();
                }).error(function(result) {
                    notify.addErrorMessage('Anvendelse af systemet kunne ikke fjernes!');
                });
            }

            //usagedetails grid empty-grid handling
            function detailsBound(e) {
                var grid = e.sender;
                if (grid.dataSource.total() == 0) {
                    var colCount = grid.columns.length;
                    $(e.sender.wrapper)
                        .find('tbody')
                        .append('<tr class="kendo-data-row"><td colspan="' + colCount + '" class="no-data text-muted">System anvendens ikke</td></tr>');
                }
            };

            var localStorageKey = "kendo-grid-it-system-catalog-options";

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
                itSystemCatalogDataSource.read();
            }

            // show usageDetailsGrid - takes a itSystemUsageId for data and systemName for modal title
            $scope.showUsageDetails = function (usageId, systemName) {
                //Filter by usageId
                $scope.usageGrid.dataSource.filter({ field: "ItSystemId", operator: "eq", value: usageId });
                //Set modal title
                $scope.modal.setOptions({ title: "Anvendelse af " + systemName });
                //Open modal
                $scope.modal.center().open();
            }

            // usagedetails grid
            $scope.usageDetailsGrid = {
                    dataSource: {
                    type: "odata-v4",
                        transport:
                        {
                            read: {
                                url: "/odata/ItSystemUsages?$expand=Organization",
                                dataType: "json"
                            },
                        },
                    pageSize: 10,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true
                },
                columns: [
                    {
                        field: "Organization.Name",
                        title: "Organisation"
                    }
                ],
                dataBound: detailsBound
            };

            var itSystemCatalogDataSource = new kendo.data.DataSource({
                type: "odata-v4",
                transport: {
                    read: {
                        url: "/odata/Organizations(" + user.currentOrganizationId + ")/ItSystems?$expand=AppTypeOption,BusinessType,BelongsTo,TaskRefs,Parent,Organization,ObjectOwner,Usages($expand=Organization)",
                        dataType: "json"
                    },
                    parameterMap: function (options, type) {
                        var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                        if (parameterMap.$filter) {
                            // replaces "startswith(TaskKey,'11')" with "TaskRefs/any(c: startswith(c/TaskKey),'11')"
                            parameterMap.$filter = parameterMap.$filter.replace(/(\w+\()(TaskKey.*\))/, "TaskRefs/any(c: $1c/$2)");
                        }
                        
                        return parameterMap;
                    }
                },
                pageSize: 10,
                serverPaging: true,
                serverSorting: true,
                serverFiltering: true
            });

            // catalog grid
            $scope.itSystemCatalogueGrid = {
                dataSource: itSystemCatalogDataSource,
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
                        field: "Name", title: "It System", width: 150,
                        template: '<a data-ui-sref="it-system.edit.interfaces({id: #: Id #})">#: Name #</a>',
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
                        field: "Parent.Name", title: "Overordnet", width: 150,
                        template: "#: Parent ? Parent.Name : '' #",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "AppTypeOption.Name", title: "Applikationstype", width: 150,
                        template: "#: AppTypeOption ? AppTypeOption.Name : '' #",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        field: "BusinessType.Name", title: "Forretningtype", width: 150,
                        template: "#: BusinessType ? BusinessType.Name : '' #",
                        filterable: {
                            cell: {
                                delay: 1500,
                                operator: "contains",
                                suggestionOperator: "contains"
                            }
                        }
                    },
                    {
                        // DON'T YOU DARE RENAME!
                        field: "TaskKey", title: "KLE", width: 150,
                        template: "#: TaskRefs.length > 0 ? _.pluck(TaskRefs.slice(0,4), 'TaskKey').join(', ') : '' ##: TaskRefs.length > 5 ? ', ...' : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                delay: 1500,
                                operator: "startswith",
                            }
                        },
                        sortable: false
                    },
                    {
                        field: "BelongsTo.Name", title: "Rettighedshaver", width: 150,
                        template: "#: BelongsTo ? BelongsTo.Name : '' #",
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
                        template: "#: Organization ? Organization.Name : '' #",
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
                    },
                    {
                        field: "Usages.length" || 0, title: "Anvender", width: 95,
                        template: '<a class="col-md-7 text-center" data-ng-click="showUsageDetails(#: Id #,\'#: Name #\')">#: Usages.length #</a>',
                        filterable: false,
                        sortable: false
                    },
                    {
                        title: "Anvendelse",
                        width: 110,
                        field: "Usages",
                        template: '<button class="btn btn-success col-md-7" data-ng-click="enableUsage(#: Id #)" data-ng-show="!systemHasUsages(dataItem)">Anvend</button>' +
                                  '<button class="btn btn-danger  col-md-7" data-ng-click="removeUsage(#: Id #)" data-ng-show="systemHasUsages(dataItem)">Fjern anv.</button>',
                        filterable: false,
                        sortable: false
                    },
                ],
                dataBound: saveGridOptions,
                columnResize: saveGridOptions,
                columnHide: saveGridOptions,
                columnShow: saveGridOptions,
                columnReorder: saveGridOptions,
                error: function(e) {
                    console.log(e);
                }
            };

            // returns bool if system is being used by system within current context
            $scope.systemHasUsages = function(system) {
                return _.find(system.Usages, function (d) { return d.OrganizationId == user.currentOrganizationId; });
            }

            // adds usage at selected system within current context
            $scope.enableUsage = function (dataItem) {
                addODataUsage(dataItem);
            }

            // removes usage at selected system within current context
            $scope.removeUsage = function (dataItem) {
                var sure = confirm("Er du sikker på at du vil fjerne den lokale anvendelse af systemet? Dette sletter ikke systemet, men vil slette alle lokale detaljer vedrørende anvendelsen.");
                if (sure)
                    deleteODataUsage(dataItem);
            }
        }
    ]);
})(angular, app);

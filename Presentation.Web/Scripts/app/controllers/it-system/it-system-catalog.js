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

            // catalog grid
            $scope.itSystemCatalogueGrid = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: "/odata/Organizations(" + user.currentOrganizationId + ")/ItSystems?$expand=AppTypeOption,BusinessType,BelongsTo,TaskRefs,Parent,Organization,ObjectOwner,Usages($expand=Organization),LastChangedByUser",
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
                        field: "Name", title: "It System", width: 150,
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
                        field: "AccessModifier", title: "Synlighed", width: 80,
                        persistId: "accessmod", // DON'T YOU DARE RENAME!
                        filterable: false,
                        sortable: false
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
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "BelongsTo.Name", title: "Rettighedshaver", width: 150, persistId: "belongsto",
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
                        field: "Url", title: "Link til yderligere beskrivelse", width: 100,
                        persistId: "link", // DON'T YOU DARE RENAME!
                        template: linkTemplate,
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
                        field: "Usages.length", title: "IT System: Anvendes af", width: 95,
                        persistId: "usages", // DON'T YOU DARE RENAME!
                        template: '<a class="col-md-7 text-center" data-ng-click="showUsageDetails(#: Id #,\'#: Name #\')">#: Usages.length #</a>',
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "", title: "Snitflader: Udstilles globalt", width: 95,
                        persistId: "globalexpsure", // DON'T YOU DARE RENAME!
                        template: "TODO",
                        hidden: true,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "", title: "Snitflader: Anvendes globalt", width: 95,
                        persistId: "globalusage", // DON'T YOU DARE RENAME!
                        template: "TODO",
                        hidden: true,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "Organization.Name", title: "Oprettet af: Organisation", width: 150,
                        persistId: "orgname", // DON'T YOU DARE RENAME!
                        template: "#: Organization ? Organization.Name : '' #",
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
                error: function(e) {
                    console.log(e);
                }
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
        }
    ]);
})(angular, app);

(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.catalog', {
            url: '/catalog',
            templateUrl: 'partials/it-system/it-system-catalog.html',
            controller: 'system.CatalogCtrl',
            resolve: {
                organizations: [
                    '$http', function ($http) {
                        return $http.get('api/organization');
                    }
                ],
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
        '$rootScope', '$scope', '$http', 'notify', '$state', 'organizations', 'user',
        function($rootScope, $scope, $http, notify, $state, organizationsHttp, user) {
            $rootScope.page.title = 'IT System - Katalog';

            //$scope.pagination = {
            //    search: '',
            //    skip: 0,
            //    take: 20
            //};

            //$scope.csvUrl = 'api/itSystem/?csv&organizationId=' + user.currentOrganizationId;

            //var organizations = organizationsHttp.data.response;

            //function loadUser(system) {
            //    return $http.get('api/user/' + system.objectOwnerId, { cache: true })
            //        .success(function(result) {
            //            system.user = result.response;
            //        });
            //}

            //function loadOrganization(system) {
            //    return $http.get('api/organization/' + system.organizationId, { cache: true })
            //        .success(function(result) {
            //            system.organization = result.response;
            //        });
            //}

            //function loadTaskRef(system) {
            //    if (system.taskRefIds.length == 0) return null;

            //    return $http.get('api/taskref/' + system.taskRefIds[0])
            //        .success(function(result) {
            //            system.taskId = result.response.taskKey;
            //            system.taskName = result.response.description;
            //        });
            //}

            //function loadUsage(system) {
            //    return $http.get(system.usageUrl)
            //        .success(function(result) {
            //            system.hasUsage = true;
            //            system.usage = result.response;
            //        });
            //}

            function addUsage(system) {
                return $http.post('api/itsystemusage', {
                    itSystemId: system.id,
                    organizationId: user.currentOrganizationId
                }).success(function(result) {
                    notify.addSuccessMessage('Systemet er taget i anvendelse');
                    system.hasUsage = true;
                    system.usage = result.response;
                }).error(function(result) {
                    notify.addErrorMessage('Systemet kunne ikke tages i anvendelse!');
                });
            }

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

            function deleteUsage(system) {

                return $http.delete(system.usageUrl).success(function(result) {
                    notify.addSuccessMessage('Anvendelse af systemet er fjernet');
                    system.hasUsage = false;
                }).error(function(result) {
                    notify.addErrorMessage('Anvendelse af systemet kunne ikke fjernes!');
                });
            }

            function deleteODataUsage(systemId) {
                var url = 'api/itsystemusage?itSystemId=' + systemId + '&organizationId=' + user.currentOrganizationId;

                return $http.delete(url).success(function(result) {
                    notify.addSuccessMessage('Anvendelse af systemet er fjernet');
                    $scope.mainGrid.dataSource.read();
                }).error(function(result) {
                    notify.addErrorMessage('Anvendelse af systemet kunne ikke fjernes!');
                });
            }

            $scope.$watchCollection('pagination', function() {
                //var url = 'api/itSystem/?csv&skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take + '&organizationId=' + user.currentOrganizationId;

                //if ($scope.pagination.orderBy) {
                //    url += '&orderBy=' + $scope.pagination.orderBy;
                //    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                //}

                //if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                //else url += '&q=';

                //$scope.csvUrl = url;
                //loadSystems();
            });

            //function loadSystems() {
            //    var url = 'api/itSystem/?skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take + '&organizationId=' + user.currentOrganizationId;

            //    if ($scope.pagination.orderBy) {
            //        url += '&orderBy=' + $scope.pagination.orderBy;
            //        if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
            //    }

            //    if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
            //    else url += '&q=';

            //    $http.get(url).success(function(result, status, headers) {

            //        var paginationHeader = JSON.parse(headers('X-Pagination'));
            //        $scope.totalCount = paginationHeader.TotalCount;

            //        $scope.systems = [];
            //        _.each(result.response, function(system) {
            //            system.belongsTo = _.findWhere(organizations, { id: system.belongsToId });

            //            system.usageUrl = 'api/itsystemusage?itSystemId=' + system.id + '&organizationId=' + user.currentOrganizationId;

            //            loadUser(system);
            //            loadOrganization(system);
            //            loadTaskRef(system);
            //            loadUsage(system);

            //            system.addUsage = function() {
            //                addUsage(system);
            //            };

            //            system.deleteUsage = function() {
            //                deleteUsage(system);
            //            };

            //            $scope.systems.push(system);
            //        });
            //    });
            //}



            //Usage Details
            $scope.showUsageDetails = function (usageId, systemName) {
                //Filter by usageId
                $scope.usageGrid.dataSource.filter({ field: "ItSystemId", operator: "eq", value: usageId });
                //Set modal title
                $scope.modal.setOptions({ title: "Anvendelse af " + systemName });
                //Open modal
                $scope.modal.center().open();
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

            //usagedetails grid
            $scope.usageDetailsGrid = {
                    dataSource: {
                    type: "odata-v4",
                        transport:
                    {
                        read: {
                            url: "/odata/ItSystemUsages?$expand=Organization"
                        }
                    },
                    pageSize: 10,
                        serverPaging:
                    true,
                        serverSorting:
                    true,
                        serverFiltering:
                    true
                },
                columns: [
                    {
                        field: "Organization.Name",
                        title: "Organisation"
                    }
                ],
                dataBound: detailsBound
            };

            //catalog grid
            $scope.itSystemCatalogueGrid = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: "/odata/ItSystems?$expand=AppTypeOption,BusinessType,BelongsTo,Organization,ObjectOwner,Usages($expand=Organization)&$filter=OrganizationId eq " + user.currentOrganizationId  + " or AccessModifier eq Core.DomainModel.AccessModifier'1'"
                        }
                    },
                    pageSize: 10,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true
                },
                toolbar: [
                    { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
                    {
                        name: "clearFilter", text: "Ryd filtrering",
                        template: "<a class='k-button k-button-icontext' data-ng-click='clearOptions()'>#: data.text#</a>"
                    }
                ],
                excel: {
                    fileName: "IT System Katalog.xlsx",
                    filterable: false,
                    allPages: true
                },
                pageable: {
                    refresh: true,
                    pageSizes: true,
                    buttonCount: 5
                },
                sortable: true,
                reorderable: true,
                resizable: true,
                filterable: true,
                groupable: true,
                columnMenu: true,
                columns: [
                    //{ field: "Id", title: "ID", width: "40px" },
                    {
                        field: "Name", title: "It System",
                        // <a data-ui-sref="it-contract.edit.deadlines">
                        //template: function (data) { return '<a href="/#/system/edit/'+ data.Id +'/interfaces">' + data.Name + '</a>' }
                        template: '<a data-ui-sref="it-system.edit.interfaces({id: #:data.Id#})" data-ng-bind="dataItem.Name"></a>',
                    },
                    {
                        field: "AccessModifier", title: "(p)", width: 80
                    },
                    {
                        field: "AppTypeOption.Name", title: "Applikationstype", groupable: false,
                        //template: function (data) { return data.AppTypeOption ? data.AppTypeOption.Name : "" }
                        template: '<span data-ng-bind="dataItem.AppTypeOption.Name"></span>'
                    },
                    {
                        field: "BusinessType.Name", title: "Forretningtype", groupable: false,
                        //template: function (data) { return data.BusinessType ? data.BusinessType.Name : "" }
                        template: '<span data-ng-bind="dataItem.BusinessType.Name"></span>'
                    },
                    {
                        field: "TaskRefs.Id", title: "KLE ID", width: "70px", sortable: false, groupable: false,
                        //template: function (data) { return data.TaskRefs ? data.TaskRefs[0].Id : "" }
                        template: '<span data-ng-bind="dataItem.TaskRefs[0].Id"></span>',
                    },
                    {
                        field: "TaskRefs.Name", title: "KLE Navn", sortable: false, groupable: false,
                        //template: function (data) { return data.TaskRefs ? data.TaskRefs[0].Name : "" }
                        template: '<span data-ng-bind="dataItem.TaskRefs[0].Name"></span>'
                    },
                    {
                        field: "BelongsTo.Name", title: "Rettighedshaver",
                        //template: function (data) { return data.BelongsTo.Name }
                        template: '<span data-ng-bind="dataItem.BelongsTo.Name"></span>'
                    },
                    {
                        field: "Organization.Name", title: "Oprettet i",
                        //template: function(data) { return data.Organization.Name }
                        template: '<span data-ng-bind="dataItem.Organization.Name"></span>'
                    },
                    {
                        field: "ObjectOwner.Name", title: "Oprettet af",
                        //template: function (data) { return data.ObjectOwner.Name + " " + data.ObjectOwner.LastName }
                        template: '<span>{{ dataItem.ObjectOwner.Name + " " + dataItem.ObjectOwner.LastName }}</span>'
                    },
                    {
                        field: "Usages", title: "Anvender", width: 95, sortable: { sort: length },
                        template: '<a class="col-md-7 text-center" data-ng-click="showUsageDetails(#: data.Id#,\'#: data.Name#\')">#: data.Usages.length#</a>'
                        //command: { text: "Vis", click: showDetails }
                    },
                    {
                        title: "Anvendelse",
                        width: "110px",
                        field: "Usages", sortable: false, groupable: false, filterable: false, columnMenu: false,
                        //template: function(data) {
                        //    return _.find(data.Usages, function (d) { return d.OrganizationId == user.currentOrganizationId }) ?
                        //        '<button class="btn btn-danger col-md-7" data-ng-click="removeUsage(' + data.Id + ')">Fjern anv.</button>' :
                        //        '<button class="btn btn-success col-md-7" data-ng-click="enableUsage(' + data.Id + ')">Anvend</button>';
                        //},
                        template: '<button class="btn btn-success col-md-7" data-ng-click="enableUsage(#: data.Id#)" data-ng-show="!systemHasUsages(dataItem)">Anvend</button>' +
                                  '<button class="btn btn-danger  col-md-7" data-ng-click="removeUsage(#: data.Id#)" data-ng-show="systemHasUsages(dataItem)">Fjern anv.</button>'
                    },
                ],
                dataBound: onDataBound,
                columnResize: onDataBound,
                error: function(e) {
                    console.log(e);
                }
            };

            function test(e) {
                $scope.saveOptions();
            }

            function onDataBound(e) {
                if ($scope.mainGrid) $scope.saveOptions();
            }

            //Grid methods
            $scope.systemHasUsages = function(system) {
                return _.find(system.Usages, function (d) { return d.OrganizationId == user.currentOrganizationId });
            }

            $scope.enableUsage = function (dataItem) {
                addODataUsage(dataItem);
            }
            $scope.removeUsage = function (dataItem) {
                var sure = confirm("Er du sikker på at du vil fjerne den lokale anvendelse af systemet? Dette sletter ikke systemet, men vil slette alle lokale detaljer vedrørende anvendelsen.");

                if(sure) deleteODataUsage(dataItem);
            }

            //Grid state
            $scope.saveOptions = function() {
                localStorage["kendo-grid-it-system-catalog-options"] = kendo.stringify($scope.mainGrid.getOptions());
            };

            $scope.loadOptions = function() {
                var options = localStorage["kendo-grid-it-system-catalog-options"];
                if (options !== 'undefined') {
                    $scope.mainGrid.setOptions(JSON.parse(options));
                }
            }

            $scope.clearOptions = function () {
                localStorage["kendo-grid-it-system-catalog-options"] = undefined;
                $state.go($state.current, {}, { reload: true });
            }

            $scope.$on("kendoRendered", function (e) {
                $scope.loadOptions();
            });
        }
    ]);
})(angular, app);

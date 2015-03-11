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
        '$rootScope', '$scope', '$http', 'notify', '$state', 'organizations', 'user', '$timeout',
        function ($rootScope, $scope, $http, notify, $state, organizationsHttp, user, $timeout) {
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

            // show usageDetailsGrid
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
                            url: "/odata/ItSystemUsages?$expand=Organization",
                            dataType: "json"
                        },
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
                            url: "/odata/ItSystems?$expand=AppTypeOption,BusinessType,BelongsTo,TaskRefs,Parent,Organization,ObjectOwner,Usages($expand=Organization)",
                            dataType: "json"
                        },
                    },
                    schema: {
                        parse: function(data) {
                            $.each(data.value, function(i, elem) {
                                elem.AppTypeOption = elem.AppTypeOption ? elem.AppTypeOption : { Name: "" };
                                elem.BusinessType = elem.BusinessType ? elem.BusinessType : { Name: "" };
                                elem.UsagesLength = elem.Usages ? elem.Usages.length : 0;
                                elem.TaskRefs = elem.TaskRefs ? elem.TaskRefs : [{ TaskKey: "" }];
                            });
                            return data;
                        },
                    },
                    pageSize: 10,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                    filter: {
                        extra: false,
                        logic: "or",
                        filters: [
                            {
                                field: "OrganizationId",
                                operator: "eq",
                                value: user.currentOrganizationId
                            },
                            // TODO: implement this!
                            //{
                            //    field: "AccessModifier",
                            //    operator: "eq",
                            //    value: "Core.DomainModel.AccessModifier'1'"
                            //}
                        ]
                    } 
                },
                toolbar: [
                    { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
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
                sortable: {
                    mode: "single"
                },
                reorderable: true,
                resizable: true,
                filterable: true,
                groupable: false,
                columnMenu: true,
                columns: [
                    {
                        field: "Name", title: "It System",
                        template: '<a data-ui-sref="it-system.edit.interfaces({id: #:data.Id#})" data-ng-bind="dataItem.Name"></a>',
                    },
                    {
                        field: "AccessModifier", title: "(p)", width: 80, filterable: false
                    },
                    {
                        field: "AppTypeOption.Name", title: "Applikationstype",
                        template: '<span data-ng-bind="dataItem.AppTypeOption.Name"></span>'
                    },
                    {
                        field: "BusinessType.Name", title: "Forretningtype",
                        template: '<span data-ng-bind="dataItem.BusinessType.Name"></span>'
                    },
                    {
                        field: "TaskRefs", title: "KLE", width: 100, sortable: false,
                        template: '<span data-ng-bind="dataItem.TaskRefs[0].TaskKey"></span>'
                    },
                    {
                        field: "BelongsTo.Name", title: "Rettighedshaver",
                        template: '<span data-ng-bind="dataItem.BelongsTo.Name"></span>'
                    },
                    {
                        field: "Organization.Name", title: "Oprettet i",
                        template: '<span data-ng-bind="dataItem.Organization.Name"></span>'
                    },
                    {
                        field: "ObjectOwner.Name", title: "Oprettet af",
                        template: '<span>{{ dataItem.ObjectOwner.Name + " " + dataItem.ObjectOwner.LastName }}</span>'
                    },
                    {
                        field: "Usages.length" || 0, title: "Anvender", width: 95, sortable: false, filterable: false,
                        template: '<a class="col-md-7 text-center" data-ng-click="showUsageDetails(#: data.Id#,\'#: data.Name#\')">#: data.UsagesLength#</a>'
                    },
                    {
                        title: "Anvendelse",
                        width: "110px",
                        field: "Usages", sortable: false, filterable: false, columnMenu: false,
                        template: '<button class="btn btn-success col-md-7" data-ng-click="enableUsage(#: data.Id#)" data-ng-show="!systemHasUsages(dataItem)">Anvend</button>' +
                                  '<button class="btn btn-danger  col-md-7" data-ng-click="removeUsage(#: data.Id#)" data-ng-show="systemHasUsages(dataItem)">Fjern anv.</button>'
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

            // saves grid state to localStorage
            function saveGridOptions(e) {
                if ($scope.mainGrid) {
                    // timeout fixes columnReorder saves before the column is actually reordered 
                    // http://stackoverflow.com/questions/21270748/kendo-grid-saving-state-in-columnreorder-event
                    $timeout(function() {
                        localStorage["kendo-grid-it-system-catalog-options"] = kendo.stringify($scope.mainGrid.getOptions());
                    });
                }      
            }

            // returns bool if system is being used by system within current context
            $scope.systemHasUsages = function(system) {
                return _.find(system.Usages, function (d) { return d.OrganizationId == user.currentOrganizationId });
            }

            // adds usage at selected system within current context
            $scope.enableUsage = function (dataItem) {
                addODataUsage(dataItem);
            }

            // removes usage at selected system within current context
            $scope.removeUsage = function (dataItem) {
                var sure = confirm("Er du sikker på at du vil fjerne den lokale anvendelse af systemet? Dette sletter ikke systemet, men vil slette alle lokale detaljer vedrørende anvendelsen.");

                if(sure) deleteODataUsage(dataItem);
            }
            
            // loads
            $scope.loadOptions = function() {
                var options = localStorage["kendo-grid-it-system-catalog-options"];
                if (options) {
                    $scope.mainGrid.setOptions(JSON.parse(options));
                }
            }

            // clears grid filters by removing the localStorageItem and reloading the page
            $scope.clearOptions = function () {
                localStorage.removeItem("kendo-grid-it-system-catalog-options");
                $state.go($state.current, {}, { reload: true });
            }

            //
            $scope.$on("kendoRendered", function (e) {
                $scope.loadOptions();
            });
        }
    ]);
})(angular, app);

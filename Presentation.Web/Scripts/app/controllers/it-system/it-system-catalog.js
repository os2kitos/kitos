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

            $scope.pagination = {
                search: '',
                skip: 0,
                take: 20
            };
            
            $scope.csvUrl = 'api/itSystem/?csv&organizationId=' + user.currentOrganizationId;

            var organizations = organizationsHttp.data.response;

            function loadUser(system) {
                return $http.get('api/user/' + system.objectOwnerId, { cache: true })
                    .success(function(result) {
                        system.user = result.response;
                    });
            }

            function loadOrganization(system) {
                return $http.get('api/organization/' + system.organizationId, { cache: true })
                    .success(function(result) {
                        system.organization = result.response;
                    });
            }

            function loadTaskRef(system) {
                if (system.taskRefIds.length == 0) return null;

                return $http.get('api/taskref/' + system.taskRefIds[0])
                    .success(function(result) {
                        system.taskId = result.response.taskKey;
                        system.taskName = result.response.description;
                    });
            }

            function loadUsage(system) {
                return $http.get(system.usageUrl)
                    .success(function(result) {
                        system.hasUsage = true;
                        system.usage = result.response;
                    });
            }

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

            function addODataUsage(system) {
                return $http.post('api/itsystemusage', {
                    itSystemId: system.Id,
                    organizationId: user.currentOrganizationId
                }).success(function (result) {
                    notify.addSuccessMessage('Systemet er taget i anvendelse');
                    system.hasUsage = true;
                    system.usage = result.response;
                }).error(function (result) {
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

            $scope.$watchCollection('pagination', function() {
                //var url = 'api/itSystem/?csv&skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take + '&organizationId=' + user.currentOrganizationId;

                //if ($scope.pagination.orderBy) {
                //    url += '&orderBy=' + $scope.pagination.orderBy;
                //    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                //}

                //if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                //else url += '&q=';

                //$scope.csvUrl = url;
                loadSystems();
            });

            function loadSystems() {
                var url = 'api/itSystem/?skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take + '&organizationId=' + user.currentOrganizationId;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                else url += '&q=';

                $http.get(url).success(function(result, status, headers) {

                    var paginationHeader = JSON.parse(headers('X-Pagination'));
                    $scope.totalCount = paginationHeader.TotalCount;

                    $scope.systems = [];
                    _.each(result.response, function(system) {
                        system.belongsTo = _.findWhere(organizations, { id: system.belongsToId });

                        system.usageUrl = 'api/itsystemusage?itSystemId=' + system.id + '&organizationId=' + user.currentOrganizationId;

                        loadUser(system);
                        loadOrganization(system);
                        loadTaskRef(system);
                        loadUsage(system);

                        system.addUsage = function() {
                            addUsage(system);
                        };

                        system.deleteUsage = function() {
                            deleteUsage(system);
                        };

                        $scope.systems.push(system);
                    });
                });
            }

            //KENDO
            $scope.itSystemCatalogueGrid = {
                dataSource: {
                    type: "odata-v4", // IMPORTANT!! "odata" != "odata-v4" https://github.com/telerik/ui-for-aspnet-mvc-examples/blob/master/grid/odata-v4-web-api-binding-wrappers/KendoUIMVC5/Views/Home/Index.cshtml
                    transport: {
                        read: {
                            url: "/odata/ItSystems?$expand=AppTypeOption,BusinessType,BelongsTo,Organization,ObjectOwner,Usages"
                        }
                    },
                    pageSize: 5,
                    serverPaging: true,
                    serverSorting: true,
                },
                sortable: true,
                groupable: {
                    messages: {
                        empty: "Drag a column header and drop it here to group by that column"
                    }
                },
                pageable: true,
                reorderable: true,
                resizable: true,
                columnMenu: true,
                columns: [
                    //{ field: "Id", title: "ID", width: "40px" },
                    { field: "Name", title: "It System" },
                    { field: "AccessModifier", title: "(P)", width: "80px" },
                    {
                        field: "AppTypeOption", title: "Applikationstype",
                        template: function (data) { return data.AppTypeOption ? data.AppTypeOption.Name : "" }
                    },
                    {
                        field: "BusinessType", title: "Forretningtype",
                        template: function (data) { return data.BusinessType ? data.BusinessType.Name : "" }
                    },
                    {
                        field: "TaskRefs", title: "KLE ID", width: "70px",
                        template: function (data) { return data.TaskRefs ? data.TaskRefs[0].Id : ""  }
                    },
                    {
                        field: "TaskRefs", title: "KLE Navn",
                        template: function (data) { return data.TaskRefs ? data.TaskRefs[0].Navn : "" }
                    },
                    {
                        field: "BelongsTo", title: "Rettighedshaver",
                        template: function (data) { return data.BelongsTo.Name }
                    },
                    {
                        field: "Organization", title: "Oprettet i",
                        template: function(data) { return data.Organization.Name }
                    },
                    {
                        field: "ObjectOwner", title: "Oprettet af",
                        template: function (data) { return data.ObjectOwner.Name + " " + data.ObjectOwner.LastName }
                    },
                    {
                        field: "Usage",
                        command: 1 == 1 ? {
                            text: "Anvend",
                            click: test
                        } : {
                            text: "Fjern anv.",
                            click: test
                        },
                        title: "Anvendelse",
                        width: "95px"
                    }
                ],
                error: function(e) {
                    console.log(e);
                }
            };
            //KENDO SLUT

            function test(e) {
                e.preventDefault();

                var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
                addODataUsage(dataItem);
            }
        }
    ]);
})(angular, app);

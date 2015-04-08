(function (ng, app) {
    app.config([
        '$stateProvider', function($stateProvider) {
            $stateProvider.state('it-system.interfaceCatalog', {
                url: '/interface-catalog',
                templateUrl: 'partials/it-system/it-interface-catalog.html',
                controller: 'system.interfaceCatalogCtrl'
            });
        }
    ]);

    app.controller('system.interfaceCatalogCtrl',
    [
        '$rootScope', '$scope',
        function ($rootScope, $scope) {
            $rootScope.page.title = 'Snitflade - Katalog';

            $scope.itInterfaceOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: "/odata/ItInterfaces?$expand=Interface,ObjectOwner,BelongsTo,Organization,Tsa,ExhibitedBy($expand=ItSystem),Method"
                        }
                    },
                    pageSize: 5,
                    serverPaging: true,
                    serverSorting: true
                },
                toolbar: [
                { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
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
                columnMenu: true,
                reorderable: true,
                resizable: true,
                columns: [
                    {
                        field: "ItInterfaceId", title: "Snidtflade ID"
                    },
                    {
                        field: "Name", title: "Snitflade",
                        template: "<a data-ui-sref='it-system.interface-edit.interface-details({id: #: data.Id#})' data-ng-bind='dataItem.Name'></a>"
                    },
                    {
                        field: "AccessModifier", title: "Tilgængelighed", width: 125
                    },
                    {
                        field: "InterfaceType.Name", title: "Snitfladetype",
                        template: "<span data-ng-bind='dataItem.InterfaceType.Name'></span>"
                    },
                    {
                        field: "Interface.Name", title: "Grænseflade",
                        template: "<span data-ng-bind='dataItem.Interface.Name'></span>"
                    },
                    {
                        field: "Method", title: "Metode", sortable: false,
                        template: "<span data-ng-bind='dataItem.Method.Name'></span>"
                    },
                    {
                        field: "TSA", title: "TSA", sortable: false,
                        template: "<span data-ng-bind='dataItem.Tsa.Name'></span>"
                    },
                    {
                        field: "ExhibitedBy", title: "Udstillet af", sortable: false,
                        template: "<span data-ng-bind='dataItem.ExhibitedBy.ItSystem.Name'></span>"
                    },
                    {
                        field: "BelongsTo.Name", title: "Rettighedshaver"
                    },
                    {
                        field: "Organization.Name", title: "Oprettet i"
                    },
                    {
                        field: "ObjectOwner.Name", title: "Oprettet af",
                        template: "<span>{{dataItem.ObjectOwner.Name + ' ' + dataItem.ObjectOwner.LastName}}</span>"
                    }
                ],
                error: function (e) {
                    console.log(e);
                }
            };
        }
    ]);
})(angular, app);

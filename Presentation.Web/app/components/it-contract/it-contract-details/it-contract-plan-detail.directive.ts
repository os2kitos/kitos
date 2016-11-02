module Kitos.Contract.Details {
    "use strict";

    // TODO modify to fit needs
    interface IDirectiveScope extends ng.IScope {
        detail: string,
        contractId: number,
        mainGrid: IKendoGrid<>;
        mainGridOptions: IKendoGridOptions<>;
    }

    class ContractDetails implements ng.IDirective {
        public templateUrl = "app/components/it-contract/it-contract-details/it-contract-details.view.html";
        public scope = {
            detail: "@",
            contractId: "@"
        };

        public static directiveName = "contractDetails";

        public link(scope: IDirectiveScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
            scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: () => ``,
                            dataType: "json"
                        },
                    },
                    sort: {
                        field: "ObjectId",
                        dir: "asc"
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                },
                noRecords: true,
                messages: {
                    noRecords: `Systemet har ingen ${scope.detail} tilknyttet.`
                },
                columns: [
                    {
                        field: "Object.Name", title: `Antallet af ${scope.detail}`, width: 150,
                        persistId: "numberOf", // DON'T YOU DARE RENAME!
                        template: (dataItem) => dataItem.Object.Name,
                        excelTemplate: (dataItem) => dataItem.Object.Name,
                        hidden: false,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                ],
            };
        }
    }

    angular.module("app")
        .directive(ContractDetails.directiveName, DirectiveFactory.getFactoryFor(ContractDetails));
}
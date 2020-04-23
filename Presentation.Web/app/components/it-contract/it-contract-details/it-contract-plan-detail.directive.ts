module Kitos.Contract.Details {
    "use strict";

    interface IDirectiveScope extends ng.IScope {
        detailType: string;
        action: string;
        fieldValue: string;
        odataQuery: string;
        detailModelType: string;
        mainGrid: IKendoGrid<any>;
        mainGridOptions: IKendoGridOptions<any>;
    }

    class ContractDetails implements ng.IDirective {
        public templateUrl = "app/components/it-contract/it-contract-details/it-contract-details.view.html";
        public scope = {
            detailType: "@",
            action: "@",
            fieldValue: "@",
            odataQuery: "@",
            detailModelType: "@"
        };

        public static directiveName = "contractDetails";

        public link(scope: IDirectiveScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
            scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: () => `${scope.odataQuery}`,
                            dataType: "json"
                        }
                    },
                    sort: {
                        field: "Id",
                        dir: "asc"
                    }
                },
                noRecords: true,
                messages: {
                    noRecords: `Kontrakten ${scope.action} ingen ${scope.detailType}.`
                },
                columns: [
                    {
                        persistId: "fieldValue", // DON'T YOU DARE RENAME!
                        field: `${scope.fieldValue}`,
                        title: `Kontrakten ${scope.action} følgende ${scope.detailType}`, width: 150,
                        template: dataItem => {
                            if (dataItem[scope.detailModelType].Disabled)
                                return dataItem[scope.detailModelType].Name + " (Slettes)";
                            else
                                return dataItem[scope.detailModelType].Name;
                        },
                        hidden: false
                    }
                ]
            };
        }
    }

    angular.module("app")
        .directive(ContractDetails.directiveName, DirectiveFactory.getFactoryFor(ContractDetails));
}
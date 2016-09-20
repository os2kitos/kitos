module Kitos.Organization.Users {
    "use strict";

    interface IDirectiveScope extends ng.IScope {
        userId: number;
        currentOrganizationId: number;
        mainGrid: IKendoGrid<IGridModel>;
        mainGridOptions: IKendoGridOptions<IGridModel>;
    }

    interface IGridModel extends Models.ItSystem.IItSystemRight {

    }

    class UserSystemRights implements ng.IDirective {
        public templateUrl = "app/components/org/user/user-rights-details/user-rights.view.html";
        public scope = {
            userId: "@",
            currentOrganizationId: "@"
        };

        public static directiveName = "userSystemRoles";

        public link(scope: IDirectiveScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
            scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: () => `/odata/Users(${scope.userId})/ItSystemRights?$filter=Object/OrganizationId eq ${scope.currentOrganizationId}&$expand=Object($expand=ItSystem($select=Name)),Role($select=Name,HasWriteAccess)`,
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
                    noRecords: "Ingen system roller tilknyttet"
                },
                columns: [
                    {
                        field: "Object.ItSystem.Name", title: "IT System", width: 150,
                        persistId: "systemname", // DON'T YOU DARE RENAME!
                        template: (dataItem) => dataItem.Object.ItSystem.Name,
                        excelTemplate: (dataItem) => dataItem.Object.ItSystem.Name,
                        hidden: false,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Role.Name", title: "Rolle", width: 150,
                        persistId: "systemrole", // DON'T YOU DARE RENAME!
                        template: (dataItem) => dataItem.Role.Name,
                        excelTemplate: (dataItem) => dataItem.Role.Name,
                        hidden: false,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Role.HasWriteAccess", title: "Skrive", width: 150,
                        persistId: "systemroleaccess", // DON'T YOU DARE RENAME!
                        template: (dataItem) => dataItem.Role.HasWriteAccess ? `<span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span>` : `<span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span>`,
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
        .directive(UserSystemRights.directiveName, DirectiveFactory.getFactoryFor(UserSystemRights));
}

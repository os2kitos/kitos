module Kitos.Organization.Users {
    "use strict";

    interface IDirectiveScope extends ng.IScope {
        userId: number;
        currentOrganizationId: number;
        mainGrid: IKendoGrid<IGridModel>;
        mainGridOptions: IKendoGridOptions<IGridModel>;
    }

    interface IGridModel extends Models.Organization.IOrganizationUnitRight {

    }

    class UserOrganizationUnitRights implements ng.IDirective {
        public templateUrl = "app/components/org/user/user-rights-details/user-rights.view.html";
        public scope = {
            userId: "@",
            currentOrganizationId: "@"
        };

        public static directiveName = "userOrganizationUnitRoles";

        public link(scope: IDirectiveScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
            scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: () => `/odata/Users(${scope.userId})/OrganizationUnitRights?$filter=Object/OrganizationId eq ${scope.currentOrganizationId}&$expand=Object($select=Name),Role($select=Name,HasWriteAccess)`,
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
                    noRecords: "Ingen organisationenheds roller tilknyttet"
                },
                columns: [
                    {
                        field: "Object.Name", title: "Organisationsenhed", width: 150,
                        persistId: "orgunitname", // DON'T YOU DARE RENAME!
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
                    {
                        field: "Role.Name", title: "Rolle", width: 150,
                        persistId: "orgunitrole", // DON'T YOU DARE RENAME!
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
                        persistId: "orgunitroleaccess", // DON'T YOU DARE RENAME!
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
        .directive(UserOrganizationUnitRights.directiveName, DirectiveFactory.getFactoryFor(UserOrganizationUnitRights));
}

module Kitos.Services {
    "use strict";

    interface IOrgUnitRoleModel {
        Id: number;
        HasReadAccess: boolean;
        HasWriteAccess: boolean;
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Description?: any;
        ObjectOwnerId: number;
        LastChanged: Date;
        LastChangedByUserId: number;
    }

    interface IOrgRightsModel {
        Id: number;
        UserId: number;
        RoleId: number;
        ObjectId: number;
        ObjectOwnerId: number;
        LastChanged: Date;
        LastChangedByUserId: number;
    }

    export class OrganizationService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        public addRole(organizationId: number, user: Services.IUser, role: Models.OrganizationRole): ng.IHttpPromise<Models.IOrganizationRight> {
            var rightsPayload: Models.IOrganizationRight = {
                UserId: user.id,
                ObjectOwner: user,
                ObjectOwnerId: user.id,
                Role: role
            };

            return this.$http.post<Models.IOrganizationRight>(`odata/Organizations(${organizationId})/Rights`, rightsPayload);
        }

        GetOrganisationRightsById = (id: number) => {
            return this.$http.get<Models.IOrganizationRight>(`odata/Organizations(${id})/Rights`);
        }

        GetAllOrganizationUnitRoles = () => {
            return this.$http.get<IOrgUnitRoleModel>(`odata/OrganizationUnitRoles`);
        }

        GetOrganizationUnitRightsById = (id: number) => {
            return this.$http.get<IOrgRightsModel>(`odata/OrganizationUnitRights?$filter=UserId eq (${id})`);
        }

        GetOrganizationUnitById = () => {
            return this.$http.get<Models.IOrganizationUnit>(`odata/OrganizationUnits`);
        }
    }

    app.service("organizationService", OrganizationService);
}

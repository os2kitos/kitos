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
        
        GetOrganizationUnitDataById = (id: number, orgId: number) => {
            return this.$http.get(`odata/OrganizationUnitRights?$expand=role,object&$filter=UserId eq (${id} )AND Object/OrganizationId eq (${orgId})`);
        }

        GetOrganizationData = (userId: number, organizationId: number) => {
            return this.$http.get(`odata/OrganizationRights?$expand=Organization&$filter=UserId eq (${userId}) AND OrganizationId eq (${organizationId})`);
        }

        DeleteOrganizationAdminData = (user: any, adminRole: string, module: string) => {
            var localRight = user.OrganizationRights.filter(x => x.Role === adminRole);
            var localId = localRight[0].Id;
            return this.$http.delete(`/odata/${module}(${localId})`);
        }
    }

    app.service("organizationService", OrganizationService);
}

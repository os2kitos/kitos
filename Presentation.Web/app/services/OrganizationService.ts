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
        
        GetOrganizationUnitDataById = (id: number) => {
            return this.$http.get(`odata/OrganizationUnitRights?$expand=role,object&$filter=UserId eq (${id})`);
        }
    }

    app.service("organizationService", OrganizationService);
}

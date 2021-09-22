module Kitos.Services {
    "use strict";

    interface ISystemRoleModel {
        Id: number;
        HasReadAccess: boolean;
        HasWriteAccess: boolean;
        Name: string;
        IsActive: boolean;
        Description?: any;
        ObjectOwnerId: number;
        LastChanged: Date;
        LastChangedByUserId: number;
    }

    interface ISystemRightsModel {
        Id: number;
        UserId: number;
        RoleId: number;
        ObjectId: number;
        ObjectOwnerId: number;
        LastChanged: Date;
        LastChangedByUserId: number;
    }

    export class ItSystemService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        GetSystemById = (id: number) => {
            return this.$http.get<Models.ItSystem.IItSystem>(`odata/ItSystems(${id})`);
        }

        GetAllSystems = () => {
            return this.$http.get<Models.ItSystem.IItSystem>(`odata/ItSystems`);
        }

        GetSystemRoleById = (roleId: number) => {
            return this.$http.get<ISystemRoleModel>(`odata/ItSystemRoles(${roleId})`);
        }

        GetAllSystemRoles = () => {
            return this.$http.get<ISystemRoleModel>(`odata/ItSystemRoles`);
        }

        GetSystemRightsById = (id: number) => {
            return this.$http.get<ISystemRightsModel>(`odata/ItSystemRights?$filter=UserId eq (${id})`);
        }

        GetSystemDataById = (id: number) => {
            return this.$http.get(`odata/ItSystemRights?$expand=role,object&$filter=UserId eq (${id})`);
        }

        GetSystemDataByIdFiltered = (id: number, orgId: number) => {
            return this.$http
                .get(`odata/ItSystemRights?$expand=role($select=Name),object($select=Id;$expand=ItSystem($select=Id,Name))&$filter=Object/OrganizationId eq (${orgId}) AND UserId eq (${id})&$select=Id`);
        }
    }

    app.service("ItSystemService", ItSystemService);
}

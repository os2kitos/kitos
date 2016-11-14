module Kitos.Services {
    "use strict";

    interface IProjectRoleModel {
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

    interface IProjectRightsModel {
        Id: number;
        UserId: number;
        RoleId: number;
        ObjectId: number;
        ObjectOwnerId: number;
        LastChanged: Date;
        LastChangedByUserId: number;
    }

    //laver kaldet til odata for at hente data kollektions ud af databasen
    export class ItProjectService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        GetProjectById = (id: number) => {
            return this.$http.get<Models.ItProject.IItProject>(`odata/ItProjects(${id})`);
        }

        GetAllProjects = () => {
            return this.$http.get<Models.ItProject.IItProject>(`odata/ItProjects`);
        }

        GetProjectRoleById = (roleId: number) => {
            return this.$http.get<IProjectRoleModel>(`odata/ItProjectRoles(${roleId})`);
        }

        GetAllProjectRoles = () => {
            return this.$http.get<IProjectRoleModel>(`odata/ItProjectRoles`);
        }

        GetProjectRightsById = (id: number) => {
            return this.$http.get<IProjectRightsModel>(`odata/ItProjectRights?$filter=UserId eq (${id})`);
        }

        GetProjectDataById = (id: number) => {
            return this.$http.get(`odata/ItProjectRights?$expand=role,object&$filter=UserId eq (${id})`);
        }
    }

    app.service("ItProjectService", ItProjectService);
}

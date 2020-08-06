module Kitos.Services {
    "use strict";

    export class ItContractsService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        GetItContractById = (id: number) => {
            return this.$http.get(`odata/ItContracts(${id})`);
        }

        GetItContractRoleById = (roleId: number) => {
            return this.$http.get(`odata/ItContractRoles(${roleId})`);
        }

        GetAllItContractRoles = () => {
            return this.$http.get(`odata/ItContractRoles`);
        }

        GetItContractRightsById = (id: number) => {
            return this.$http.get(`odata/ItContractRights?$filter=UserId eq (${id})`);
        }

        GetContractDataById = (id: number, orgId: number) => {
            return this.$http.get(`odata/ItContractRights?$expand=role,object&$filter=UserId eq (${id}) AND Object/OrganizationId eq (${orgId})`);
        }
    }

    app.service("ItContractsService", ItContractsService);
}

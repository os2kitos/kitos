module Kitos.Services {
    "use strict";

    export class ItContractsService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        GetItContractById = (id: number) => {
            return this.$http.get(`odata/ItContracts(${id})`);
        }

        GetAllItContracts = () => {
            return this.$http.get(`odata/ItContracts`);
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
    }

    app.service("ItContractsService", ItContractsService);
}

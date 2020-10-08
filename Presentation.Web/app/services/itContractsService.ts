module Kitos.Services.Contract {

    export interface IItContractsService {
        assignDataProcessingRegistration(contractId: number, dataProcessingRegistrationId: number): angular.IPromise<IContractPatchResult>;
        removeDataProcessingRegistration(contractId: number, dataProcessingRegistrationId: number): angular.IPromise<IContractPatchResult>;
        getAvailableDataProcessingRegistrations(contractId: number, query: string): angular.IPromise<Models.Generic.NamedEntity.NamedEntityDTO[]>;
    }

    export interface IContractPatchResult {
        valueModifiedTo: any;
    }

    export class ItContractsService implements IItContractsService{

        private handleServerError(error) {
            console.log("Request failed with:", error);
            let errorCategory: Models.Api.ApiResponseErrorCategory;
            switch (error.status) {
                case 400:
                    errorCategory = Models.Api.ApiResponseErrorCategory.BadInput;
                    break;
                case 404:
                    errorCategory = Models.Api.ApiResponseErrorCategory.NotFound;
                    break;
                case 409:
                    errorCategory = Models.Api.ApiResponseErrorCategory.Conflict;
                    break;
                case 500:
                    errorCategory = Models.Api.ApiResponseErrorCategory.ServerError;
                    break;
                default:
                    errorCategory = Models.Api.ApiResponseErrorCategory.UnknownError;
            }
            throw errorCategory;
        }

        //Use for contracts that take an input defined as SingleValueDTO
        private simplePatch(url: string, value: any): angular.IPromise<IContractPatchResult> {

            const payload = {
                Value: value
            };

            return this
                .$http
                .patch<API.Models.IApiWrapper<any>>(url, payload)
                .then(
                    response => {
                        var res = response.data as { response: Models.ItContract.IItContractDTO };
                        return <IContractPatchResult>{
                            valueModifiedTo: value,
                            optionalServerDataPush: res.response
                        };
                    },
                    error => this.handleServerError(error)
                );
        }

        assignDataProcessingRegistration(contractId: number, dataProcessingRegistrationId: number): ng.IPromise<IContractPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(contractId, "data-processing-registration/assign"), dataProcessingRegistrationId);
        }
        removeDataProcessingRegistration(contractId: number, dataProcessingRegistrationId: number): ng.IPromise<IContractPatchResult> {

            return this.simplePatch(this.getUriWithIdAndSuffix(contractId, "data-processing-registration/remove"), dataProcessingRegistrationId);
        }

        getAvailableDataProcessingRegistrations(contractId: number, query: string): ng.IPromise<Models.Generic.NamedEntity.NamedEntityDTO[]> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(this.getUriWithIdAndSuffix(contractId, `data-processing-registration/available?nameQuery=${query}`))
                .then(
                    result => {
                        var response = result.data as { response: Models.Generic.NamedEntity.NamedEntityDTO[] }
                        return response.response;
                    },
                    error => this.handleServerError(error)
                );
        }


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

        private getUriWithIdAndSuffix(id: number, suffix: string) {
            return this.getBaseUri() + `${id}/${suffix}`;
        }

        private getBaseUri() {
            return "api/itcontract/";
        }

    }

    app.service("ItContractsService", ItContractsService);
}

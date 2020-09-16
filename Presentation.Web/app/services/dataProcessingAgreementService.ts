module Kitos.Services.DataProcessing {
    export interface IDataProcessingAgreementService {
        create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult>;
        delete(dataProcessingAgreementId: number): angular.IPromise<IDataProcessingAgreementDeletedResult>;
        rename(dataProcessingAgreementId: number, name: string): angular.IPromise<IDataProcessingAgreementPatchResult>;
        get(dataProcessingAgreementId: number): angular.IPromise<Models.DataProcessing.IDataProcessingAgreementDTO>;
        getAvailableRoles(dataProcessingAgreementId: number): angular.IPromise<Models.DataProcessing.IDataProcessingRoleDTO[]>;
        getApplicableUsers(dataProcessingAgreementId: number, roleId: number, nameOrEmailContent: string): angular.IPromise<Models.DataProcessing.ISimpleUserDTO[]>;
        assignNewRole(dataProcessingAgreementId: number, roleId: number, userId: number): angular.IPromise<void>;
        removeRole(dataProcessingAgreementId: number, roleId: number, userId: number): angular.IPromise<void>;
    }

    export interface IDataProcessingAgreementCreatedResult {
        createdObjectId: number;
    }


    export interface IDataProcessingAgreementDeletedResult {
        deletedObjectId: number;
    }

    export interface IDataProcessingAgreementPatchResult {
        valueModifiedTo: string;
    }

    export class DataProcessingAgreementService implements IDataProcessingAgreementService {

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

        rename(dataProcessingAgreementId: number, name: string): angular.IPromise<IDataProcessingAgreementPatchResult> {

            const payload = {
                Value: name
            };

            return this
                .$http
                .patch<API.Models.IApiWrapper<any>>(this.getUriWithIdAndSuffix(dataProcessingAgreementId.toString(), "name"), payload)
                .then(
                    response => {
                        return <IDataProcessingAgreementPatchResult>{
                            valueModifiedTo: name,
                        };
                    },
                    error => this.handleServerError(error)
                );
        }

        delete(dataProcessingAgreementId: number): angular.IPromise<IDataProcessingAgreementDeletedResult> {

            return this
                .$http
                .delete<API.Models.IApiWrapper<any>>(this.getUri(dataProcessingAgreementId.toString()))
                .then(
                    response => {
                        return <IDataProcessingAgreementDeletedResult>{
                            deletedObjectId: response.data.response.id,
                        };
                    },
                    error => this.handleServerError(error)
                );

        }

        create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult> {
            const payload = {
                name: name,
                organizationId: organizationId
            };
            return this
                .$http
                .post<API.Models.IApiWrapper<any>>(this.getUri(""), payload)
                .then(
                    response => {
                        return <IDataProcessingAgreementCreatedResult>{
                            createdObjectId: response.data.response.id
                        };
                    },
                    error => this.handleServerError(error)
                );
        }

        get(dataProcessingAgreementId: number): angular.IPromise<Models.DataProcessing.IDataProcessingAgreementDTO> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(this.getUri(dataProcessingAgreementId.toString()))
                .then(
                    result => {
                        var response = result.data as { response: Models.DataProcessing.IDataProcessingAgreementDTO}
                        return response.response;
                    },
                    error => this.handleServerError(error)
                );
        }

        getAvailableRoles(dataProcessingAgreementId: number): angular.IPromise<Models.DataProcessing.IDataProcessingRoleDTO[]> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(this.getUriWithIdAndSuffix(dataProcessingAgreementId.toString(),
                    "available-roles"))
                .then(
                    result => {
                        var response = result.data as { response: Models.DataProcessing.IDataProcessingRoleDTO[] }
                        return response.response;
                    },
                    error => this.handleServerError(error)
                    );
        }

        getApplicableUsers(dataProcessingAgreementId: number, roleId: number, nameOrEmailContent: string): angular.IPromise<Models.DataProcessing.ISimpleUserDTO[]> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(this.getUri(`${dataProcessingAgreementId}/available-roles/${roleId}/applicable-users?nameOrEmailContent=${nameOrEmailContent}`))
                .then(
                    result => {
                        var response = result.data as { response: Models.DataProcessing.ISimpleUserDTO[] }
                        return response.response;
                    },
                    error => this.handleServerError(error)
            );
        }

        assignNewRole(dataProcessingAgreementId: number, roleId: number, userId: number): angular.IPromise<void> {
            const payload = {
                RoleId: roleId,
                UserId: userId
            };
            return this
                .$http
                .patch<API.Models.IApiWrapper<any>>(
                    this.getUriWithIdAndSuffix(dataProcessingAgreementId.toString(), "roles/assign"),
                    payload)
                .then(
                    result => {},
                    error => this.handleServerError(error)
                );
        }

        removeRole(dataProcessingAgreementId: number, roleId: number, userId: number): angular.IPromise<void> {
            const payload = {
            };
            return this
                .$http
                .patch<API.Models.IApiWrapper<any>>(
                    this.getUriWithIdAndSuffix(dataProcessingAgreementId.toString(), `roles/remove/${roleId}/from/${userId}`),
                    payload)
                .then(
                    result => { },
                    error => this.handleServerError(error)
                );
        }

        private mapToSelect2Option(input: Models.DataProcessing.ISimpleUserDTO): Models.ViewModel.Generic.Select2OptionViewModel {
            return { id: input.id, text: input.name };
        }

        static $inject = ["$http"];

        constructor(private readonly $http: ng.IHttpService) {
        }

        private getUri(suffix: string): string {
            return this.getBaseUri() + `${suffix}`;
        }

        private getUriWithIdAndSuffix(id: string, suffix: string) {
            return this.getBaseUri() + `${id}/${suffix}`;
        }

        private getBaseUri() {
            return "api/v1/data-processing-agreement/";
        }
    }

    app.service("dataProcessingAgreementService", DataProcessingAgreementService);
}
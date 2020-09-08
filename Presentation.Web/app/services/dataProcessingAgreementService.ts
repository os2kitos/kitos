module Kitos.Services.DataProcessing {
    import IApiWrapper = API.Models.IApiWrapper;

    export interface IDataProcessingAgreementService {
        create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult>;
        delete(dataProcessingAgreementId: number): angular.IPromise<IDataProcessingAgreementDeletedResult>;
        rename(dataProcessingAgreementId: number, name: string): angular.IPromise<IDataProcessingAgreementPatchResult>;
    }

    export interface IDataProcessingAgreementCreatedResult {
        created: boolean;
        createdObjectId : number;
        error: string;
    }


    export interface IDataProcessingAgreementDeletedResult {
        deleted: boolean;
        deletedObjectId: number;
        error: string;
    }

    export interface IDataProcessingAgreementPatchResult {
        modified: boolean;
        valueModified: string;
        error: string;
    }

    export class DataProcessingAgreementService implements IDataProcessingAgreementService {


        public rename(dataProcessingAgreementId: number, name: string): angular.IPromise<IDataProcessingAgreementPatchResult> { ;

            const payload  = {
                Value: name
            };

            return this
                .$http
                .patch<IApiWrapper<any>>(this.getUriWithIdAndSuffix(dataProcessingAgreementId.toString(), "name"), payload).then(
                    response => {
                    return <IDataProcessingAgreementPatchResult>{
                        modified: true,
                        valueModified: name,
                        error: ""
                    };
                },
                error => {
                    return <IDataProcessingAgreementPatchResult>{
                        modified: false,
                        valueModified: name,
                        error: error.data.msg
                };
                }
            );
        }

        public delete(dataProcessingAgreementId: number): angular.IPromise<IDataProcessingAgreementDeletedResult> {

            return this
                .$http
                .delete<IApiWrapper<any>>(this.getUri(dataProcessingAgreementId.toString()))
                .then(
                    response => {
                        return <IDataProcessingAgreementDeletedResult>{
                            deleted: true,
                            deletedObjectId: response.data.response.id,
                            error: "TODO"
                        };
                    },
                    error => {
                        return <IDataProcessingAgreementDeletedResult>{
                            deleted: false,
                            error: "TODO"
                        };
                    }
                );

        }

        public create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult> {
            const payload = {
                name: name,
                organizationId: organizationId
            };
            return this
                .$http
                .post<IApiWrapper<any>>(this.getUri(""), payload)
                .then(
                    response => {
                        return <IDataProcessingAgreementCreatedResult>{
                            created: true,
                            createdObjectId: response.data.response.id,
                            error: "TODO"
                        };
                    },
                    error => {
                        return <IDataProcessingAgreementCreatedResult>{
                            created: false,
                            error: "TODO"
                        };
                    }
                );
        }

        static $inject = ["$http", "notify"];

        constructor(private readonly $http: ng.IHttpService, private notify) {
        }

        private getUri(suffix: string) : string {
            return this.getBaseUri() + `${suffix}`;
        }

        private getUriWithIdAndSuffix(id: string, suffix: string) {
            return this.getBaseUri() + `${id}/${suffix}`;
        }

        private getBaseUri() {
            return "api/v1/data-processing-agreement/";
        }
    }

    app.service("dataProcessingAgreementService", Kitos.Services.DataProcessing.DataProcessingAgreementService);
}
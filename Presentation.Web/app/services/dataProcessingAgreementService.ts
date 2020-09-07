module Kitos.Services.DataProcessing {
    import IApiWrapper = API.Models.IApiWrapper;
    import ApiResponseErrorCategory = Models.Api.ApiResponseErrorCategory;

    export interface IDataProcessingAgreementService {
        create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult>;
        //TODO: Extend with type safe methods for getting and changing data
    }

    export interface IDataProcessingAgreementCreatedResult {
        createdObjectId : number;
    }

    export class DataProcessingAgreementService implements IDataProcessingAgreementService {
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
                            createdObjectId: response.data.response.id
                        };
                    },
                    error => {
                        var errorCategory : ApiResponseErrorCategory;
                        switch (error.status) {
                            case 400:
                                errorCategory = ApiResponseErrorCategory.BadInput;
                                break;
                            case 409:
                                errorCategory = ApiResponseErrorCategory.Conflict;
                                break;
                            case 500:
                                errorCategory = ApiResponseErrorCategory.ServerError;
                                break;
                            default:
                                errorCategory = ApiResponseErrorCategory.UnknownError;
                        }
                        throw errorCategory;
                    }
                );
        }

        static $inject = ["$http", "notify"];

        constructor(private readonly $http: ng.IHttpService, private notify) {
        }

        private getUri(suffix: string) : string {
            return `api/v1/data-processing-agreement/${suffix}`;
        }
    }

    app.service("dataProcessingAgreementService", Kitos.Services.DataProcessing.DataProcessingAgreementService);
}
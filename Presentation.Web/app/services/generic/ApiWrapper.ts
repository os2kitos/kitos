module Kitos.Services.Generic {

    /*
     * Base wrapper for API clients which interact with api backends using the v1 REST
     */
    export class ApiWrapper {

        public constructor(private readonly httpService: ng.IHttpService) {

        }

        getDataFromUrl<TResponse>(url: string) {
            return this
                .httpService
                .get<API.Models.IApiWrapper<any>>(url)
                .then(
                    result => {
                        var response = result.data as { response: TResponse }
                        return response.response;
                    },
                    error => this.handleServerError(error)
                );
        }

        delete(url: string) : ng.IPromise<boolean> {
            return this
                .httpService
                .delete(url)
                .then(
                    result => {
                        return true;
                    },
                    error => {
                        console.log("Error deleting ", url, error);
                        return false;
                    }
                );
        }

        handleServerError(error) {
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
    }
}
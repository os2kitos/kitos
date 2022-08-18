module Kitos.Services.Generic {

    /*
     * Base wrapper for API clients which interact with api backends using the v1 REST
     */
    export class ApiWrapper {

        static $inject = ["$http"];
        public constructor(private readonly $http: ng.IHttpService) {

        }

        getDataFromUrl<TResponse>(url: string) {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(url)
                .then(
                    result => {
                        var response = result.data as { response: TResponse }
                        return response.response;
                    },
                    error => this.handleServerError(error)
                );
        }

        delete(url: string, payload?: any): ng.IPromise<boolean> {
            let config: ng.IRequestShortcutConfig;
            if (payload) {
                config = {
                    data: payload,
                    headers: {
                        "content-type": "application/json"
                    }
                }
            }
            return this
                .$http
                .delete(url, config)
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

        post<TResponse>(url: string, payload?: any): ng.IPromise<TResponse> {
            return this
                .$http
                .post(url, payload ?? {})
                .then(
                    result => {
                        var response = result.data as { response: TResponse }
                        return response.response;
                    },
                    error => this.handleServerError(error)
                );
        }

        put(url: string, payload?: any): ng.IPromise<void> {
            return this
                .$http
                .put(url, payload ?? {})
                .then(_ => { }, error => this.handleServerError(error));
        }

        patch(url: string, payload?: any): ng.IPromise<void> {
            return this
                .$http
                .patch(url, payload ?? {})
                .then(_ => { }, error => this.handleServerError(error));
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

    app.service("genericApiWrapper", ApiWrapper);

}
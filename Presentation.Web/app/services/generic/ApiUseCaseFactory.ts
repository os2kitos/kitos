module Kitos.Services.Generic {

    export type ApiCall<T> = () => angular.IPromise<T>
    export type ApiCallSuccessCallback<T> = (value: T) => T;
    export type ApiCallErrorCallback = (error: Models.Api.ApiResponseErrorCategory) => void;

    export interface IAsyncApiUseCase<T> {
        executeAsync(optionalSuccessCallback?: ApiCallSuccessCallback<T>, optionalErrorCallback?: ApiCallErrorCallback): angular.IPromise<T>
    }

    class AsyncApiChangeFieldUseCase<T> implements IAsyncApiUseCase<T> {
        constructor(private readonly notify, private readonly apiCall: ApiCall<T>) {
            if (!notify) throw new Error("notify must be defined");
            if (!apiCall) throw new Error("apiCall must be defined");
        }

        executeAsync(optionalSuccessCallback?: (successValue: T) => T, optionalErrorCallback?: (errorValue: Models.Api.ApiResponseErrorCategory) => void): angular.IPromise<T> {
            var msg = this.notify.addInfoMessage("Gemmer...", false);

            return this
                .apiCall()
                .then
                (
                    (success: T) => {
                        msg.toSuccessMessage("Feltet er opdateret.");
                        if (!!optionalSuccessCallback) {
                            success = optionalSuccessCallback(success);
                        }
                        return success;
                    },
                    (error: Models.Api.ApiResponseErrorCategory) => {

                        switch (error) {
                            case Models.Api.ApiResponseErrorCategory.NotFound:
                                msg.toErrorMessage("Fejl! Registreringen kunne ikke findes. En af dine kollegaer har muligvis nedlagt den. Tryk F5 og prøv igen!");
                                break;
                            case Models.Api.ApiResponseErrorCategory.BadInput:
                                msg.toErrorMessage("Fejl! Værdien er ugyldigt!");
                                break;
                            case Models.Api.ApiResponseErrorCategory.Conflict:
                                msg.toErrorMessage("Fejl! Feltet kunne ikke ændres da værdien den allerede findes i KITOS!");
                                break;
                            default:
                                msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                break;
                        }

                        if (!!optionalErrorCallback) {
                            optionalErrorCallback(error);
                        }

                        //Fail the continuation
                        throw error;
                    }
                );
        }
    }

    export interface IApiUseCaseFactory {
        createUpdate<T>(change: ApiCall<T>): AsyncApiChangeFieldUseCase<T>;
    }

    export class ApiUseCaseFactory implements IApiUseCaseFactory {
        static $inject = ["notify"];
        constructor(private readonly notify) {

        }

        createUpdate<T>(apiCall: ApiCall<T>): AsyncApiChangeFieldUseCase<T> {
            if (!apiCall) {
                throw new Error("apiCall must be defined");
            }
            return new AsyncApiChangeFieldUseCase<T>(this.notify, apiCall);
        }
    }

    app.service("apiUseCaseFactory", ApiUseCaseFactory);
}

module Kitos.Services.Generic {

    export type ApiCall<T> = () => angular.IPromise<T>
    export type ApiCallSuccessCallback<T> = (value: T) => T;
    export type ApiCallErrorCallback = (error: Models.Api.ApiResponseErrorCategory) => void;

    export interface IAsyncApiUseCase<T> {
        executeAsync(optionalSuccessCallback?: ApiCallSuccessCallback<T>, optionalErrorCallback?: ApiCallErrorCallback): angular.IPromise<T>
    }

    enum ChangeType {
        Creation,
        Removal,
        Update
    }

    class AsyncApiChangeUseCase<T> implements IAsyncApiUseCase<T> {
        constructor(private readonly notify, private readonly apiCall: ApiCall<T>, private readonly contextName: string, private readonly changeType: ChangeType) {
            if (!notify) throw new Error("notify must be defined");
            if (!apiCall) throw new Error("apiCall must be defined");
            if (!contextName) throw new Error("removalCategory must be defined");
        }

        executeAsync(optionalSuccessCallback?: (successValue: T) => T, optionalErrorCallback?: (errorValue: Models.Api.ApiResponseErrorCategory) => void): angular.IPromise<T> {


            var messages = {
                start: "",
                success: "",
                notFound: `Fejl! ${this.contextName} kunne ikke findes. En af dine kollegaer har muligvis nedlagt den. Tryk F5 og prøv igen!`,
                conflict: `Fejl! ${this.contextName} kunne oprettes da den allerede findes!`,
                fallback: "Fejl!",
                badInput: "Fejl! Værdien er ugyldig!"
            }

            switch (this.changeType) {
                case ChangeType.Creation:
                    messages.start = `Opretter ${this.contextName} ...`;
                    messages.success = `${this.contextName} blev oprettet`;
                    messages.fallback = `Fejl! ${this.contextName} kunne ikke oprettes!`;
                    break;
                case ChangeType.Removal:
                    messages.start = `Fjerner ${this.contextName} ...`;
                    messages.success = `${this.contextName} blev fjernet`;
                    messages.fallback = `Fejl! ${this.contextName} kunne ikke fjernes!`;
                    break;
                case ChangeType.Update:
                    messages.start = `Gemmer ${this.contextName}...`;
                    messages.notFound = `Fejl! Registreringen der indeholder ${this.contextName} kunne ikke findes. En af dine kollegaer har muligvis nedlagt den. Tryk F5 og prøv igen!`;
                    messages.success = `${this.contextName} er opdateret`;
                    messages.fallback = `Fejl! ${this.contextName} kunne ikke ændres!`;
                    messages.conflict = `Fejl! ${this.contextName} kunne ikke ændres da værdien den allerede findes i KITOS!`;
                    break;
                default:
                    break;
            }

            var msg = this.notify.addInfoMessage(messages.start, false);

            return this
                .apiCall()
                .then
                (
                    (success: T) => {
                        msg.toSuccessMessage(messages.success);
                        if (!!optionalSuccessCallback) {
                            success = optionalSuccessCallback(success);
                        }
                        return success;
                    },
                    (error: Models.Api.ApiResponseErrorCategory) => {

                        switch (error) {
                            case Models.Api.ApiResponseErrorCategory.Conflict:
                                msg.toErrorMessage(messages.conflict);
                                break;
                            case Models.Api.ApiResponseErrorCategory.BadInput:
                                msg.toErrorMessage(messages.badInput);
                                break;
                            case Models.Api.ApiResponseErrorCategory.NotFound:
                                msg.toErrorMessage(messages.notFound);
                                break;
                            default:
                                msg.toErrorMessage(messages.fallback);
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
        createUpdate<T>(fieldName: string, change: ApiCall<T>): AsyncApiChangeUseCase<T>;
        createAssignmentRemoval<T>(apiCall: ApiCall<T>): AsyncApiChangeUseCase<T>;
        createAssignmentCreation<T>(apiCall: ApiCall<T>): AsyncApiChangeUseCase<T>;
    }

    export class ApiUseCaseFactory implements IApiUseCaseFactory {
        static $inject = ["notify"];
        constructor(private readonly notify) {

        }

        createUpdate<T>(fieldName: string, apiCall: ApiCall<T>): AsyncApiChangeUseCase<T> {
            if (!apiCall) {
                throw new Error("apiCall must be defined");
            }
            return new AsyncApiChangeUseCase<T>(this.notify, apiCall, fieldName, ChangeType.Update);
        }

        createAssignmentRemoval<T>(apiCall: ApiCall<T>): AsyncApiChangeUseCase<T> {
            if (!apiCall) {
                throw new Error("apiCall must be defined");
            }
            return new AsyncApiChangeUseCase<T>(this.notify, apiCall, "Tilknytningen", ChangeType.Removal);
        }

        createAssignmentCreation<T>(apiCall: ApiCall<T>): AsyncApiChangeUseCase<T> {
            if (!apiCall) {
                throw new Error("apiCall must be defined");
            }
            return new AsyncApiChangeUseCase<T>(this.notify, apiCall, "Tilknytningen", ChangeType.Creation);
        }
    }

    app.service("apiUseCaseFactory", ApiUseCaseFactory);
}

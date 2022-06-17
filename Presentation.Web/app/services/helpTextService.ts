module Kitos.Services {

    export interface IHelpText {
        title: string | null;
        htmlText: string | null;
    }

    interface IHelpTextOdataModel {
        Id?: number;
        Title?: string;
        Description?: string;
    }

    const helpTextCache: Record<string, IHelpText | null> = {}

    function clearCache(key: string): void {
        const cacheKey = getCacheKey(key);
        const cachedValue = helpTextCache[cacheKey];
        if (cachedValue != undefined) {
            delete helpTextCache[cacheKey];
        }
    }

    function getCacheKey(key: string) {
        return key.replace(".", "_");
    }

    export interface IHelpTextService {
        /**
         * Loads the help text based on the provided key
         * @param key
         */
        loadHelpText(key: string, ignoreCache?: boolean): ng.IPromise<IHelpText | null>;
        deleteHelpText(id: number, key: string): ng.IPromise<unknown>;
        updateHelpText(id: number, key: string, title: string, text: string): ng.IPromise<unknown>;
        createHelpText(key: string, title: string): angular.IPromise<unknown>;
        getHelpTextFromApi(key: string): angular.IHttpPromise<Models.IODataResult<IHelpTextOdataModel>>;
    }

    class HelpTextService implements IHelpTextService {
        deleteHelpText(id: number, key: string): angular.IPromise<unknown> {

            return this.apiUseCaseFactory
                .createDeletion
                (
                    `Hjælpeteksten for '${key}'`,
                    () => this.$http({ method: "DELETE", url: `odata/HelpTexts(${id})` })
                )
                .executeAsync(result => {
                    clearCache(key);
                    return result;
                });
        }

        updateHelpText(id: number, key: string, title: string, text: string): angular.IPromise<unknown> {
            const payload: IHelpTextOdataModel = {
                Title: title,
                Description: text
            };

            return this.apiUseCaseFactory
                .createUpdate(
                    `Hjælpeteksten for '${key}'`,
                    () => this.$http<unknown>({
                        method: "PATCH",
                        url: `odata/HelpTexts(${id})`,
                        data: payload
                    })
                )
                .executeAsync(result => {
                    clearCache(key);
                    return result;
                });
        }

        loadHelpText(key: string, ignoreCache?: boolean): angular.IPromise<IHelpText> {
            const cacheKey = getCacheKey(key);
            if (!ignoreCache) {
                const cachedValue = helpTextCache[cacheKey];
                if (cachedValue != undefined) {
                    return this.$q.resolve(cachedValue);
                }
            }
            return this.getHelpTextFromApi(key)
                .then((result) => {
                    let text: IHelpText | null = null;
                    if (result.data.value.length > 0) {
                        const translation = result.data.value[0];

                        text = {
                            title: translation.Title == undefined ? null : translation.Title,
                            htmlText: translation.Description == undefined ? null : this.$sce.trustAsHtml(translation.Description),
                        }
                    }
                    helpTextCache[cacheKey] = text;
                    return text;
                });
        }

        createHelpText(key: string, title: string): angular.IPromise<unknown> {
            const user = this.userService.getUser();
            const payload = {
                Title: title,
                Key: key
            };

            return this.$http.post(`odata/HelpTexts?organizationId=${user.$$state.value.currentOrganizationId}`, payload, { handleBusy: true });
        }

        getHelpTextFromApi(key: string): angular.IHttpPromise<Models.IODataResult<IHelpTextOdataModel>> {
            return this.$http.get<Models.IODataResult<IHelpTextOdataModel>>(`odata/HelpTexts?$filter=Key eq '${key}'`);
        }

        static $inject = ["$http", "$sce", "$q", "apiUseCaseFactory", "userService"];

        constructor(
            private readonly $http: IHttpServiceWithCustomConfig,
            private readonly $sce: ng.ISCEService,
            private readonly $q: ng.IQService,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory,
            private readonly userService: Services.IUserService) { }
    }

    app.service("helpTextService", HelpTextService);
}
module Kitos.Services {

    export type Select2AsyncDataSource = (query: string) => ng.IPromise<Models.ViewModel.Generic.Select2OptionViewModel<any>[]>

    export interface ISelect2LoadingService {
        loadSelect2(url: string, allowClear: boolean, paramArray: any, removeDisabledItems: boolean, nameContentQueryParamName?: string);
        loadSelect2WithDataSource(source: Select2AsyncDataSource, allowClear: boolean, formatResult?: (input: Models.ViewModel.Generic.Select2OptionViewModel<any>) => string);
        loadSelect2WithDataHandler(url: string, allowClear: boolean, paramArray: any, resultBuilder: (candidate: any, allResults: any[]) => void, nameContentQueryParamName?: string, formatResult?: (input: any) => string);
        select2LocalData(dataFn: () => Models.ViewModel.Generic.Select2OptionViewModel<any>[]);
        select2LocalDataNoSearch(dataFn: () => Models.ViewModel.Generic.Select2OptionViewModel<any>[], allowClear?: boolean);
    }

    export class Select2LoadingService implements ISelect2LoadingService {

        private static readonly defaultQuietMillis = 500;

        static $inject = ["$http"];
        constructor(private readonly $http: ng.IHttpService) {
        }

        select2LocalData(dataFn: () => Models.ViewModel.Generic.Select2OptionViewModel<any>[]) {
            return {
                data: () => ({ "results": dataFn() }),
                allowClear: true
            };
        }

        select2LocalDataNoSearch(dataFn: () => Models.ViewModel.Generic.Select2OptionViewModel<any>[], allowClear = true) {
            return {
                minimumResultsForSearch: Infinity,
                data: () => ({ "results": dataFn() }),
                allowClear: allowClear
            };
        }

        loadSelect2WithDataSource(source: Select2AsyncDataSource, allowClear: boolean, formatResult?: (input: Models.ViewModel.Generic.Select2OptionViewModel<any>) => string) {
            const config = <any>{
                minimumInputLength: 1,
                allowClear: allowClear,
                initSelection(elem, callback) {
                },
                ajax: {
                    data(term, _) {
                        return { query: term };
                    },
                    quietMillis: Select2LoadingService.defaultQuietMillis,
                    transport(queryParams) {
                        return source(queryParams.data.query)
                            .then
                            (
                                queryParams.success,
                                () => null
                            );
                    },
                    results(data, _) {
                        return { results: data };
                    }
                }
            };
            if (!! formatResult) {
                config.formatResult = formatResult;
            }
            return config;
        }

        loadSelect2(
            url: string,
            allowClear: boolean,
            paramArray,
            removeDisabledItems,
            nameContentQueryParamName = "q") {
            var self = this;
            return this.loadSelect2WithDataHandler(url, allowClear, paramArray, (item, items) => {
                if (removeDisabledItems) {
                    self.filterDisabledResults(items, item);
                } else {
                    self.handleResults(items, item);
                }
            }, nameContentQueryParamName);
        }

        loadSelect2WithDataHandler(
            url: string,
            allowClear: boolean,
            paramArray,
            resultBuilder: (candidate: any, allResults: any[]) => void,
            nameContentQueryParamName = "q",
            formatResult = null) {
            var self = this;
            let config = <any>{
                minimumInputLength: 1,
                initSelection(elem, callback) {
                },
                allowClear: allowClear,
                ajax: {
                    data(term, page) {
                        return { query: term };
                    },
                    quietMillis: Select2LoadingService.defaultQuietMillis,
                    transport(queryParams) {
                        const extraParams = paramArray ? `&${paramArray.join("&")}` : "";
                        const res = self.$http.get(url + "?" + nameContentQueryParamName + "=" + queryParams.data.query + extraParams).then(queryParams.success, () => null);
                        return res;
                    },

                    results(data, page) {
                        var results = [];
                        _.each(data.data.response, (obj) => {
                            resultBuilder(obj, results);
                        });
                        return { results: results };
                    }
                }
            };
            if (formatResult != null) {
                config.formatResult = formatResult;
            }
            return config;
        }

        private filterDisabledResults(list: any, obj: { id; name; disabled; itSystemDisabled; }) {
            if (!obj.disabled && !obj.itSystemDisabled) {
                this.handleResults(list, obj);
            }
        }

        private handleResults(list: any, obj: { id; name; disabled; itSystemDisabled }) {
            list.push({
                id: obj.id,
                text: Helpers.SystemNameFormat.apply(obj.name, obj.disabled || obj.itSystemDisabled)
            });
        }

    }
    app.service("select2LoadingService", Select2LoadingService);
}


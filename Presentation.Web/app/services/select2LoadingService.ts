module Kitos.Services {

    export interface ISelect2LoadingService {
        loadSelect2(url: string, allowClear: boolean, paramArray: any, removeDisabledItems: boolean, nameContentQueryParamName?: string);
        loadSelect2WithDataHandler(url: string, allowClear: boolean, paramArray: any, resultBuilder: (candidate: any, allResults: any[]) => void, nameContentQueryParamName?: string, formatResult?: (input: any) => string);
        select2LocalData(dataFn: () => [Models.ViewModel.Generic.Select2OptionViewModel]);
        select2LocalDataNoSearch(dataFn: () => [Models.ViewModel.Generic.Select2OptionViewModel], allowClear? :boolean);
    }

    export class Select2LoadingService implements ISelect2LoadingService {

        static $inject = ["$http"];
        constructor(private readonly $http: ng.IHttpService) {
        }

        select2LocalData(dataFn: () => [Models.ViewModel.Generic.Select2OptionViewModel]) {
            return {
                data: () => ({ "results": dataFn() }),
                allowClear: true
            };
        }

        select2LocalDataNoSearch(dataFn: () => [Models.ViewModel.Generic.Select2OptionViewModel], allowClear = true) {
            return {
                minimumResultsForSearch: Infinity,
                data: () => ({ "results": dataFn() }),
                allowClear: allowClear
            };
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
                    quietMillis: 500,
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

        private filterDisabledResults(list: any, obj: { id; name; disabled; }) {
            if (!obj.disabled) {
                this.handleResults(list, obj);
            }
        }

        private handleResults(list: any, obj: { id; name; disabled}) {
            list.push({
                id: obj.id,
                text: obj.name + (obj.disabled ? " (Ikke aktiv)" : "")
        });
        }

    }
    app.service("select2LoadingService", Select2LoadingService);
}


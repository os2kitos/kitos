module Kitos.Services {

    export interface ISelect2LoadingService {
        loadSelect2(url: string, allowClear: boolean, paramArray: any, checkResultsForDisabled: boolean);
        select2LocalData(dataFn: () => [Models.Generic.NamedEntity.NamedEntityDTO]);
        select2LocalDataNoSearch(dataFn: () => [Models.Generic.NamedEntity.NamedEntityDTO]);
    }

    export class Select2LoadingService implements ISelect2LoadingService {

        static $inject = ["$http"];
        constructor(private readonly $http: ng.IHttpService) {
        }

        mapNamedEntitiesToSelect2(data: () => [Models.Generic.NamedEntity.NamedEntityDTO]) {
            var mappedData = [];
            _.each(data(),
                (namedEntity) =>
                mappedData.push({
                    "id": namedEntity.id,
                    "text": namedEntity.name
                })
            );
            return {"results": mappedData}
        }

        select2LocalData(dataFn: () => [Models.Generic.NamedEntity.NamedEntityDTO]) {
            return {
                data: () => ({ "results": dataFn() }) ,
                allowClear: true
            };
        }

        select2LocalDataNoSearch(dataFn: () => [Models.Generic.NamedEntity.NamedEntityDTO]) {
            return {
                minimumResultsForSearch: Infinity,
                data: () => ({ "results": dataFn() }),
                allowClear: true
            };
        }

        loadSelect2(
            url: string,
            allowClear: boolean,
            paramArray,
            checkResultsForDisabled,
            nameContentQueryParamName = "q") {
            var self = this;
            return {
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
                            if (checkResultsForDisabled) {
                                self.handleResultsWithDisabled(results, obj);
                            } else {
                                self.handleResults(results, obj);
                            }
                            
                        });
                        return { results: results };
                    }
                }
            };
        }

        private handleResultsWithDisabled(list: any, obj: { id; name; disabled; }) {
            if (!obj.disabled) {
                this.handleResults(list, obj);
            }
        }

        private handleResults(list: any, obj: { id; name; }) {
            list.push({
                id: obj.id,
                text: obj.name
            });
        }
        
    }
    app.service("select2LoadingService", Select2LoadingService);
}


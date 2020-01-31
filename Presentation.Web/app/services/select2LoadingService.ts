module Kitos.Services {

    export interface ISelect2LoadingService {
        loadSelect2(url: string, allowClear: boolean, paramArray: any)
    }

    export class Select2LoadingService implements ISelect2LoadingService {

        static $inject = ["$http", "_"];
        constructor(private readonly $http: ng.IHttpService, private readonly _: _.LoDashStatic) {
        }

        loadSelect2(url: string, allowClear: boolean, paramArray) {
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
                        const res = self.$http.get(url + "?q=" + queryParams.data.query + extraParams).then(queryParams.success, () => null);
                        return res;
                    },

                    results(data, page) {
                        var results = [];

                        self._.each(data.data.response, (obj: { id; name; }) => {
                            results.push({
                                id: obj.id,
                                text: obj.name
                            });
                        });

                        return { results: results };
                    }
                }
            };
        }
        
    }
    app.service("select2LoadingService", Select2LoadingService);
}


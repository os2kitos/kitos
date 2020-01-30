module Kitos.Services {
    
    export interface ILazyLoadingService {
        selectLazyLoading(url, paramAry): any;
    }

    export class LazyLoadingService implements ILazyLoadingService {

        static $inject = ["$http", "_"];
        constructor(private $http: ng.IHttpService, private _: _.LoDashStatic) {
        }

        selectLazyLoading (url, paramAry) {
            return {
                allowClear: true,
                minimumInputLength: 1,
                initSelection: (elem, callback) => {
                },
                ajax: {
                    data: (term, page) => ({ query: term }),
                    quietMillis: 500,
                    transport: queryParams => {
                        var extraParams = paramAry ? "&" + paramAry.join("&") : "";
                        var res = this.$http.get(url + "?q=" + queryParams.data.query + extraParams).then(queryParams.success, () => null);
                        return res;
                    },

                    results: (data, page) => {
                        var results = [];

                        _.each(data.data.response, (obj: { id; itSystem; }) => {
                            results.push({
                                id: obj.id,
                                text: obj.itSystem.name
                            });
                        });

                        return { results: results };
                    }
                }
            };
        }
        
    }
    app.service("lazyLoadingService", LazyLoadingService);
}


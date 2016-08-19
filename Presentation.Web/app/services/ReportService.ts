/// <reference path="../index.d.ts" />

// https://docs.angularjs.org/api/ngResource/service/$resource
// https://chsakell.com/2015/04/04/asp-net-web-api-feat-odata/

module Kitos.Services.ReportService {

    class ReportService {

        static $inject = ['$http'];
        private baseUrl = '/odata/Reports';

        /** returns a promise resolve with  then(success, error) */
        /** https://docs.angularjs.org/api/ng/service/$http */
        constructor(private $http: ng.IHttpService) {
        }

        GetById = (id: number) => {
            return this.$http.get(`${this.baseUrl}(${id})`);
        }

        GetAll = () => {
            return this.$http.get<Kitos.Models.IOdataWrapper<any>>("/odata/reports?$expand=CategoryType");
        }

    }

    app.service("reportService", ReportService);
}

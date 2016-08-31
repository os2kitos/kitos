/// <reference path="../index.d.ts" />

// https://docs.angularjs.org/api/ngResource/service/$resource
// https://chsakell.com/2015/04/04/asp-net-web-api-feat-odata/

module Kitos.Services {
    export class ReportService {

        static $inject = ["$http"];
        private baseUrl = "/odata/reports";

        /** returns a promise resolve with  then(success, error) */
        /** https://docs.angularjs.org/api/ng/service/$http */
        constructor(private $http: ng.IHttpService) {
        }

        GetById = (id: number) => {
            return this.$http.get<Models.IReport>(`${this.baseUrl}(${id})`);
        }

        GetAll = () => {
            return this.$http.get<Kitos.Models.IOdataWrapper<Models.IReport>>(this.baseUrl + "?$expand=CategoryType");
        }

        GetEmptyReport = () => {
            return this.$http.get("/appReport/empty-report.json")
        }

        saveReport = (report: Kitos.Models.IReport) => {
            this.$http.patch(this.baseUrl + "(" + report.Id + ")", report);
        }
    }

    app.service("reportService", ReportService);
}

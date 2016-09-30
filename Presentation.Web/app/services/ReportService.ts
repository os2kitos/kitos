/// <reference path="../index.d.ts" />

module Kitos.Services {
    export class ReportService {

        static $inject = ["$http"];
        private baseUrl = "/odata/reports";

        /** returns a promise resolve with then(success, error) */
        /** https://docs.angularjs.org/api/ng/service/$http */
        constructor(private $http: ng.IHttpService) {
        }

        GetById = (id: number) => {
            return this.$http.get<Models.IReport>(`${this.baseUrl}(${id})`);
        }

        GetAll = () => {
            return this.$http.get<Kitos.Models.IODataResult<Models.IReport>>(this.baseUrl + "?$expand=CategoryType");
        }

        GetEmptyReport = () => {
            return this.$http.get("/appReport/empty-report.json")
        }

        saveReport = (report: Kitos.Models.IReport) => {
            this.$http.patch(this.baseUrl + "(" + report.Id + ")", report);
        }

        getReportCategories = () => {
            return this.$http.get<Kitos.Models.IODataResult<Models.IReportCategory>>("/odata/ReportCategories")
        }
    }

    app.service("reportService", ReportService);
}

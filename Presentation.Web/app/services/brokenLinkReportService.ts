module Kitos.Services.BrokenLinksReport {
    import IBrokenLinksReportDTO = Models.Api.BrokenLinksReport.IBrokenLinksReportDTO;

    export interface IBrokenLinksReportService {
        getStatus(): ng.IPromise<IBrokenLinksReportDTO>;
    }

    export class BrokenLinksReportService implements IBrokenLinksReportService{
        static $inject = ["$http"];
        constructor(private readonly $http: ng.IHttpService) { }

        getStatus(): ng.IPromise<IBrokenLinksReportDTO> {
            return this.$http.get("api/v2/internal/broken-external-references-report/status")
                .then(result => {
                    var response = result.data as IBrokenLinksReportDTO ;
                    return response;
                });
        }
    }

    app.service("brokenLinksReportService", BrokenLinksReportService);
}
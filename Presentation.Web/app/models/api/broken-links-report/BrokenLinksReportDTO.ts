module Kitos.Models.Api.BrokenLinksReport {

    export interface IBrokenLinksReportDTO {
        available: boolean;
        createdDate: string;
        brokenLinksCount: number;
    }
}
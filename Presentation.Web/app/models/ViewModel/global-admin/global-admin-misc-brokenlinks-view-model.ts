module Kitos.Models.ViewModel.GlobalAdmin.BrokenLinks {
    import IBrokenLinksReportDTO = Models.Api.BrokenLinksReport.IBrokenLinksReportDTO;

    export interface IBrokenLinksReportStatusViewModel {
        available: boolean;
        createdDate: string;
        brokenLinksCount: number;
        linkToCurrentReport : string;
    }

    export class BrokenLinksViewModelMapper {
        
        static mapDate(date: string) {
            if (!date)
                return "Ukendt";
            return new Date(date).toLocaleDateString();
        }

        static mapFromApiResponse(apiResponse: IBrokenLinksReportDTO): IBrokenLinksReportStatusViewModel {
            return <IBrokenLinksReportStatusViewModel>{
                available: apiResponse.available,
                createdDate: BrokenLinksViewModelMapper.mapDate(apiResponse.createdDate),
                brokenLinksCount: apiResponse.brokenLinksCount,
                linkToCurrentReport: "api/v1/broken-external-references-report/current/csv"
            };
        }
    }
}
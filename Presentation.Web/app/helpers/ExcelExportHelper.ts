module Kitos.Helpers {
    import IItProjectInactiveOverview = ItProject.OverviewInactive.IItProjectInactiveOverview;

    export class ExcelExportHelper {

        static renderReferenceUrl(reference: Models.Reference.IOdataReference) {
            if (reference == null) {
                return "";
            }
            if (Utility.Validation.validateUrl(reference.URL)) {
                return reference.URL;
            }
            return reference.Title;
        }

        static renderExternalReferenceId(reference: Models.Reference.IOdataReference) {
            if (reference == null) {
                return "";
            }
            if (reference.ExternalReferenceId != null) {
                return reference.ExternalReferenceId;
            }
            return reference.Title;
        }

        static renderUrlOrFallback(url, fallback) {
            if (Utility.Validation.validateUrl(url)) {
                return url;
            }
            if (fallback != null) {
                return fallback;
            }
            return "";
        }

        static renderProjectStatusColor(status: Models.ItProject.IItProjectStatusUpdate[]) {

            var getColor = (statusArray: Array<string>) => {
                var colToMatch = ["Red", "Yellow", "Green", "White"];
                var i: number;
                for (i = 0; i < colToMatch.length; i++) {
                    if (statusArray.indexOf(colToMatch[i]) > -1) {
                        this.convertColorsToDanish(colToMatch[i]);
                    }
                    else {
                        continue;
                    }
                }
                return this.convertColorsToDanish(colToMatch[i]);
            };

            if (status.length > 0) {
                var latestStatus = status[0];
                var statusArray = [latestStatus.TimeStatus, latestStatus.QualityStatus, latestStatus.ResourcesStatus];

                if (latestStatus.IsCombined) {
                    return this.convertColorsToDanish(latestStatus.CombinedStatus);
                }
                else {
                    return getColor(statusArray);
                }
            }
            else {
                return "";
            }
        }

        static renderDate(date: Date) {
            return moment(date).format("DD-MM-YYYY");
        }

        static renderStatusColorWithStatus(dataItem: IItProjectInactiveOverview, status) {
            var latestStatus = dataItem.ItProjectStatusUpdates[0];
            var statusToShow = (latestStatus.IsCombined) ? latestStatus.CombinedStatus : status;
            return ExcelExportHelper.convertColorsToDanish(statusToShow);
        }

        static convertColorsToDanish(color: string) {
            switch (color) {
                case "Green":
                    return "Grøn";
                case "Yellow":
                    return "Gul";
                case "Red":
                    return "Rød";
                case "White":
                    return "Hvid";
                default:
                    return "";
            }
        }

        static getGoalStatus(goalStatus: Models.TrafficLight) {
            if (goalStatus == null) {
                return "";
            }
            return this.convertColorsToDanish(goalStatus.toString());
        }
    }
}
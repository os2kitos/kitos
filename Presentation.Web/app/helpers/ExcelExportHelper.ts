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
            if (status.length > 0) {
                var latestStatus = status[0];
                var statusTime = latestStatus.TimeStatus;
                var statusQuality = latestStatus.QualityStatus;
                var statusResources = latestStatus.ResourcesStatus;
                if (latestStatus.IsCombined) {
                    return latestStatus.CombinedStatus;
                } else {
                    /* If no combined status exists, take the lowest status from the splitted status */
                    if (statusTime === "Red" || statusQuality === "Red" || statusResources === "Red") {
                        return "Rød";
                    } else if (statusTime === "Yellow" || statusQuality === "Yellow" || statusResources === "Yellow") {
                        return "Gul";
                    } else if (statusTime === "Green" || statusQuality === "Green" || statusResources === "Green") {
                        return "Grøn";
                    } else {
                        return "Hvid";
                    }
                }
            } else {
                return "";
            }
        }

        static renderDate(dataItem: IItProjectInactiveOverview, moment: moment.MomentStatic) {

            if (!dataItem.CurrentPhaseObj || !dataItem.CurrentPhaseObj.StartDate) {
                return "";
            }

            return moment(dataItem.CurrentPhaseObj.StartDate).format("DD-MM-YYYY");
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
                    return color;
            }
        }

    }
}
module Kitos.Helpers {
    import IItProjectInactiveOverview = ItProject.OverviewInactive.IItProjectInactiveOverview;

    interface IStatusColor {
        danish: string;
        english: string;
    }

    export class ExcelExportHelper {

        private static readonly noValueFallback = "";

        private static readonly colors = {
            red: <IStatusColor>{ danish: "Rød", english: "Red" },
            green: <IStatusColor>{ danish: "Grøn", english: "Green" },
            yellow: <IStatusColor>{ danish: "Gul", english: "Yellow" },
            white: <IStatusColor>{ danish: "Hvid", english: "White" }
        }

        static renderReferenceUrl(reference: Models.Reference.IOdataReference) {
            if (reference == null) {
                return ExcelExportHelper.noValueFallback;
            }
            if (Utility.Validation.validateUrl(reference.URL)) {
                return reference.URL;
            }
            return reference.Title;
        }

        static renderExternalReferenceId(reference: Models.Reference.IOdataReference) {
            if (reference == null) {
                return ExcelExportHelper.noValueFallback;
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
            return ExcelExportHelper.noValueFallback;
        }

        static renderProjectStatusColor(status: Models.ItProject.IItProjectStatusUpdate[]) {

            const getColor = (statusArray: Array<string>) => {
                var prioritizedColorOrder = [
                    ExcelExportHelper.colors.red,
                    ExcelExportHelper.colors.yellow,
                    ExcelExportHelper.colors.green,
                    ExcelExportHelper.colors.white
                ];

                let normalizedStatuses = _.filter(statusArray, item => !!item);
                normalizedStatuses = _.map(normalizedStatuses, item => item.toLowerCase());

                for (let currentPrioritizedColor of prioritizedColorOrder) {

                    const existingColor = _.find(normalizedStatuses, currentPrioritizedColor.english.toLowerCase());

                    if (existingColor) {
                        return currentPrioritizedColor.danish;
                    }
                }

                return ExcelExportHelper.noValueFallback;
            };

            if (status.length > 0) {
                const latestStatus = status[0];
                const statusArray = [latestStatus.TimeStatus, latestStatus.QualityStatus, latestStatus.ResourcesStatus];

                if (latestStatus.IsCombined) {
                    return this.convertColorsToDanish(latestStatus.CombinedStatus);
                }
                else {
                    return getColor(statusArray);
                }
            }
            else {
                return ExcelExportHelper.noValueFallback;
            }
        }

        static renderDate(date: Date) {
            return moment(date).format("DD-MM-YYYY");
        }

        static renderStatusColorWithStatus(dataItem: IItProjectInactiveOverview, status) {
            const latestStatus = dataItem.ItProjectStatusUpdates[0];
            const statusToShow = (latestStatus.IsCombined) ? latestStatus.CombinedStatus : status;
            return ExcelExportHelper.convertColorsToDanish(statusToShow);
        }

        static convertColorsToDanish(color: string) {
            if (color !== null) {
                const knownColor = ExcelExportHelper.colors[color.toLowerCase()];
                if (!_.isUndefined(knownColor)) {
                    return knownColor.danish;
                }
            }
            return ExcelExportHelper.noValueFallback;
        }

        static getGoalStatus(goalStatus: Models.TrafficLight) {
            if (goalStatus == null) {
                return ExcelExportHelper.noValueFallback;
            }
            return this.convertColorsToDanish(goalStatus.toString());
        }

        static getRoles(rights: any[]) {

            var string = "";

            for (let right of rights) {
                string += `${right.Role.Name}`;
            }
            return string;

        }
    }
}
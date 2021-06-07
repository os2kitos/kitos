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

        static renderUrlWithOptionalTitle(title: string | null | undefined, url: string) {
            if (Utility.Validation.isValidExternalReference(url)) {
                return url;
            }
            if (title === null || _.isUndefined(title)) {
                return ExcelExportHelper.noValueFallback;
            }
            return title;
        }

        static renderReference(referenceTitle: string, referenceUrl: string) {
            return ExcelExportHelper.renderUrlWithOptionalTitle(referenceTitle, referenceUrl);
        }

        static renderReferenceUrl(reference: Models.Reference.IOdataReference) {
            if (reference === null || _.isUndefined(reference)) {
                return ExcelExportHelper.noValueFallback;
            }
            return ExcelExportHelper.renderReference(reference.Title, reference.URL);
        }

        static renderReferenceId(externalReferenceId: string) {
            if (externalReferenceId != null) {
                return externalReferenceId;
            }
            return ExcelExportHelper.noValueFallback;
        }

        static renderExternalReferenceId(reference: Models.Reference.IOdataReference) {
            if (reference === null || _.isUndefined(reference)) {
                return ExcelExportHelper.noValueFallback;
            }
            return ExcelExportHelper.renderReferenceId(reference.ExternalReferenceId);
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

                const statusMap = _.reduce(statusArray, (acc: any, current) => {
                    if (!!current) {
                        acc[current.toLowerCase()] = true;
                    }
                    return acc;
                }, <any>{});

                for (let currentPrioritizedColor of prioritizedColorOrder) {
                    if (statusMap.hasOwnProperty(currentPrioritizedColor.english.toLowerCase())) {
                        return currentPrioritizedColor.danish;
                    }
                }

                return ExcelExportHelper.noValueFallback;
            };

            if (status.length > 0) {
                const latestStatus = status[0];

                if (latestStatus.IsCombined) {
                    return this.convertColorsToDanish(latestStatus.CombinedStatus);
                }
                else {
                    const statusArray = [latestStatus.TimeStatus, latestStatus.QualityStatus, latestStatus.ResourcesStatus];
                    return getColor(statusArray);
                }
            }
            else {
                return ExcelExportHelper.noValueFallback;
            }
        }

        static renderDate(date: Date) {
            if (!!date) {
                return moment(date).format(Constants.DateFormat.DanishDateFormat);
            }
            return ExcelExportHelper.noValueFallback;
        }

        static renderStatusColorWithStatus(dataItem: IItProjectInactiveOverview, status) {
            const latestStatus = dataItem.ItProjectStatusUpdates[0];
            const statusToShow = (latestStatus.IsCombined) ? latestStatus.CombinedStatus : status;
            return ExcelExportHelper.convertColorsToDanish(statusToShow);
        }

        static convertColorsToDanish(color: string) {
            if (color === null || _.isUndefined(color)) {
                return ExcelExportHelper.noValueFallback;
            }
            const knownColor = ExcelExportHelper.colors[color.toLowerCase()];
            if (!_.isUndefined(knownColor)) {
                return knownColor.danish;
            }
        }

        static getGoalStatus(goalStatus: Models.TrafficLight) {
            if (goalStatus === null || _.isUndefined(goalStatus)) {
                return ExcelExportHelper.noValueFallback;
            }
            return this.convertColorsToDanish(goalStatus.toString());
        }

        static renderUserRoles(rights: any[], projectRoles) {
            let result = "";
            _.each(rights,
                (right, index,) => {
                    if (!_.find(projectRoles, (option: any) => (option.Id === parseInt(right.Role.Id, 10)))) {
                        result += `${right.Role.Name} (udgået)`;
                    } else {
                        result += `${right.Role.Name}`;
                    }

                    if (index !== rights.length - 1) {
                        result += ", ";
                    }

                });
            return result;
        }

        static renderString(value: string) {
            return value || ExcelExportHelper.noValueFallback;
        }
    }
}
module Kitos.Helpers {

    export class KendoToolbarCustomizationHelper {

        static getMargins(margins?: Utility.KendoGrid.KendoToolbarMargin[]): string {
            if (margins === undefined)
                return "";

            const uniqueMargins = margins.filter((v, i, a) => a.indexOf(v) === i);
            if (uniqueMargins.length < 1)
                return "";
            var marginHtmlClasses: string[] = [];
            uniqueMargins.forEach(margin => {
                marginHtmlClasses.push(this.convertMarginEnumToHtmlClass(margin));
            });

            return marginHtmlClasses.join(" ");
        }

        static convertMarginEnumToHtmlClass(margin: Utility.KendoGrid.KendoToolbarMargin): string {
            switch (margin) {
                case Utility.KendoGrid.KendoToolbarMargin.Left:
                    return "kendo-margin-left";
                case Utility.KendoGrid.KendoToolbarMargin.Right:
                    return "kendo-margin-right";
                case Utility.KendoGrid.KendoToolbarMargin.Top:
                    throw `Unknown margin ${margin}`;
                case Utility.KendoGrid.KendoToolbarMargin.Down:
                    throw `Unknown margin ${margin}`;
                default:
                    throw `Unknown margin ${margin}`;
            }
        }

        static getColorClass(color: Utility.KendoGrid.KendoToolbarButtonColor): string {
            switch (color) {
                case Utility.KendoGrid.KendoToolbarButtonColor.None:
                    return "";
                case Utility.KendoGrid.KendoToolbarButtonColor.Green:
                    return "btn kendo-btn-sm btn-success";
                case Utility.KendoGrid.KendoToolbarButtonColor.Grey:
                    return "k-button k-button-icontext";
                default:
                    throw `Unknown color ${color}`;
            }
        };

        static getPositionClass(position: Utility.KendoGrid.KendoToolbarButtonPosition): string {
            switch (position) {
                case Utility.KendoGrid.KendoToolbarButtonPosition.Left:
                    return "";
                case Utility.KendoGrid.KendoToolbarButtonPosition.Right:
                    return "pull-right";
                default:
                    throw `Unknown position ${position}`;
            }
        };

        static getStandardWidthCssClass(standardWidth: Utility.KendoGrid.KendoToolbarStandardWidth | undefined): string {
            if (standardWidth === undefined) {
                return "";
            }

            switch (standardWidth) {
                case Utility.KendoGrid.KendoToolbarStandardWidth.Standard:
                    return "";
                case Utility.KendoGrid.KendoToolbarStandardWidth.FitContent:
                    return "width-fit-content";
                default:
                    throw `Unknown position ${standardWidth}`;
            }
        };
    }
}
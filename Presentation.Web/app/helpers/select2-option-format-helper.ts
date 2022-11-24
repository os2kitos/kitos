module Kitos.Helpers {
    export class Select2OptionsFormatHelper {
        public static formatUserWithEmail(user: {text:string, email?: string}): string {
            return Select2OptionsFormatHelper.formatText(user.text, user.email);
        }

        public static formatOrganizationWithCvr(org: {text: string, cvr?: string}): string {
            return Select2OptionsFormatHelper.formatText(org.text, org.cvr);
        }

        public static formatOrganizationWithOptionalObjectContext(org: { text: string, optionalObjectContext?: { cvrNumber: string } }): string {
            return Select2OptionsFormatHelper.formatText(org.text, org.optionalObjectContext?.cvrNumber);
        }

        public static formatChangeLog(changeLog: Models.Api.Organization.ConnectionChangeLogDTO): string {
            const dateText = Helpers.RenderFieldsHelper.renderDateWithTime(changeLog.logTime);
            const responsibleEntityText = Helpers.ConnectionChangeLogHelper.getResponsibleEntityTextBasedOnOrigin(changeLog);

            return Select2OptionsFormatHelper.formatText(dateText, responsibleEntityText);
        }

        public static addIndentationToUnitChildren(orgUnit: Models.Api.Organization.OrganizationUnit, indentationLevel: number): Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<Models.Api.Organization.OrganizationUnit>[] {
            const options: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<Models.Api.Organization.OrganizationUnit>[] = [];
            Select2OptionsFormatHelper.visitUnit(orgUnit, indentationLevel, options);

            return options;
        }

        private static formatText(text: string, subText?: string): string {
            let result = `<div>${text}</div>`;
            if (subText) {
                result += `<div class="small">${subText}</div>`;
            }
            return result;
        }
        
        private static visitUnit(orgUnit: Kitos.Models.Api.Organization.OrganizationUnit, indentationLevel: number, options: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<Models.Api.Organization.OrganizationUnit>[]) {
            const option = {
                id: String(orgUnit.id),
                text: orgUnit.name,
                indentationLevel: indentationLevel,
                optionalExtraObject: orgUnit
            };

            options.push(option);

            orgUnit.children.forEach(child => {
                return Select2OptionsFormatHelper.visitUnit(child, indentationLevel + 1, options);
            });

        }
    }
}

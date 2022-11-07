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

        public static addIndentationToUnitChildren(orgUnit: Kitos.Models.Api.Organization.OrganizationUnit, indentationLevel: number): Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<number>[] {
            const options: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<number>[] = [];
            Select2OptionsFormatHelper.visitUnit(orgUnit, 0, options);

            return options;
        }

        private static formatText(text: string, subText?: string): string {
            let result = `<div>${text}</div>`;
            if (subText) {
                result += `<div class="small">${subText}</div>`;
            }
            return result;
        }
        
        private static visitUnit(orgUnit: Kitos.Models.Api.Organization.OrganizationUnit, indentationLevel: number, options: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<number> []) {
            const option = {
                id: String(orgUnit.id),
                uuid: orgUnit.uuid,
                text: orgUnit.name,
                indentationLevel: indentationLevel
            };

            options.push(option);

            _.each(orgUnit.children, function (child) {
                return Select2OptionsFormatHelper.visitUnit(child, indentationLevel + 1, options);
            });

        }
    }
}

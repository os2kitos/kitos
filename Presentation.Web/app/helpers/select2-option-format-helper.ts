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

        public static addIndentationToUnitChildren(orgUnit: Models.Api.Organization.OrganizationUnit, indentationLevel: number, idToSkip?: number): Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<Models.Api.Organization.OrganizationUnit>[] {
            const options: Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<Models.Api.Organization.OrganizationUnit>[] = [];
            Select2OptionsFormatHelper.visitUnit(orgUnit, indentationLevel, options, idToSkip);

            return options;
        }

        public static formatIndentation(result: Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<any>): string {
            function visit(text: string, indentationLevel: number): string {
                if (indentationLevel <= 0) {
                    return text;
                }
                //indentation is four non breaking spaces
                return visit("&nbsp&nbsp&nbsp&nbsp" + text, indentationLevel - 1);
            }

            const formattedResult = visit(result.text, result.indentationLevel);
            return formattedResult;
        }

        private static formatText(text: string, subText?: string): string {
            let result = `<div>${text}</div>`;
            if (subText) {
                result += `<div class="small">${subText}</div>`;
            }
            return result;
        }

        
        private static visitUnit(orgUnit: Models.Api.Organization.OrganizationUnit, indentationLevel: number, options: Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<Models.Api.Organization.OrganizationUnit>[], unitIdToSkip?: number) {
            if (unitIdToSkip && orgUnit.id === unitIdToSkip) {
                return;
            }

            const option = {
                id: String(orgUnit.id),
                text: orgUnit.name,
                indentationLevel: indentationLevel,
                optionalExtraObject: orgUnit
            };

            options.push(option);

            orgUnit.children.forEach(child => {
                return Select2OptionsFormatHelper.visitUnit(child, indentationLevel + 1, options, unitIdToSkip);
            });

        }
    }
}

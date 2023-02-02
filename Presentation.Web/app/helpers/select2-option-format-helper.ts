module Kitos.Helpers {
    export class Select2OptionsFormatHelper {
        public static formatUserWithEmail(user: {text:string, email?: string}): string {
            return Select2OptionsFormatHelper.formatText(user.text, user.email);
        }

        public static formatOrganizationWithCvr(org: Models.ViewModel.Select2.ISelect2OrganizationOptionWithCvrViewModel): string {
            return Select2OptionsFormatHelper.formatText(org.text, org.cvr);
        }

        public static formatOrganizationWithOptionalObjectContext(org: { text: string, optionalObjectContext?: { cvrNumber: string } }): string {
            return Select2OptionsFormatHelper.formatText(org.text, org.optionalObjectContext?.cvrNumber);
        }

        public static formatChangeLog(changeLog: Models.Api.Organization.ConnectionChangeLogDTO): string {
            const dateText = Helpers.RenderFieldsHelper.renderDate(changeLog.logTime);
            const responsibleEntityText = Helpers.ConnectionChangeLogHelper.getResponsibleEntityTextBasedOnOrigin(changeLog);

            return Select2OptionsFormatHelper.formatText(dateText, responsibleEntityText);
        }

        public static addIndentationToUnitChildren(orgUnit: Models.Api.Organization.OrganizationUnit, indentationLevel: number, idToSkip?: number): Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<Models.Api.Organization.OrganizationUnit>[] {
            const options: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<Models.Api.Organization.OrganizationUnit>[] = [];
            Select2OptionsFormatHelper.visitUnit(orgUnit, indentationLevel, options, idToSkip);

            return options;
        }

        public static formatIndentation(result: Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<any>, addUnitOriginIndication: boolean = false): string {
            function visit(text: string, indentationLevel: number, addUnitOriginIndication: boolean, isKitosUnit: boolean = false, indentationText: string = ""): string {
                if (indentationLevel <= 0) {
                    return addUnitOriginIndication === false ? indentationText + text : Select2OptionsFormatHelper.formatIndentationWithOriginText(text, indentationText, isKitosUnit);
                }

                //indentation is four non breaking spaces
                return visit(text, indentationLevel - 1, addUnitOriginIndication, isKitosUnit, indentationText + Constants.Select2.UnitIndentation);
            }

            let isKitosUnit = true;
            if (addUnitOriginIndication) {
                if (result.optionalObjectContext?.externalOriginUuid) {
                    isKitosUnit = false;
                }
            }

            const formattedResult = visit(result.text, result.indentationLevel, addUnitOriginIndication, isKitosUnit);
            return formattedResult;
        }

        private static formatIndentationWithOriginText(text: string, indentationText: string, isKitosUnit: boolean) {
            if (isKitosUnit) {
                return `<div><span class="org-structure-legend-square org-structure-legend-color-native-unit right-margin-5px"></span>${indentationText}${text}</div>`;
            }

            return `<div><span class="org-structure-legend-square org-structure-legend-color-fk-org-unit right-margin-5px"></span>${indentationText}${text}</div>`;
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
                optionalObjectContext: orgUnit
            };

            options.push(option);

            orgUnit.children.forEach(child => {
                return Select2OptionsFormatHelper.visitUnit(child, indentationLevel + 1, options, unitIdToSkip);
            });

        }
    }
}

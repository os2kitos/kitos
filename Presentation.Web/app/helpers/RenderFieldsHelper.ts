module Kitos.Helpers {

    export class RenderFieldsHelper {

        private static readonly noValueFallback = "";

        static renderInternalReference(detailsType : string, detailsState : string, stateId : number, value : string) {
            if (!!value) {
                return `<a data-element-type="${detailsType}" data-ui-sref="${detailsState}({id: ${stateId}})">${value}</a>`;
            } else {
                return RenderFieldsHelper.noValueFallback;
            }
        }

        static renderReference(referenceTitle: string, referenceUrl: string) {
            if (referenceTitle === null || _.isUndefined(referenceTitle)) {
                if (Utility.Validation.isValidExternalReference(referenceUrl)) {
                    return `<a target="_blank" style="float:left;" href="${referenceUrl}">${referenceUrl}</a>`;
                } else {
                    return RenderFieldsHelper.noValueFallback;
                }
            }
            if (Utility.Validation.isValidExternalReference(referenceUrl)) {
                return `<a target="_blank" style="float:left;" href="${referenceUrl}">${referenceTitle}</a>`;
            }
            return referenceTitle;
        }

        static renderReferenceUrl(reference: Models.Reference.IOdataReference) {
            
            if (reference === null || _.isUndefined(reference)) {
                return RenderFieldsHelper.noValueFallback;
            }
            return RenderFieldsHelper.renderReference(reference.Title, reference.URL);
        }

        static renderReferenceId(externalReferenceId: string) {
            if (externalReferenceId != null) {
                return externalReferenceId;
            }
            return RenderFieldsHelper.noValueFallback;
        }

        static renderExternalReferenceId(reference: Models.Reference.IOdataReference) {
            if (reference === null || _.isUndefined(reference)) {
                return RenderFieldsHelper.noValueFallback;
            }
            return RenderFieldsHelper.renderReferenceId(reference.ExternalReferenceId);
        }
    }
}
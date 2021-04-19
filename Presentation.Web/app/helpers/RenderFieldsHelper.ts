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
                referenceTitle = referenceUrl;
            }
            if (Utility.Validation.isValidExternalReference(referenceUrl)) {
                return `<a target="_blank" style="float:left;" href="${referenceUrl}">${referenceTitle}</a>`;
            }
            return referenceTitle || this.noValueFallback;
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

        static renderString(value: string) {
            if (!!value) {
                return value;
            }
            return RenderFieldsHelper.noValueFallback;
        }

        static renderDate(date: Date) {
            if (!!date) {
                return moment(date).format("DD-MM-YYYY");
            }
            return RenderFieldsHelper.noValueFallback;
        }
    }
}
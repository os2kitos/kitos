﻿module Kitos.Helpers {

    export interface ActiveStatusTexts {
        active: string
        notActive: string
    }

    export interface StatusTexts {
        status: {
            systems: ActiveStatusTexts,
            other: ActiveStatusTexts
        }
    }
    export class RenderFieldsHelper {

        private static readonly noValueFallback = "";

        static renderInternalReference(detailsType: string, detailsState: string, stateId: number, value: string) {
            if (!!value) {
                return `<a data-element-type="${detailsType}" data-ui-sref="${detailsState}({id: ${stateId}})">${value}</a>`;
            } else {
                return RenderFieldsHelper.noValueFallback;
            }
        }

        static renderInternalReferenceFromModal(detailsType: string, detailsState: string, stateId: number, value: string) {
            if (!!value && !!stateId) {
                return `<a ng-click="$dismiss()" data-element-type="${detailsType}" data-ui-sref="${detailsState}({id: ${stateId}})">${value}</a>`;
            }
            else {
                return RenderFieldsHelper.noValueFallback;
            }
        }

        static renderUrlWithTitle(title: string | null | undefined, url: string) {
            if (title === null || _.isUndefined(title) || title.trim() === "") {
                title = url;
            }
            if (Utility.Validation.isValidExternalReference(url)) {
                return `<a target="_blank" style="float:left;" href="${url}">${title}</a>`;
            }
            return title || this.noValueFallback;
        }

        static renderReference(referenceTitle: string, referenceUrl: string) {
            return RenderFieldsHelper.renderUrlWithTitle(referenceTitle, referenceUrl);
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

        static renderDate(date: Date | string | undefined | null) {
            return ExcelExportHelper.renderDate(date);
        }

        static renderBoolean(value: boolean) {
            return ExcelExportHelper.renderBoolean(value);
        }

        static renderActiveNotActive(value: boolean) {
            return value ? RenderFieldsHelper.texts.status.other.active : RenderFieldsHelper.texts.status.other.notActive;
        }

        static renderActiveNotActiveSystem(value: boolean) {
            return value ? RenderFieldsHelper.texts.status.systems.active : RenderFieldsHelper.texts.status.systems.notActive;
        }

        static getTexts(): StatusTexts {
            return RenderFieldsHelper.texts;
        }

        private static readonly texts = {
            status: {
                systems: {
                    active: "Aktivt",
                    notActive: "Ikke aktivt"
                },
                other: {
                    active: "Aktiv",
                    notActive: "Ikke aktiv"
                }
            }
        }
    }
}
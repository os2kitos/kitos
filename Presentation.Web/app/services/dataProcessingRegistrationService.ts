﻿module Kitos.Services.DataProcessing {
    export interface IDataProcessingRegistrationService {
        create(organizationId: number, name: string): angular.IPromise<IDataProcessingRegistrationCreatedResult>;
        delete(dataProcessingRegistrationId: number): angular.IPromise<IDataProcessingRegistrationDeletedResult>;
        rename(dataProcessingRegistrationId: number, name: string): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        get(dataProcessingRegistrationId: number): angular.IPromise<Models.DataProcessing.IDataProcessingRegistrationDTO>;
        setMasterReference(dataProcessingRegistrationId: number, referenceId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        assignSystem(dataProcessingRegistrationId: number, systemId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        removeSystem(dataProcessingRegistrationId: number, systemId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        getAvailableSystems(dataProcessingRegistrationId: number, query: string): angular.IPromise<Models.Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[]>;
        getAvailableRoles(dataProcessingRegistrationId: number): angular.IPromise<Models.DataProcessing.IDataProcessingRoleDTO[]>;
        getApplicableUsers(dataProcessingRegistrationId: number, roleId: number, nameOrEmailContent: string): angular.IPromise<Models.DataProcessing.ISimpleUserDTO[]>;
        assignNewRole(dataProcessingRegistrationId: number, roleId: number, userId: number): angular.IPromise<void>;
        removeRole(dataProcessingRegistrationId: number, roleId: number, userId: number): angular.IPromise<void>;
        removeDataProcessor(dataProcessingRegistrationId: number, dataProcessorId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        assignDataProcessor(dataProcessingRegistrationId: number, dataProcessorId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        getApplicableDataProcessors(dataProcessingRegistrationId: number, query: string, pageSize: number): angular.IPromise<Models.DataProcessing.IDataProcessorDTO[]>;
        removeSubDataProcessor(dataProcessingRegistrationId: number, dataProcessorId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        updateSubDataProcessorsState(dataProcessingRegistrationId: number, state: Models.Api.Shared.YesNoUndecidedOption): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        assignSubDataProcessor(dataProcessingRegistrationId: number, dataProcessorId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        getApplicableSubDataProcessors(dataProcessingRegistrationId: number, query: string, pageSize: number): angular.IPromise<Models.DataProcessing.IDataProcessorDTO[]>;
        updateIsAgreementConcluded(dataProcessingRegistrationId: number, value: Models.Api.Shared.YesNoIrrelevantOption): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        updateAgreementConcludedAt(dataProcessingRegistrationId: number, dateTime: string): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        updateAgreementConcludedRemark(dataProcessingRegistrationId: number, remark: string): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        updateTransferToInsecureThirdCountry(dataProcessingRegistrationId: number, value: Models.Api.Shared.YesNoUndecidedOption): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        removeInsecureThirdCountry(dataProcessingRegistrationId: number, countryId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        assignInsecureThirdCountry(dataProcessingRegistrationId: number, countryId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        assignBasisForTransfer(dataProcessingRegistrationId: number, basisForTransferId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        clearBasisForTransfer(dataProcessingRegistrationId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        updateOversightInterval(dataProcessingRegistrationId: number, oversightInterval: Models.Api.Shared.YearMonthUndecidedIntervalOption): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        updateOversightIntervalRemark(dataProcessingRegistrationId: number, remark: string): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        assignDataResponsible(dataProcessingRegistrationId: number, dataResponsibleId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        clearDataResponsible(dataProcessingRegistrationId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        updateDataResponsibleRemark(dataProcessingRegistrationId: number, remark: string): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        getApplicableDataProcessingRegistrationOptions(dataProcessingRegistrationId: number): angular.IPromise<Models.DataProcessing.IDataProcessingRegistrationOptions>;
        assignOversightOption(dataProcessingRegistrationId: number, oversightOptionId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        removeOversightOption(dataProcessingRegistrationId: number, oversightOptionId: number): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        updateOversightOptionRemark(dataProcessingRegistrationId: number, remark: string): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        updateOversightCompleted(dataProcessingRegistrationId: number, isOversightCompleted: Models.Api.Shared.YesNoUndecidedOption): angular.IPromise<IDataProcessingRegistrationPatchResult>;
        updateOversightCompletedRemark(dataProcessingRegistrationId: number, remark: string): angular.IPromise<IDataProcessingRegistrationPatchResult>;

        assignOversightDate(dataProcessingRegistrationId: number, dateTime: string, remark: string): angular.IPromise<IOversightDateResult>;
        updateOversightDate(dataProcessingRegistrationId: number, oversightDateId: number, dateTime: string, remark: string): angular.IPromise<IOversightDateResult>;
        removeOversightDate(dataProcessingRegistrationId: number, oversightDateId: number): angular.IPromise<IOversightDateResult>;
    }

    export interface IDataProcessingRegistrationCreatedResult {
        createdObjectId: number;
    }

    export interface IDataProcessingRegistrationDeletedResult {
        deletedObjectId: number;
    }

    export interface IDataProcessingRegistrationPatchResult {
        valueModifiedTo: any;
        optionalServerDataPush?: Models.DataProcessing.IDataProcessingRegistrationDTO;
    }

    export interface IOversightDateResult {
        id: number;
        oversightDate: string;
        oversightRemark: string;
    }

    export class DataProcessingRegistrationService implements IDataProcessingRegistrationService {

        //Use for contracts that take an input defined as SingleValueDTO
        private simplePatch(url: string, value: any): angular.IPromise<IDataProcessingRegistrationPatchResult> {

            const payload = {
                Value: value
            };

            return this
                .$http
                .patch<API.Models.IApiWrapper<any>>(url, payload)
                .then(
                    response => {
                        var res = response.data as { response: Models.DataProcessing.IDataProcessingRegistrationDTO };
                        return <IDataProcessingRegistrationPatchResult>{
                            valueModifiedTo: value,
                            optionalServerDataPush: res.response
                        };
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        rename(dataProcessingRegistrationId: number, name: string): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "name"), name);
        }

        delete(dataProcessingRegistrationId: number): angular.IPromise<IDataProcessingRegistrationDeletedResult> {

            return this
                .$http
                .delete<API.Models.IApiWrapper<any>>(this.getUri(dataProcessingRegistrationId.toString()))
                .then(
                    response => {
                        return <IDataProcessingRegistrationDeletedResult>{
                            deletedObjectId: response.data.response.id,
                        };
                    },
                    error => this.apiWrapper.handleServerError(error)
                );

        }

        create(organizationId: number, name: string): angular.IPromise<IDataProcessingRegistrationCreatedResult> {
            const payload = {
                name: name,
                organizationId: organizationId
            };
            return this
                .$http
                .post<API.Models.IApiWrapper<any>>(this.getUri(""), payload)
                .then(
                    response => {
                        return <IDataProcessingRegistrationCreatedResult>{
                            createdObjectId: response.data.response.id
                        };
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }


        get(dataProcessingRegistrationId: number): angular.IPromise<Models.DataProcessing.IDataProcessingRegistrationDTO> {
            return this.apiWrapper.getDataFromUrl<Models.DataProcessing.IDataProcessingRegistrationDTO>(this.getUri(dataProcessingRegistrationId.toString()));
        }

        getApplicableDataProcessors(dataProcessingRegistrationId: number, query: string, pageSize = 25): angular.IPromise<Models.DataProcessing.IDataProcessorDTO[]> {
            return this.apiWrapper.getDataFromUrl<Models.DataProcessing.IDataProcessorDTO[]>(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "data-processors/available?nameQuery=" + encodeURIComponent(query) + `&pageSize=${pageSize}`));
        }

        getApplicableSubDataProcessors(dataProcessingRegistrationId: number, query: string, pageSize = 25): angular.IPromise<Models.DataProcessing.IDataProcessorDTO[]> {
            return this.apiWrapper.getDataFromUrl<Models.DataProcessing.IDataProcessorDTO[]>(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "sub-data-processors/available?nameQuery=" + encodeURIComponent(query) + `&pageSize=${pageSize}`));
        }

        setMasterReference(dataProcessingRegistrationId: number, referenceId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "master-reference"), referenceId);
        }

        assignSystem(dataProcessingRegistrationId: number, systemId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "it-systems/assign"), systemId);
        }
        removeSystem(dataProcessingRegistrationId: number, systemId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "it-systems/remove"), systemId);
        }

        removeDataProcessor(dataProcessingRegistrationId: number, dataProcessorId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "data-processors/remove"), dataProcessorId);
        }

        assignDataProcessor(dataProcessingRegistrationId: number, dataProcessorId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "data-processors/assign"), dataProcessorId);
        }

        updateSubDataProcessorsState(dataProcessingRegistrationId: number, state: Models.Api.Shared.YesNoUndecidedOption): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "sub-data-processors/state"), state as number);
        }

        removeSubDataProcessor(dataProcessingRegistrationId: number, dataProcessorId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "sub-data-processors/remove"), dataProcessorId);
        }

        assignSubDataProcessor(dataProcessingRegistrationId: number, dataProcessorId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "sub-data-processors/assign"), dataProcessorId);
        }

        updateTransferToInsecureThirdCountry(dataProcessingRegistrationId: number, value: Models.Api.Shared.YesNoUndecidedOption): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "insecure-third-countries/state"), value as number);
        }

        removeInsecureThirdCountry(dataProcessingRegistrationId: number, countryId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "insecure-third-countries/remove"), countryId);
        }

        assignInsecureThirdCountry(dataProcessingRegistrationId: number, countryId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "insecure-third-countries/assign"), countryId);
        }

        assignBasisForTransfer(dataProcessingRegistrationId: number, basisForTransferId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "basis-for-transfer/assign"), basisForTransferId);
        }

        clearBasisForTransfer(dataProcessingRegistrationId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "basis-for-transfer/clear"), {});
        }

        assignDataResponsible(dataProcessingRegistrationId: number, dataResponsibleId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "data-responsible/assign"), dataResponsibleId);
        }

        clearDataResponsible(dataProcessingRegistrationId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "data-responsible/clear"), {});
        }

        updateDataResponsibleRemark(dataProcessingRegistrationId: number, remark: string): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "data-responsible-remark"), remark);
        }

        getAvailableSystems(dataProcessingRegistrationId: number, query: string): angular.IPromise<Models.Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[]> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, `it-systems/available?nameQuery=${query}`))
                .then(
                    result => {
                        var response = result.data as { response: Models.Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[] }
                        return response.response;
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        getAvailableRoles(dataProcessingRegistrationId: number): angular.IPromise<Models.DataProcessing.IDataProcessingRoleDTO[]> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(this.getUriWithIdAndSuffix(dataProcessingRegistrationId,
                    "available-roles"))
                .then(
                    result => {
                        var response = result.data as { response: Models.DataProcessing.IDataProcessingRoleDTO[] }
                        return response.response;
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        getApplicableUsers(dataProcessingRegistrationId: number, roleId: number, nameOrEmailContent: string): angular.IPromise<Models.DataProcessing.ISimpleUserDTO[]> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(this.getUri(`${dataProcessingRegistrationId}/available-roles/${roleId}/applicable-users?nameOrEmailContent=${nameOrEmailContent}`))
                .then(
                    result => {
                        var response = result.data as { response: Models.DataProcessing.ISimpleUserDTO[] }
                        return response.response;
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        assignNewRole(dataProcessingRegistrationId: number, roleId: number, userId: number): angular.IPromise<void> {
            const payload = {
                RoleId: roleId,
                UserId: userId
            };
            return this
                .$http
                .patch<API.Models.IApiWrapper<any>>(
                    this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "roles/assign"),
                    payload)
                .then(
                    result => { },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        removeRole(dataProcessingRegistrationId: number, roleId: number, userId: number): angular.IPromise<void> {
            const payload = {
            };
            return this
                .$http
                .patch<API.Models.IApiWrapper<any>>(
                    this.getUriWithIdAndSuffix(dataProcessingRegistrationId, `roles/remove/${roleId}/from/${userId}`),
                    payload)
                .then(
                    result => { },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        updateIsAgreementConcluded(dataProcessingRegistrationId: number, value: Models.Api.Shared.YesNoIrrelevantOption) {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "agreement-concluded"), value);
        }

        updateAgreementConcludedAt(dataProcessingRegistrationId: number, dateString: string) {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "agreement-concluded-at"), dateString);
        }

        updateAgreementConcludedRemark(dataProcessingRegistrationId: number, remark: string) {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "agreement-concluded-remark"), remark);
        }

        updateOversightInterval(dataProcessingRegistrationId: number, oversightInterval: Models.Api.Shared.YearMonthUndecidedIntervalOption) {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "oversight-interval"), oversightInterval);
        }

        updateOversightIntervalRemark(dataProcessingRegistrationId: number, remark: string) {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "oversight-interval-remark"), remark);
        }

        getApplicableDataProcessingRegistrationOptions(organizationId: number): angular.IPromise<Models.DataProcessing.IDataProcessingRegistrationOptions> {
            return this.apiWrapper.getDataFromUrl<Models.DataProcessing.IDataProcessingRegistrationOptions>(this.getUri(`available-options-in/${organizationId}`));
        }

        assignOversightOption(dataProcessingRegistrationId: number, oversightOptionId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "oversight-option/assign"), oversightOptionId);
        }
        removeOversightOption(dataProcessingRegistrationId: number, oversightOptionId: number): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "oversight-option/remove"), oversightOptionId);
        }

        updateOversightOptionRemark(dataProcessingRegistrationId: number, remark: string): angular.IPromise<IDataProcessingRegistrationPatchResult> {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "oversight-option-remark"), remark);
        }

        updateOversightCompleted(dataProcessingRegistrationId: number, isOversightCompleted: Models.Api.Shared.YesNoUndecidedOption) {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "oversight-completed"), isOversightCompleted);
        }

        updateOversightCompletedRemark(dataProcessingRegistrationId: number, remark: string) {
            return this.simplePatch(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "oversight-completed-remark"), remark);
        }

        assignOversightDate(dataProcessingRegistrationId: number, dateTime: string, remark: string) {
            const payload = {
                oversightDate: dateTime,
                oversightRemark: remark
            };
            return this
                .$http
                .patch<API.Models.IApiWrapper<IOversightDateResult>>(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "oversight-date/assign"), payload)
                .then(
                    response => {
                        return response.data.response;
                    },
                    error => this.apiWrapper.handleServerError(error));
        }

        updateOversightDate(dataProcessingRegistrationId: number, oversightDateId: number, dateTime: string, remark: string) {
            var payload = {
                id: oversightDateId,
                oversightDate: dateTime,
                oversightRemark: remark
            };
            return this
                .$http
                .patch<API.Models.IApiWrapper<IOversightDateResult>>(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "oversight-date/modify"), payload)
                .then(
                    response => {
                        return response.data.response;
                    },
                    error => this.apiWrapper.handleServerError(error));
        }

        removeOversightDate(dataProcessingRegistrationId: number, oversightDateId: number) {
            var payload = {
                value: oversightDateId
            };
            return this
                .$http
                .patch<API.Models.IApiWrapper<IOversightDateResult>>(this.getUriWithIdAndSuffix(dataProcessingRegistrationId, "oversight-date/remove"), payload)
                .then(
                    response => {
                        return response.data.response;
                    },
                    error => this.apiWrapper.handleServerError(error));
        }

        static $inject = ["$http"];
        private readonly apiWrapper : Services.Generic.ApiWrapper;
        constructor(private readonly $http: ng.IHttpService) {
            this.apiWrapper = new Services.Generic.ApiWrapper($http);
        }
        
        private getUri(suffix: string): string {
            return this.getBaseUri() + `${suffix}`;
        }

        private getUriWithIdAndSuffix(id: number, suffix: string) {
            return this.getBaseUri() + `${id}/${suffix}`;
        }

        private getBaseUri() {
            return "api/v1/data-processing-registration/";
        }
    }

    app.service("dataProcessingRegistrationService", DataProcessingRegistrationService);
}
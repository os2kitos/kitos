module Kitos.Helpers {
    export class OrganizationRegistrationHelper {
        static createChangeRequest(
            contractRegistrations: Models.ViewModel.Organization.IOrganizationUnitRegistration[],
            externalPayments: Models.ViewModel.Organization.IOrganizationUnitRegistration[],
            internalPayments: Models.ViewModel.Organization.IOrganizationUnitRegistration[],
            roles: Models.ViewModel.Organization.IOrganizationUnitRegistration[],
            relevantSystems: Models.ViewModel.Organization.IOrganizationUnitRegistration[],
            responsibleSystems: Models.ViewModel.Organization.IOrganizationUnitRegistration[]): Models.Api.Organization.OrganizationUnitRegistrationChangeRequestDto {

            const changeRequest = new Models.Api.Organization.OrganizationUnitRegistrationChangeRequestDto();

            changeRequest.itContractRegistrations = OrganizationRegistrationHelper.mapSelectedOptionsToIds(contractRegistrations);
            changeRequest.paymentRegistrationDetails = OrganizationRegistrationHelper.createPaymentChangeRequest(externalPayments, internalPayments);
            changeRequest.organizationUnitRights = OrganizationRegistrationHelper.mapSelectedOptionsToIds(roles);
            changeRequest.relevantSystems = OrganizationRegistrationHelper.mapSelectedOptionsToIds(relevantSystems);
            changeRequest.responsibleSystems = OrganizationRegistrationHelper.mapSelectedOptionsToIds(responsibleSystems);

            return changeRequest;
        }

        static createTransferRequest(
            targetUnitUuid: string,
            contractRegistrations: Models.ViewModel.Organization.IOrganizationUnitRegistration[],
            externalPayments: Models.ViewModel.Organization.IOrganizationUnitRegistration[],
            internalPayments: Models.ViewModel.Organization.IOrganizationUnitRegistration[],
            roles: Models.ViewModel.Organization.IOrganizationUnitRegistration[],
            relevantSystems: Models.ViewModel.Organization.IOrganizationUnitRegistration[],
            responsibleSystems: Models.ViewModel.Organization.IOrganizationUnitRegistration[]): Models.Api.Organization.TransferOrganizationUnitRegistrationRequestDto {

            const request = OrganizationRegistrationHelper.createChangeRequest(contractRegistrations,
                externalPayments,
                internalPayments,
                roles,
                relevantSystems,
                responsibleSystems) as Models.Api.Organization.TransferOrganizationUnitRegistrationRequestDto;
            request.targetUnitUuid = targetUnitUuid;

            return request;
        }

        static mapSelectedOptionsToIds(options: Array<Models.ViewModel.Organization.IOrganizationUnitRegistration>): Array<number> {
            return options.filter(x => x.selected)
                .map(item => item.id);
        }

        private static createPaymentChangeRequest(
            externalPayments: Models.ViewModel.Organization.IOrganizationUnitRegistration[],
            internalPayments: Models.ViewModel.Organization.IOrganizationUnitRegistration[]): Models.Api.Organization.PaymentRegistrationChangeRequestDto[] {

            const selectedExternPayments = externalPayments.filter(x => x.selected);
            const selectedInternPayments = internalPayments.filter(x => x.selected);
            const selectedPayments = selectedExternPayments.concat(selectedInternPayments);

            const contractIds = selectedPayments.map(x => x.optionalObjectContext.id)
                .filter((value, index, self) => self.indexOf(value) === index) as number[];

            var payments = [];
            contractIds.forEach(contractId => {
                var payment = new Models.Api.Organization.PaymentRegistrationChangeRequestDto();
                payment.itContractId = contractId;
                payment.externalPayments = this.mapSelectedOptionsToIds(selectedExternPayments.filter(x => x.optionalObjectContext.id === contractId));
                payment.internalPayments = this.mapSelectedOptionsToIds(selectedInternPayments.filter(x => x.optionalObjectContext.id === contractId));

                payments.push(payment);
            });

            return payments;
        }
    }
}
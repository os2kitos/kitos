module Kitos.Helpers {
    export class OrganizationRegistrationHelper {
        static createChangeRequest(
            contractRegistrations: Models.Organization.IOrganizationUnitRegistration[],
            externalPayments: Models.Organization.IOrganizationUnitRegistration[],
            internalPayments: Models.Organization.IOrganizationUnitRegistration[],
            roles: Models.Organization.IOrganizationUnitRegistration[],
            relevantSystems: Models.Organization.IOrganizationUnitRegistration[],
            responsibleSystems: Models.Organization.IOrganizationUnitRegistration[]): Models.Api.Organization.OrganizationRegistrationChangeRequest {

            const changeRequest = new Models.Api.Organization.OrganizationRegistrationChangeRequest();

            changeRequest.itContractRegistrations = OrganizationRegistrationHelper.mapSelectedOptionsToIds(contractRegistrations);
            changeRequest.paymentRegistrationDetails = OrganizationRegistrationHelper.createPaymentChangeRequest(externalPayments, internalPayments);
            changeRequest.organizationUnitRights = OrganizationRegistrationHelper.mapSelectedOptionsToIds(roles);
            changeRequest.relevantSystems = OrganizationRegistrationHelper.mapSelectedOptionsToIds(relevantSystems);
            changeRequest.responsibleSystems = OrganizationRegistrationHelper.mapSelectedOptionsToIds(responsibleSystems);

            return changeRequest;
        }

        static mapSelectedOptionsToIds(options: Array<Models.Organization.IOrganizationUnitRegistration>): Array<number> {
            return options.filter(x => x.selected)
                .map(item => item.id);
        }

        private static createPaymentChangeRequest(
            externalPayments: Models.Organization.IOrganizationUnitRegistration[],
            internalPayments: Models.Organization.IOrganizationUnitRegistration[]): Models.Api.Organization.PaymentRegistrationChangeRequest[] {

            const selectedExternPayments = externalPayments.filter(x => x.selected);
            const selectedInternPayments = internalPayments.filter(x => x.selected);
            const selectedPayments = selectedExternPayments.concat(selectedInternPayments);

            const contractIds = selectedPayments.map(x => x.optionalObjectContext.id)
                .filter((value, index, self) => self.indexOf(value) === index) as number[];

            var payments = [];
            contractIds.forEach(contractId => {
                var payment = new Models.Api.Organization.PaymentRegistrationChangeRequest();
                payment.itContractId = contractId;
                payment.externalPayments = this.mapSelectedOptionsToIds(selectedExternPayments.filter(x => x.optionalObjectContext.id === contractId));
                payment.internalPayments = this.mapSelectedOptionsToIds(selectedInternPayments.filter(x => x.optionalObjectContext.id === contractId));

                payments.push(payment);
            });

            return payments;
        }
    }
}
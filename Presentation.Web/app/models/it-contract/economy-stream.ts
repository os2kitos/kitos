module Kitos.Models.ItContract {
    export interface IEconomyStream extends IEntity {
        /** The EconomyStream might be an extern payment for a contract. */
        ExternPaymentFor: IItContract;
        ExternPaymentForId: number;
        /** The EconomyStream might be an intern payment for a contract. */
        InternPaymentFor: IItContract;
        InternPaymentForId: number;
        /** Gets or sets the associated organization unit identifier. */
        OrganizationUnitId: number;
        /** Gets or sets the organization unit. */
        OrganizationUnit: IOrganizationUnit;
        /** The field "anskaffelse". */
        Acquisition: number;
        /** The field "drift/år". */
        Operation: number;
        Other: number;
        /** The field "kontering". */
        AccountingEntry: string;
        /** Traffic light for audit. */
        AuditStatus: any;
        /** DateTime for audit. */
        AuditDate: Date;
        /** Gets or sets the note. */
        Note: string;
    }
}

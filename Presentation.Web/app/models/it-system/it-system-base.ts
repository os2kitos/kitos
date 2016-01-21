module Kitos.Models.ItSystem {
    export interface IItSystemBase extends IEntity {
        Name: string;
        Uuid: any;
        Description: string;
        Url: string;
        AccessModifier: AccessModifier;
        OrganizationId: number;
        /** Gets or sets the organization this instance was created under. */
        Organization: IOrganization;
        BelongsToId: number;
        /** Gets or sets the organization the system belongs to. */
        BelongsTo: IOrganization;
    }
}

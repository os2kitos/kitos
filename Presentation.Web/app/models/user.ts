module Kitos.Models {
    export interface IUser extends IEntity {
        Name?: string;
        LastName?: string;
        PhoneNumber?: string;
        Email?: string;
        IsGlobalAdmin?: boolean;
        HasApiAccess?: boolean;
        Uuid?: any;
        LastAdvisDate?: Date;
        /** The admin rights of the user */
        OrganizationRights?: IOrganizationRight[];

        OrganizationUnitRights?: IOrganizationUnitRight[];
        /** Passwords reset request issued for the user */
        PasswordResetRequests?: IPasswordResetRequest[];
        /** The contracts that the user has been marked as contract signer for */
        SignerForContracts?: ItContract.IItContract[];
        /** Stakeholder access */
        HasStakeHolderAccess: boolean;
        DefaultUserStartPreference?: string;
    }

    export interface IContactPerson extends IEntity {
        Name?: string;
        LastName?: string;
        PhoneNumber?: string;
        Email?: string;
    }

    export interface ICreateUser {
        Name: string;
        LastName: string;
        Email: string;
        PhoneNumber?: string;
        IsGlobalAdmin?: boolean;
        HasApiAccess?: boolean;
        HasStakeHolderAccess?: boolean;
        DefaultUserStartPreference?: string;
    }

    export interface ICreateUserPayload {
        user: ICreateUser;
        organizationId: number;
        sendMailOnCreation?: boolean;
    }
}

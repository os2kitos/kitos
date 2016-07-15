module Kitos.Models {
    export interface IUser {
        Name: string;
        LastName: string;
        PhoneNumber: string;
        Email: string;
        Password: string;
        Salt: string;
        IsGlobalAdmin: boolean;
        Uuid: any;
        LastAdvisDate: Date;
        /** The admin rights of the user */
        OrganizationRights: Array<IAdminRight>;
        /** Passwords reset request issued for the user */
        PasswordResetRequests: Array<IPasswordResetRequest>;
        /** Wishes created by this user */
        Wishes: Array<ItSystem.IWish>;
        /** Gets or sets the  or  associated with this user */
        ItProjectStatuses: Array<ItProject.IItProjectStatus>;
        /** Risks associated with this user */
        ResponsibleForRisks: Array<ItProject.IRisk>;
        /** Communications associated with this user */
        ResponsibleForCommunications: Array<ItProject.ICommunication>;
        /** Handovers associated with this user */
        HandoverParticipants: Array<ItProject.IHandover>;
        /** The contracts that the user has been marked as contract signer for */
        SignerForContracts: Array<ItContract.IItContract>;
    }
}

module Kitos.Models {
    export interface IEntity {
        /** Gets or sets the primary identifier. */
        Id?: number;
        /** Gets or sets the object owner  identifier. */
        ObjectOwnerId?: number;
        /** Gets or sets the  that owns this instance. */
        ObjectOwner?: IUser;
        /** Gets or sets the DateTime of when the last change occurred to this instance. */
        LastChanged?: Date;
        /** Gets or sets the User identifier for */
        LastChangedByUserId?: number;
        /** Gets or sets the User which made the most recent change to this instance. */
        LastChangedByUser?: IUser;
    }
}

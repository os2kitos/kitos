module Kitos.Models.ItSystem {
    export interface IWish extends IEntity {
        /** Gets or sets a value indicating whether this instance is public. */
        IsPublic: boolean;
        /** Gets or sets the wish text. */
        Text: string;
        UserId: number;
        /** Gets or sets the user that made the wish. */
        User: IUser;
        ItSystemUsageId: number;
        /** Gets or sets it system which this wish concerns. */
        ItSystemUsage: ItSystemUsage.IItSystemUsage;
    }
}

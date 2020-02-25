module Kitos.Models.ItSystem {
    export interface IItSystem extends IItSystemBase {
        /** Gets or sets the user defined system identifier. */
        ItSystemId: string;
        /** Gets or sets exhibited interfaces for this instance. */
        ItInterfaceExhibits: Array<IItInterfaceExhibit>;
        /** Gets or sets interfaces that can use this instance. */
        CanUseInterfaces: Array<IItInterfaceUse>;
        /** Gets or sets the sub (child) it systems. */
        Children: Array<IItSystem>;
        ParentId: number;
        Disabled: boolean;
        /** Gets or sets the parent (master) it system. */
        Parent: IItSystem;
        BusinessTypeId: number;
        /** Gets or sets the type of the business option. */
        BusinessType: IBusinessType;
        Wishes: Array<IWish>;
        TaskRefs: Array<ITaskRef>;
        /** Gets or sets the usages. */
        Usages: Array<ItSystemUsage.IItSystemUsage>;
        Reference: any;
        PreviousName: string;
    }
}

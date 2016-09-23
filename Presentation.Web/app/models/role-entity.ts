module Kitos.Models {
    export interface IRoleEntity extends IOptionEntity {
        HasReadAccess: boolean;
        HasWriteAccess: boolean;
    }
}
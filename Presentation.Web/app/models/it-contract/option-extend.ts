﻿module Kitos.Models.ItContract {
    export interface IOptionExtend extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}

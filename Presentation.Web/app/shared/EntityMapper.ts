module Kitos.Services.LocalOptions {
    "use strict";

    export interface IOptionalDescription {
        description: string
    }

    export interface IWriteAccess {
        hasWriteAccess: boolean
    }

    export interface IEntityMapper{
        mapOptionToSelect2ViewModel(input: Models.IOptionEntity[]): Models.ViewModel.Generic.Select2OptionViewModel<IOptionalDescription>[];
        mapRoleToSelect2ViewModel(input: Models.IOptionEntity[]): Models.ViewModel.Generic.Select2OptionViewModel<IWriteAccess>[];
    }

    class EntityMapper implements IEntityMapper {

        mapOptionToSelect2ViewModel(input: Models.IOptionEntity[]): Models.ViewModel.Generic.Select2OptionViewModel<IOptionalDescription>[] {
            return _.map(input, this.mapOption);
        }

        mapRoleToSelect2ViewModel(input: Models.IOptionEntity[]): Models.ViewModel.Generic.Select2OptionViewModel<IWriteAccess>[] {
            return _.map(input, this.mapRole);
        }

        private mapOption(input: Models.IOptionEntity): Models.ViewModel.Generic.Select2OptionViewModel<IOptionalDescription> {
            return {
                id: input.Id, text: input.Name, optionalObjectContext: { description: input.Description }}
        }

        private mapRole(input: Models.IRoleEntity): Models.ViewModel.Generic.Select2OptionViewModel<IWriteAccess> {
            return {
                id: input.Id, text: input.Name, optionalObjectContext: { hasWriteAccess: input.HasWriteAccess }
            }
        }
    }

    app.service("entityMapper", EntityMapper);
}
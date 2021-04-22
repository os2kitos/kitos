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
        mapRoleToSelect2ViewModel(input: Models.IRoleEntity[]): Models.ViewModel.Generic.Select2OptionViewModel<IWriteAccess>[];
        mapApiResponseToSelect2ViewModel(input: any): Models.ViewModel.Generic.Select2OptionViewModel<null>[];
    }

    class EntityMapper implements IEntityMapper {

        mapOptionToSelect2ViewModel(input: Models.IOptionEntity[]): Models.ViewModel.Generic.Select2OptionViewModel<IOptionalDescription>[] {
            return _.map(input, this.mapOption);
        }

        mapRoleToSelect2ViewModel(input: Models.IRoleEntity[]): Models.ViewModel.Generic.Select2OptionViewModel<IWriteAccess>[] {
            return _.map(input, this.mapRole);
        }

        mapApiResponseToSelect2ViewModel(input: any): Models.ViewModel.Generic.Select2OptionViewModel<null>[] {
            return _.map(input, this.mapApiResponse);
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

        private mapApiResponse(input: any): Models.ViewModel.Generic.Select2OptionViewModel<null> {
            return {
                id: input.id, text: input.name
            }
        }
    }

    app.service("entityMapper", EntityMapper);
}
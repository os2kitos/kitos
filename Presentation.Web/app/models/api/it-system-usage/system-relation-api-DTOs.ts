module Kitos.Models.Api.ItSystemUsage.Relation {
    import NamedEntityDTO = Models.Generic.NamedEntity.NamedEntityDTO;
    import IItSystemUsageRelationIdName = Models.ViewModel.ItSystemUsage.Relation.IItSystemUsageRelationIdName;
    import ISystemRelationModalViewModel = Models.ViewModel.ItSystemUsage.Relation.ISystemRelationModalViewModel;
    import ISystemRelationSelectionModel = Models.ViewModel.ItSystemUsage.Relation.ISystemRelationSelectionModel;
    import SystemRelationModalViewModel = Models.ViewModel.ItSystemUsage.Relation.SystemRelationModalViewModel;
    import Select2OptionViewModel = Models.ViewModel.Generic.Select2OptionViewModel;

    export interface IItSystemUsageCreateRelationDTO {
        FromUsageId: number;
        ToUsageId: number;
        Description: string;
        InterfaceId?: number;
        FrequencyTypeId?: number;
        ContractId?: number;
        Reference: string;
    }

    export interface IItSystemUsageRelationDTO {
        id: number;
        fromUsage: NamedEntityDTO;
        toUsage: NamedEntityDTO;
        interface: NamedEntityDTO;
        contract: NamedEntityDTO;
        frequencyType: NamedEntityDTO;
        description: string;
        reference: string;
    }

    export interface IItSystemUsageRelationOptionsDTO {
        availableInterfaces: [NamedEntityDTO];
        availableContracts: [NamedEntityDTO];
        availableFrequencyTypes: [NamedEntityDTO];
    }

    export interface IItSystemUsageRelationReferenceDTO {
        Reference: string;
        ValidUrl: boolean;
        LongText: boolean;
        ShortTextLineCount: number;
    }

    export interface IItSystemUsageRelationDescriptionDTO {
        Description: string;
        LongText: boolean;
        ShortTextLineCount: number;
    }

    export interface ISystemRelationPatchDTO {
        Id: number;
        Description: string;
        Reference: string;
        FromUsage: IItSystemUsageRelationIdName;
        ToUsage: IItSystemUsageRelationIdName;
        Interface?: IItSystemUsageRelationIdName;
        Contract?: IItSystemUsageRelationIdName;
        FrequencyType?: IItSystemUsageRelationIdName;

    }

    export class SystemRelationModelPostDataObject implements IItSystemUsageCreateRelationDTO {
        ToUsageId: number;
        FromUsageId: number;
        Description: string;
        InterfaceId?: number;
        FrequencyTypeId?: number;
        ContractId?: number;
        Reference: string;

        constructor(relationModelToCreate: ISystemRelationModalViewModel) {

            this.ToUsageId = relationModelToCreate.toSystem.id;
            this.FromUsageId = relationModelToCreate.fromSystem.id;
            this.Description = relationModelToCreate.description.text;
            this.Reference = relationModelToCreate.reference.text;

            this.InterfaceId = this.getIdFromValues(relationModelToCreate.interface);
            this.ContractId = this.getIdFromValues(relationModelToCreate.contract);
            this.FrequencyTypeId = this.getIdFromValues(relationModelToCreate.frequency);
        }

        private getIdFromValues(valuesToInsert: ISystemRelationSelectionModel) {
            if (valuesToInsert.value !== null) {
                return valuesToInsert.value.id;
            } else {
                return null;
            }
        }
    }

    export class SystemRelationModelPatchDataObject implements ISystemRelationPatchDTO {
        Id: number;
        Description: string;
        Reference: string;
        FromUsage: IItSystemUsageRelationIdName;
        ToUsage: IItSystemUsageRelationIdName;
        Interface?: IItSystemUsageRelationIdName;
        Contract?: IItSystemUsageRelationIdName;
        FrequencyType?: IItSystemUsageRelationIdName;

        constructor(data: SystemRelationModalViewModel) {

            this.Id = data.id;
            this.Description = data.description.text;
            this.Reference = data.reference.text;
            this.FromUsage = { Id: data.fromSystem.id, Name: data.fromSystem.name };
            this.ToUsage = { Id: data.toSystem.id, Name: data.toSystem.text };
            this.FrequencyType = this.setValuesOrNull(data.frequency.value);
            this.Interface = this.setValuesOrNull(data.interface.value);
            this.Contract = this.setValuesOrNull(data.contract.value);

        }

        private setValuesOrNull(value: Select2OptionViewModel) {
            if (value !== null) {
                return <IItSystemUsageRelationIdName>{ Id: value.id, Name: value.text }
            } else {
                return null;
            }
        }
    }

}
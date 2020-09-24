module Kitos.Models.ViewModel.ItSystemUsage.Relation {
    import NamedEntityDTO = Models.Generic.NamedEntity.NamedEntityDTO;
    import Select2OptionViewModel = Models.ViewModel.Generic.Select2OptionViewModel;
    import IGenericDescriptionViewModel = Models.ViewModel.Generic.IGenericDescriptionViewModel;
    import IItSystemUsageRelationReferenceDto = Models.Api.ItSystemUsage.Relation.IItSystemUsageRelationReferenceDTO;
    import IItSystemUsageRelationDTO = Models.Api.ItSystemUsage.Relation.IItSystemUsageRelationDTO;
    import IItSystemUsageRelationOptionsDTO = Models.Api.ItSystemUsage.Relation.IItSystemUsageRelationOptionsDTO;
    import NamedEntityWithEnabledStatusDTO = Kitos.Models.Generic.NamedEntity.NamedEntityWithEnabledStatusDTO;

    export interface IItSystemUsageRelationIdName {
        Id: number;
        Name: string;
    }

    export interface IItSystemUsageRelationIdNameWithEnabledStatus {
        Id: number;
        Name: string;
        Disabled : boolean;
    }

    export interface ISystemRelationSelectionModel {
        value?: Select2OptionViewModel<any>;
        options: Select2OptionViewModel<any>[];
    }

    export interface ISystemRelationModalViewModel {
        fromSystem: NamedEntityWithEnabledStatusDTO;
        toSystem: Select2OptionViewModel<any>;
        interface: ISystemRelationSelectionModel;
        contract: ISystemRelationSelectionModel;
        frequency: ISystemRelationSelectionModel;
        reference: Select2OptionViewModel<any>;
        description: Select2OptionViewModel<any>;
    }

    export interface ISystemRelationViewModel {
        RelationId: number;
        FromSystem: IItSystemUsageRelationIdNameWithEnabledStatus;
        ToSystem: IItSystemUsageRelationIdNameWithEnabledStatus;
        Interface?: IItSystemUsageRelationIdName;
        Description: IGenericDescriptionViewModel;
        Reference: IItSystemUsageRelationReferenceDto;
        Contract?: IItSystemUsageRelationIdName;
        Frequency?: IItSystemUsageRelationIdName;
    }

    export class SystemRelationViewModel implements ISystemRelationViewModel {
        RelationId: number;
        FromSystem: IItSystemUsageRelationIdNameWithEnabledStatus;
        ToSystem: IItSystemUsageRelationIdNameWithEnabledStatus;
        Interface?: IItSystemUsageRelationIdName;
        Description: IGenericDescriptionViewModel;
        Reference: IItSystemUsageRelationReferenceDto;
        Contract?: IItSystemUsageRelationIdName;
        Frequency?: IItSystemUsageRelationIdName;

        private mapNameAndId(viewModel: IItSystemUsageRelationIdName, sourceModel: Models.Generic.NamedEntity.NamedEntityDTO) {
            if (sourceModel != null) {
                viewModel.Name = sourceModel.name;
                viewModel.Id = sourceModel.id;
            }
        }

        private mapSystemUsage(viewModel: IItSystemUsageRelationIdNameWithEnabledStatus, sourceModel: NamedEntityWithEnabledStatusDTO) {
            if (sourceModel != null) {
                this.mapNameAndId(viewModel, sourceModel);
                viewModel.Disabled = sourceModel.disabled;
                viewModel.Name = Helpers.SystemNameFormat.apply(viewModel.Name,viewModel.Disabled);
            }
        }

        private isValidHyperLink(ref: string) {
            if (ref !== null) {
                return Utility.Validation.validateUrl(ref);
            }
            return false;
        }

        constructor(
            maxTextFieldCharCount: number,
            shortTextLineCount: number,
            currentRelation: any) {

            this.RelationId = currentRelation.id;

            // From System
            this.FromSystem = <IItSystemUsageRelationIdNameWithEnabledStatus>{};
            this.mapSystemUsage(this.FromSystem, currentRelation.fromUsage);

            // To System
            this.ToSystem = <IItSystemUsageRelationIdNameWithEnabledStatus>{};
            this.mapSystemUsage(this.ToSystem, currentRelation.toUsage);

            // interface
            if (currentRelation.interface != null) {
                this.Interface = <IItSystemUsageRelationIdName>{};
                this.mapNameAndId(this.Interface, currentRelation.interface);
            }
            // Description

            var des = currentRelation.description;
            if (des != null) {
                this.Description = <IGenericDescriptionViewModel>{};
                this.Description.description = des;
                this.Description.longText = des.length > maxTextFieldCharCount;
                this.Description.shortTextLineCount = shortTextLineCount;
            }

            //Reference
            const reference = currentRelation.reference;
            if (reference != null) {
                this.Reference = <IItSystemUsageRelationReferenceDto>{};
                if (this.isValidHyperLink(reference)) {
                    this.Reference.ValidUrl = true;
                    this.Reference.Reference = reference;
                } else {
                    this.Reference.Reference = reference;
                    this.Reference.LongText = reference.length > maxTextFieldCharCount;
                    this.Reference.ShortTextLineCount = shortTextLineCount;
                }
            }
            // Contract
            if (currentRelation.contract != null) {
                this.Contract = <IItSystemUsageRelationIdName>{};
                this.mapNameAndId(this.Contract, currentRelation.contract);
            }
            // Frequency
            if (currentRelation.frequencyType !== null) {
                this.Frequency = <IItSystemUsageRelationIdName>{};
                this.mapNameAndId(this.Frequency, currentRelation.frequencyType);
            }
        }
    }

    export class SystemRelationModalViewModel implements ISystemRelationModalViewModel {
        id: number;
        headerText: string;
        isEditDialog: boolean;
        fromSystem: NamedEntityWithEnabledStatusDTO;
        toSystem: Select2OptionViewModel<any>;
        interface: ISystemRelationSelectionModel;
        contract: ISystemRelationSelectionModel;
        frequency: ISystemRelationSelectionModel;
        reference: Select2OptionViewModel<any>;
        description: Select2OptionViewModel<any>;

        constructor(fromSystemId: number, fromSystemName: string, disabled : boolean) {
            this.fromSystem = <NamedEntityWithEnabledStatusDTO>{ id: fromSystemId, name: fromSystemName, disabled : disabled };
            this.toSystem = null;
            this.interface = <ISystemRelationSelectionModel>{ value: null, options: [] };
            this.contract = <ISystemRelationSelectionModel>{ value: null, options: [] };
            this.frequency = <ISystemRelationSelectionModel>{ value: null, options: [] };
            this.reference = <Select2OptionViewModel<any>>{};
            this.description = <Select2OptionViewModel<any>>{};
        }

        configureAsNewRelationDialog() {
            this.headerText = "Opret relation";
            this.isEditDialog = false;
        }

        configureAsEditRelationDialog(relationData: IItSystemUsageRelationDTO, optionsResult: IItSystemUsageRelationOptionsDTO) {
            this.toSystem = <Select2OptionViewModel<any>>{ id: relationData.toUsage.id, text: Helpers.SystemNameFormat.apply(relationData.toUsage.name, relationData.toUsage.disabled), disabled: relationData.toUsage.disabled };
            this.updateAvailableOptions(optionsResult);

            this.bindValue(this.frequency, relationData.frequencyType);
            this.bindValue(this.contract, relationData.contract);
            this.bindValue(this.interface, relationData.interface);
            this.id = relationData.id;
            this.description.text = relationData.description;
            this.reference.text = relationData.reference;
            this.headerText = "Rediger relation";
            this.isEditDialog = true;
        }

        updateAvailableOptions(optionsResult: IItSystemUsageRelationOptionsDTO) {
            // Build modal with data
            this.bindOptions(this.frequency, optionsResult.availableFrequencyTypes);
            this.bindOptions(this.interface, optionsResult.availableInterfaces);
            this.bindOptions(this.contract, optionsResult.availableContracts);
        }

        private bindValue(targetData: ISystemRelationSelectionModel, sourceData: NamedEntityDTO) {
            if (sourceData) {
                targetData.value = <Select2OptionViewModel<any>>{ id: sourceData.id, text: sourceData.name };
            } else {
                targetData.value = null;
            }
        }

        private bindOptions(targetData: ISystemRelationSelectionModel, sourceData: NamedEntityDTO[]) {
            let selectedValue = targetData.value;
            targetData.options = _.map(sourceData, dto => <Select2OptionViewModel<any>>{ id: dto.id, text: dto.name });
            targetData.value = null;

            //Set selected value to previously selected value if it was selected before
            if (selectedValue) {
                for (let i = 0; i < targetData.options.length; i++) {

                    let optionExists = selectedValue.id === targetData.options[i].id;
                    if (optionExists) {
                        targetData.value = targetData.options[i];
                        break;
                    }
                }
            }
        }
    }

    export class SystemRelationMapper {

        static mapSystemRelationToViewModel(systemRelation: IItSystemUsageRelationDTO, maxTextFieldCharCount, shortTextLineCount) {
            return new SystemRelationViewModel(maxTextFieldCharCount, shortTextLineCount, systemRelation);
        }

        static mapSystemRelationsToViewModels(systemRelations: [IItSystemUsageRelationDTO], maxTextFieldCharCount, shortTextLineCount) {
            return _.map(systemRelations, relation => SystemRelationMapper.mapSystemRelationToViewModel(relation, maxTextFieldCharCount, shortTextLineCount));
        }
    }
}
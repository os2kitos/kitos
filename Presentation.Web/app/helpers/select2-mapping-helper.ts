module Kitos.Helpers {
    export class Select2MappingHelper {
        static mapNamedEntityWithDescriptionAndExpirationStatusDtoArrayToOptionMap(dtos: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[],
        allowExpiredObjects: boolean = true): Models.ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>[] {

            if (!dtos) {
                throw "Select2MappingHelper.mapNamedEntityWithDescriptionAndExpirationStatusDtoArrayToOptionMap: dtos cannot be undefined/null ";
            }
            return dtos.reduce((acc, next, _) => {
                acc[next.id] = {
                    id: next.id,
                    text: next.name,
                    optionalObjectContext: {
                        id: next.id,
                        name: next.name,
                        expired: allowExpiredObjects ? next.expired : false,
                        description: next.description
                    }
                };
                return acc;
            }, {}) as Models.ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>[];
        }
        static mapNamedEntityToSelect2ViewModel(existingChoice: Models.Generic.NamedEntity.NamedEntityDTO, map: Models.ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityDTO>[], disabledValue: boolean = null): Models.Generic.NamedEntity.NamedEntityDTO {
            if (existingChoice && !map[existingChoice.id]) {
                map[existingChoice.id] = {
                    text: `${existingChoice.name} (udgået)`,
                    id: existingChoice.id,
                    disabled: disabledValue,
                    optionalObjectContext: existingChoice
                }
            }

            return existingChoice;
        }

        static mapDataProcessingSearchResults(dataProcessors: Models.DataProcessing.IDataProcessorDTO[]) {
            return dataProcessors.map(
                dataProcessor => <Models.ViewModel.Generic.
                Select2OptionViewModel<Models.DataProcessing.IDataProcessorDTO>>{
                    id: dataProcessor.id,
                    text: dataProcessor.name,
                    optionalObjectContext: dataProcessor,
                    cvrNumber: dataProcessor.cvrNumber
                }
            );
        }

        static createNewNamedEntityWithDescriptionAndExpirationStatusDtoViewModel(element: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO,
            baseOptions: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[],
            updateMethod: (newElement: Models.ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>) => void,
            select2LoadingService: Services.ISelect2LoadingService,
            hasDisabledValue: boolean = null,
            allowExpiredObjects: boolean = true,
            enableSearch : boolean = false):
            Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO> {

            if (!baseOptions) {
                throw "Select2MappingHelper.createNewNamedEntityWithDescriptionAndExpirationStatusDtoViewModel: baseOptions cannot be undefined/null ";
            }

            const optionMap = Select2MappingHelper.mapNamedEntityWithDescriptionAndExpirationStatusDtoArrayToOptionMap(baseOptions ?? [], allowExpiredObjects);

            let existingChoice = null;
            if (element) {
                //If selected state is expired, add it for presentation reasons
                existingChoice = Select2MappingHelper.mapNamedEntityToSelect2ViewModel(element, optionMap, hasDisabledValue);
            }
            
            const options = baseOptions.map(option => optionMap[option.id]);
            const select2Config = enableSearch ? select2LoadingService.select2LocalData(() => options, true) : select2LoadingService.select2LocalDataNoSearch(() => options, true);
            return {
                selectedElement: existingChoice && optionMap[existingChoice.id],
                select2Config: select2Config,
                elementSelected: (newElement) => {
                    updateMethod(newElement);
                }
            };
        }
    }
}
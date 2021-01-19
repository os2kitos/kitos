module Kitos.DataProcessing.Registration.Edit.Ref {
    "use strict";

    export class EditDataProcessingContractController {
        static $inject: Array<string> = [
            "hasWriteAccess",
            "contract",
            "ItContractsService",
            "bindingService",
            "apiUseCaseFactory"
        ];

        private readonly contractId: number;
        constructor(
            public hasWriteAccess,
            private readonly contract: Kitos.Models.ItContract.IItContractDTO,
            private readonly contractService: Kitos.Services.Contract.IItContractsService,
            private readonly bindingService: Kitos.Services.Generic.IBindingService,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory
        ) {
            this.contractId = this.contract.id;

            this.bindDataProcessingRegistrations();
        }

        dataProcessingRegistrations: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.Generic.NamedEntity.NamedEntityDTO>;

        private bindDataProcessingRegistrations() {
            this.bindingService.bindMultiSelectConfiguration<Models.Generic.NamedEntity.NamedEntityDTO>(
                config => this.dataProcessingRegistrations = config,
                () => this.contract.dataProcessingRegistrations,
                element => this.removeDataProcessingRegistration(element.id),
                newElement => this.addDataProcessingRegistration(newElement),
                this.hasWriteAccess,
                this.hasWriteAccess,
                (query) => this
                    .contractService
                    .getAvailableDataProcessingRegistrations(this.contractId, query)
                    .then(results => this.mapDataProcessingSearchResults(results))
            );
        }

        private mapDataProcessingSearchResults(dataProcessingRegistrations: Models.Generic.NamedEntity.NamedEntityDTO[]) {
            return dataProcessingRegistrations.map(
                dataProcessingRegistration => <Models.ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityDTO>>{
                    id: dataProcessingRegistration.id,
                    text: dataProcessingRegistration.name,
                    optionalObjectContext: dataProcessingRegistration
                }
            );
        }

        private addDataProcessingRegistration(newElement: Models.ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityDTO>) {
            if (!!newElement && !!newElement.optionalObjectContext) {
                const newDataProcessingAgreement = newElement.optionalObjectContext;
                this.apiUseCaseFactory
                    .createAssignmentCreation(() => this.contractService.assignDataProcessingRegistration(this.contractId, newDataProcessingAgreement.id))
                    .executeAsync(success => {
                        //Update the source collection
                        this.contract.dataProcessingRegistrations.push(newDataProcessingAgreement);

                        //Trigger UI update
                        this.bindDataProcessingRegistrations();
                        return success;
                    });
            }
        }

        private removeDataProcessingRegistration(id: number) {
            this.apiUseCaseFactory
                .createAssignmentRemoval(() => this.contractService.removeDataProcessingRegistration(this.contractId, id))
                .executeAsync(success => {

                    //Update the source collection
                    this.contract.dataProcessingRegistrations = this.contract.dataProcessingRegistrations.filter(x => x.id !== id);

                    //Propagate changes to UI binding
                    this.bindDataProcessingRegistrations();
                    return success;
                });
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("it-contract.edit.data-processing", {
                url: "/data-processing",
                templateUrl: "app/components/it-contract/tabs/it-contract-tab-data-processing.view.html",
                controller: EditDataProcessingContractController,
                controllerAs: "vm",
                resolve: {
                },
            });
        }]);
}


module Kitos.Models.ViewModel.GDPR {

    export class SubDataProcessorViewModel {

        private _subDataProcessorId: number | null = null;
        private _cvrNumber: string | null = null;
        private _basisForTransferId: number | null = null;
        private _transferToInsecureThirdCountryId: number | null = null;
        private _insecureCountryId: number | null = null;

        constructor(subDataProcessorId: number | null,
            subDataProcessorCvr: string | null,
            basisForTransferId: number | null,
            transferToInsecureThirdCountryId: number | null,
            insecureCountryId: number | null) {

            this._subDataProcessorId = subDataProcessorId;
            this._cvrNumber = subDataProcessorCvr;
            this._basisForTransferId = basisForTransferId;
            this._transferToInsecureThirdCountryId = transferToInsecureThirdCountryId;
            this._insecureCountryId = insecureCountryId;
        }

        get subDataProcessorId(): number {
            return this._subDataProcessorId;
        }

        get cvrNumber(): string {
            return this._cvrNumber;
        }

        get basisForTransferId(): number {
            return this._basisForTransferId;
        }

        get transferToInsecureThirdCountryId(): number {
            return this._transferToInsecureThirdCountryId;
        }

        get insecureCountryId(): number {
            return this._insecureCountryId;
        }

        prepareRequestPayload() {
            return {
                organizationId: this.subDataProcessorId,
                details: {
                    basisForTransferOptionId: this.basisForTransferId,
                    transferToInsecureThirdCountries: this.transferToInsecureThirdCountryId,
                    insecureCountryOptionId: this.transferToInsecureThirdCountryId === Api.Shared.YesNoUndecidedOption.Yes ? this.insecureCountryId : null
                }
            } as Models.DataProcessing.ISubDataProcessorRequestDTO;
        }

        updateSubDataProcessor(newElement: ViewModel.Generic.Select2OptionViewModel<Models.DataProcessing.IDataProcessorDTO>): void {
            this._subDataProcessorId = newElement?.id;
            this._cvrNumber = newElement?.optionalObjectContext.cvrNumber;
        }

        updateBasisForTransfer(newElement: ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityDTO>): void {
            this._basisForTransferId = newElement?.id;
        }

        updateTransferToInsecureThirdCountry(newElement: ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>): void {
            this._transferToInsecureThirdCountryId = newElement?.id;
            if (this.transferToInsecureThirdCountryId !== Models.Api.Shared.YesNoUndecidedOption.Yes) {
                this._insecureCountryId = null;
            }
        }

        updateInsecureThirdCountry(newElement: ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityDTO>): void {
            if (this.transferToInsecureThirdCountryId !== Models.Api.Shared.YesNoUndecidedOption.Yes)
                return;

            this._insecureCountryId = newElement?.id;
        }
    }
}

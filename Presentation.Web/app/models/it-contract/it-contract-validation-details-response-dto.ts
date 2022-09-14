module Kitos.Models.ItContract {

    /** Contains info about an it contract */
    export interface IItContractValidationDetailsResponseDTO extends IEntity {
        valid: boolean
        enforcedValid: boolean
        errors: Array<ItContractValidationError>
    }
}

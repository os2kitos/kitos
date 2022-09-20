module Kitos.Models.ItSystemUsage {

    /** Contains info about it system usage validity.
     * @param valid - is true if system usage is valid
     * @param errors - an Array of validation errors
     */
    export interface IItSystemUsageValidationDetailsResponseDTO extends IEntity {
        valid: boolean;
        errors: Array<ItSystemUsageValidationError>;
    }
}
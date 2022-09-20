module Kitos.Models.ItSystemUsage {

    /** Contains info about it system usage validity.*/
    export interface IItSystemUsageValidationDetailsResponseDTO extends IEntity {
        /** Is true if system usage is valid */
        valid: boolean;
        /** An Array of validation errors*/
        errors: Array<ItSystemUsageValidationError>;
    }
}
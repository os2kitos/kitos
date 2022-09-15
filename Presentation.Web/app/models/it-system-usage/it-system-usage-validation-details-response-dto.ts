module Kitos.Models.ItSystemUsage {

    /** Contains info about an it system usage */
    export interface IItSystemUsageValidationDetailsResponseDTO extends IEntity {
        valid: boolean;
        enforcedValid: boolean;
        errors: Array<ItSystemUsageValidationError>;
    }
}
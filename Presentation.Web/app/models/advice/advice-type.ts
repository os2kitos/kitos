module Kitos.Models.Advice {
    export enum AdviceType {
        ItSystemUsage = "itSystemUsage",
        ItContract = "itContract",
        DataProcessingRegistration = "dataProcessingRegistration"
    }


    const adviceTypeToRoleIdPropertyUsingOdataNaming: Record<AdviceType, string> = {
        dataProcessingRegistration: "DataProcessingRegistrationRoleId",
        itContract: "ItContractRoleId",
        itSystemUsage: "ItSystemRoleId"
    };

    const adviceTypeToRolePropertyUsingOdataNaming: Record<AdviceType, string> = {
        dataProcessingRegistration: "DataProcessingRegistrationRole",
        itContract: "ItContractRole",
        itSystemUsage: "ItSystemRole"
    };

    export function getAdviceTypeUserRelationRoleIdProperty(adviceType: AdviceType, regularApiProperty?: boolean) {
        const odataPropertyName: string = adviceTypeToRoleIdPropertyUsingOdataNaming[adviceType];
        if (regularApiProperty) {
            const head = odataPropertyName.slice(0, 1).toLowerCase();
            const tail = odataPropertyName.slice(1);
            return `${head}${tail}`;
        }
        return odataPropertyName;
    }

    export function getAdviceTypeUserRelationRoleProperty(adviceType: AdviceType) {
        return adviceTypeToRolePropertyUsingOdataNaming[adviceType];
    }
}

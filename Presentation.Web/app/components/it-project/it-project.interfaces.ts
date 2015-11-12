module Kitos.ItProject {
    export interface IDropdownOption {
        id: number;
        label?: string;
    }

    export interface ISelectSettings {
        dynamicTitle: boolean;
        buttonClasses: string;
    }

    export interface IAllowClearOption {
        allowClear: boolean;
    }

    export interface ISelectTranslation {
        checkAll: string;
        uncheckAll: string;
        buttonDefaultText: string;
    }

    export interface IPayload {
        isStatusGoalVisible: boolean;
        isStrategyVisible: boolean;
        isHierarchyVisible: boolean;
        isEconomyVisible: boolean;
        isStakeholderVisible: boolean;
        isRiskVisible: boolean;
        isCommunicationVisible: boolean;
        isHandoverVisible: boolean;
    }

    export interface IApiResponse<T> {
        response: T;
    }

    export interface IPaginationSettings {
        search: string;
        skip: number;
        take: number;
        orderBy?: string;
        descending?: boolean;
    }

    export interface IPhase {
        name: string;
        startDate?: any;
        endDate?: any;
    }

    export interface IPhaseData {
        id;
        updateUrl;
        prevPhase;
    }

    export interface IDatepickerOptions {
        format: string;
        parseFormats: Array<string>;
    }
}

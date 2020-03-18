module Kitos.Utility {

    export class ViewDataState {
        constructor(private readonly $) {

        }

        getStateOrNull(stateType: string) {
            const input = this.$("input[data-state-type='" + stateType + "']");
            if (input && input.length === 1) {
                const errorElement = input[0];
                return {
                    value: errorElement.getAttribute("data-state-value"),
                    element: errorElement
                }
            }
            return null;
        }
    }
}
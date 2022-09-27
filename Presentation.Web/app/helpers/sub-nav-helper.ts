module Kitos.Helpers.SubNav {
    export function showAnyButtons($rootScope, currentState) {
        if (currentState) {
            const filterConditions = ($rootScope.page?.subnav?.buttons ?? []).map(button => button.showWhen);
            const availableByCondition = filterConditions.filter(condition => {
                if (condition) {
                    const stateIncludes = currentState.includes(condition);
                    return stateIncludes;
                }
                return true; //Default true if no restriction
            });
            return availableByCondition.length > 0;
        }
        return false;
    }
}
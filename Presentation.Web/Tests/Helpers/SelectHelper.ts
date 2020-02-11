
class SelectHelper {

    public static openAndSelect(selectId: string, optionLabel: string) {
        console.log(`Opening select with id ${selectId} and selecting ${optionLabel}`);
        return element(by.id(selectId)).click()
            .then(() => element(by.id(selectId)).element(by.css(`[label='${optionLabel}']`)).click());
    }
}

export = SelectHelper;
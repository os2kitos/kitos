
class SelectHelper {

    public static openAndSelect(selectId: string, optionLabel: string) {
        console.log("Opening select and selecting");
        element(by.id(selectId)).click()
            .then(() => element(by.id(selectId)).element(by.css(`[label='${optionLabel}']`)).click());
    }
}

export = SelectHelper;
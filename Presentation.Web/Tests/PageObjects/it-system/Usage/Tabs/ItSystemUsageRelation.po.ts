var ec = protractor.ExpectedConditions;

class ItSystemUsageRelation {

    public static getCreateButton() {
        return element(by.id("create-Relation"));
    }

    public static getReferenceInputField() {
        return element(by.id("Reference"));
    }

    public static getDescriptionInputField() {
        return element(by.id("description"));
    }

    public static getSaveButton() {
        return element(by.buttonText("Gem"));
    }

}

export = ItSystemUsageRelation;
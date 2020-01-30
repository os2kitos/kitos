class CSSLocatorHelper {

    public byDataElementType(value: string) {
        return by.css("[data-element-type='" + value + "']");
    }

}

export = CSSLocatorHelper;
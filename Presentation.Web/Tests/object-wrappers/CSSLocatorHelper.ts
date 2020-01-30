class CSSLocatorHelper {

    public byDataElementType(value: string) {
        return by.css(`[data-element-type='${value}']`);
    }

    public byDataField(value: string) {
        return by.css(`span[data-field='${value}']`);
    }

}

export = CSSLocatorHelper;
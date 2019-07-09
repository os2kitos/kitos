class CSSLocatorHelper {

    public byDataHook(value: string) {
        return by.css("[data-hook='" + value + "']");
    }

}

export = CSSLocatorHelper;
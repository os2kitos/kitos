class kendoToolbarWrapper {

    public mainGridElement = element(by.id("mainGrid"));

    public resetFiltersButton = this.mainGridElement.element(by.buttonText("Nulstil"));

    public saveFiltersButton = this.mainGridElement.element(by.buttonText("Gem filter"));

    public useFiltersButton = this.mainGridElement.element(by.buttonText("Anvend filter"));

    public deleteFiltersButton = this.mainGridElement.element(by.buttonText("Slet filter"));

    public roleSelector = this.mainGridElement.element(by.css("span.k-widget.k-dropdown.k-header"));

    public exportButton = this.mainGridElement.element(by.css("a.k-button.k-button-icontext.pull-right.k-grid-excel"));


    

}

export = kendoToolbarWrapper;
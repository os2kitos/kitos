class ContractDprPage {

    getDprRow(dprName: string) {
        return element(by.xpath(`//*/a[text()="${dprName}"]/../..`));
    }

    getRemoveDprButton(dprName: string) {
        return element(by.xpath(`//*/a[text()="${dprName}"]/../..//button`));
    }

}

export = ContractDprPage;
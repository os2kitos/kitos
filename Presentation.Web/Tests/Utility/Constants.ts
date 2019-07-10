// Since app uses namespaces (which is fine for smaller .ts apps) we are unable to import this class which is made using modules
// Using modules is preferable in larger applications as this provides modularity and maintainability as there is no
// global namespace pollution.

// So currently this class only works for the test part, but when possible the app part should be refactored to use modules rather than the Kitos namespace

class constants {
    readonly kendoSystemNameHeader = "systemNameHeader";
    readonly kendoSystemNameObjects = "systemName";
    readonly kendoContractNameHeader = "contractNameHeader";
    readonly kendoContractNameObjects = "contractName";
}

export = constants;


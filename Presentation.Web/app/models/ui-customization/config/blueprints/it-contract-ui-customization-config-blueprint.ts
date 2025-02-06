module Kitos.Models.UICustomization.Configs.BluePrints {
    export const ItContractUiCustomizationBluePrint = {
        module: UICustomization.CustomizableKitosModule.ItContract,
        readOnly: false,
        helpText: Configs.helpTexts.generalUiCustomizationHelpText,
        text: "IT Kontrakt",
        children: {
            frontPage: {
                text: "Kontraktforside",
                readOnly: true,
                helpText: Configs.helpTexts.cannotChangeTab,
                children: {
                    contractId: {
                        text: "KontraktID"
                    },
                    contractType: {
                        text: "Kontrakttype"
                    },
                    template: {
                        text: "Kontraktskabelon"
                    },
                    criticality: {
                        text: "Kritikalitet"
                    },
                    purchaseForm: {
                        text: "Indkøbsform"
                    },
                    procurementStrategy: {
                        text: "Genanskaffelsesstrategi"
                    },
                    procurementPlan: {
                        text: "Genanskaffelsesplan"
                    },
                    procurementInitiated: {
                        text: "Genanskaffelse igangsat"
                    },
                    externalSigner: {
                        text: "Leverandørs underskrift",
                        helpText: "Herunder: 'Underskriver', 'Underskrevet' og 'Dato'"
                    },
                    internalSigner: {
                        text: "Kontraktunderskriver",
                        helpText: "Herunder: 'Underskriver', 'Underskrevet' og 'Dato'"
                    },
                    agreementPeriod: {
                        text: "Gyldig fra/til"
                    },
                    isActive: {
                        text: "Gyldig"
                    }
                }
            },
            itSystems: {
                text: "IT Systemer",
                helpText: Configs.helpTexts.cannotChangeTabOnlyThroughModuleConfig,
                readOnly: true
            },
            dataProcessing: {
                text: "Databehandling",
                helpText: Configs.helpTexts.cannotChangeTabOnlyThroughModuleConfig,
                readOnly: true
            },
            deadlines: {
                text: "Aftalefrister",
                readOnly: false,
                subtreeIsComplete: true,
                children: {
                    agreementDeadlines: {
                        text: "Aftalefrister",
                        helpText: "Herunder: 'Varighed', 'Løbende', 'Option' forlæng', 'Antal brugte optioner' og 'Uopsigelig til' "
                    },
                    termination: {
                        text: "Opsigelse",
                        helpText: "Herunder: 'Kontrakten opsagt', 'Opsigelsesfrist', 'Løbende' og 'Inden udgangen af' "
                    }
                }
            },
            economy: {
                text: "Økonomi",
                readOnly: false,
                subtreeIsComplete: true,
                children: {
                    paymentModel: {
                        text: "Betalingsmodel",
                        helpText: "Herunder: 'Driftsvederlag påbegyndt', 'Betalingsfrekvens', 'Betalingsmodel' og 'Prisregulering' "
                    },
                    extPayment: {
                        text: "Ekstern betaling"
                    },
                    intPayment: {
                        text: "Intern betaling"
                    }
                }
            },
            contractRoles: {
                text: "Kontraktroller",
            },
            hierarchy: {
                text: "Hierarki",
                helpText: Configs.helpTexts.cannotChangeTab,
                readOnly: true
            },
            advice: {
                text: "Advis"
            },
            references: {
                text: "Referencer",
                helpText: Configs.helpTexts.cannotChangeTab,
                readOnly: true
            }
        }
    }

    // Mandatory post-processing to build the keys
    processConfigurationTree(ItContractUiCustomizationBluePrint.module, ItContractUiCustomizationBluePrint, []);
}
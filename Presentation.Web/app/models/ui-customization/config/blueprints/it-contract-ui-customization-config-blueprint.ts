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
                    procurementStrategy: {
                        text: "Genanskaffelsesstrategi"
                    },
                    procurementPlan: {
                        text: "Genanskaffelsesplan"
                    },
                    contractId: {
                        text: "KontraktID"
                    },
                    contractType: {
                        text: "Kontrakttype"
                    },
                    purchaseForm: {
                        text: "Indkøbsform"
                    },
                    extSigner: {
                        text: "Leverandørs kontraktunderskriver"
                    },
                    extSigned: {
                        text: "Leverandørs underskrevet"
                    },
                    extDate: {
                        text: "Leverandørs dato"
                    },
                    intSigner: {
                        text: "Kontraktunderskriver"
                    },
                    intSigned: {
                        text: "Underskrevet"
                    },
                    intDate: {
                        text: "Dato"
                    },
                    agreementConcluded: {
                        text: "Gyldig fra"
                    },
                    agreementExpiration: {
                        text: "Gyldig til"
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
                helpText: Configs.helpTexts.cannotChangeTab,
                readOnly: true,
                children: {
                    agreementDeadlines: {
                        text: "Aftalefrister"
                    },
                    termination: {
                        text: "Opsigelse"
                    }
                }
            },
            economy: {
                text: "Økonomi",
                helpText: Configs.helpTexts.cannotChangeTab,
                readOnly: true,
                children: {
                    paymentModel: {
                        text: "Betalingsmodel"
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
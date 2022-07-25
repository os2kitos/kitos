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
                        text: "Aftalefrister",
                        children:
                        {
                            duration: {
                                text: "Varighed"
                            },
                            ongoing: {
                                text: "Løbende"
                            },
                            optionExtend: {
                                text: "Option forlæng"
                            },
                            optionExtendMultiplier: {
                                text: "Antal brugte optioner"
                            },
                            irrevocable: {
                                text: "Uopsigelig til"
                            }
                        }
                    },
                    termination: {
                        text: "Opsigelse",
                        children: {
                            terminated: {
                                text: "Kontrakten opsagt"
                            },
                            notice: {
                                text: "Opsigelsesfrist"
                            },
                            ongoing: {
                                text: "Løbende"
                            },
                            byEndingOf: {
                                text: "Inden udgangen af"
                            }
                        }
                    }
                }
            },
            economy: {
                text: "Økonomi",
                helpText: Configs.helpTexts.cannotChangeTab,
                readOnly: true,
                children: {
                    paymentModel: {
                        text: "Betalingsmodel",
                        children: {
                            operation: {
                                text: "Driftsvederlag påbegyndt"
                            },
                            frequency: {
                                text: "Betalingsfrekvens"
                            },
                            paymentModel: {
                                text: "Betalingsmodel"
                            },
                            price: {
                                text: "Prisregulering"
                            }
                        }
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
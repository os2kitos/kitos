using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;

namespace Infrastructure.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<KitosContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;

            // New timeout in seconds
            this.CommandTimeout = 60 * 5;
        }

        /// <summary>
        /// Seeds the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void Seed(KitosContext context)
        {

            //set true if there is no existing database that needs to be updated
            var newBuild = Environment.GetEnvironmentVariable("PerformInitialSeed") == "yes";

            #region USERS

            // don't overwrite global admin if it already exists
            // cause it'll overwrite UUID
            var globalAdmin = context.Users.FirstOrDefault(x => x.Email == "support@kitos.dk") ?? context.Users.Add(
                new User
                {
                    Name = "Global",
                    LastName = "admin",
                    Email = "support@kitos.dk",
                    Salt = "uH3U0wqme2mc83FvSwkDrd9fm-3MycFR0ugaKtREJBw1",
                    Password = "h3hNNY9J3SBUNCgaoccGo1WCRDxt4v9oUjD5uZhQ78M1",
                    IsGlobalAdmin = true
                });

            //var cryptoService = new CryptoService();
            //var user1 = CreateUser("Test bruger1", "1@test", "test", cryptoService);
            //var user2 = CreateUser("Test bruger2", "2@test", "test", cryptoService);
            //var user3 = CreateUser("Test bruger3", "3@test", "test", cryptoService);
            //var user4 = CreateUser("Test bruger4", "4@test", "test", cryptoService);
            //var user5 = CreateUser("Test bruger5", "5@test", "test", cryptoService);
            //context.Users.AddOrUpdate(x => x.Email, , user1, user2, user3, user4, user5);

            context.SaveChanges();

            #endregion
            
            if (newBuild == true)
            {
                #region OPTIONS


                AddOptions<ItProjectType, ItProject>(context.ItProjectTypes, globalAdmin, "Fællesoffentlig", "Fælleskommunal", "Lokal", "Tværkommunal", "SKAL", "Udvikling", "Implementering");

                AddOptions<GoalType, Goal>(context.GoalTypes, globalAdmin, "Effektivitet", "Kvalitet", "Service");

                AddOptions<ItSystemType, ItSystem>(context.ItSystemTypes, globalAdmin, "Fagsystem", "Selvbetjening", "Applikation", "Brugerinterface", "Programmeringsinterface", "Applikationsservice", "Applikationskomponent", "Applikationsfunktion", "Applikationsmodul");

                AddOptions<BusinessType, ItSystem>(context.BusinessTypes, globalAdmin, "Desing, visualisering og grafik", "Kommunikation", "Hjemmesider og portaler", "Selvbetjening og indberetning", "E-læring", "ESDH og Journalisering", "Specialsystemer", "Processtyring", "IT management", "Økonomi og betaling", "Løn, personale og HR", "BI og ledelsesinformation", "Master data og registre", "GIS", "Bruger- og rettighedsstyring", "Sikkerhed og overvågning", "Sagsbærende", "Administrativt");

                AddOptions<ArchiveType, ItSystemUsage>(context.ArchiveTypes, globalAdmin, "Arkiveret", "Ikke arkiveret", "Arkiveringspligt", "Ikke arkiveringspligt", "Øjebliksbillede", "Periode", "Løbende");

                AddOptions<ArchiveLocation, ItSystemUsage>(context.ArchiveLocation, globalAdmin, "Aalborg", "Aabenraa", "Vejle", "Vejen", "Tårnby", "Tønder", "Thisted", "Sønderborg", "Syddjurs", "Struer", "Slagelse", "Skive", "Silkeborg", "Rudersdal", "Roskilde", "Randers", "Odense", "Næstved", "Norddjurs", "Mariagerfjord", "Læsø", "Lyngby-Taarbæk", "Lolland", "København", "Kolding", "Ishøj", "Hørsholm", "Horsens", "Holbæk", "Hjørring", "Helsingør", "Hedensted", "Haderslev", "Guldborgsund", "Gribskov", "Greve", "Gladsaxe", "Gentofte", "Furesø", "Frederikssund", "Frederikshavn", "Fredensborg", "Faxe", "Esbjerg", "Egedal", "Dragør", "Brøndby", "Bornholm", "Billund");

                AddOptions<DataType, DataRow>(context.DataTypes, globalAdmin, "Person", "Virksomhed", "Sag", "Dokument", "Organisation", "Klassikfikation", "Ejendom", "GIS", "Andet");

                AddOptions<FrequencyType, DataRowUsage>(context.FrequencyTypes, globalAdmin, "Dagligt", "Ugentligt", "Månedligt", "Årligt", "Kvartal", "Halvårligt");

                AddOptions<ItInterfaceType, ItInterface>(context.ItInterfaceTypes, globalAdmin, "Webservice", "API", "iFrame", "Link", "Link - dybt", "Andet");

                AddOptions<InterfaceType, ItInterface>(context.InterfaceTypes, globalAdmin, "CSV", "WS SOAP", "WS REST", "MOX", "OIO REST", "LDAP", "User interface", "ODBC (SQL)", "Andet");

                AddOptions<MethodType, ItInterface>(context.MethodTypes, globalAdmin, "Batch", "Request-Response", "Store and forward", "Publish-subscribe", "App interface", "Andet");

                AddOptions<SensitiveDataType, ItSystemUsage>(context.SensitiveDataTypes, globalAdmin, "Ja", "Nej");

                AddOptions<TsaType, ItInterface>(context.TsaTypes, globalAdmin, "Ja", "Nej");

                AddOptions<ItContractType, ItContract>(context.ItContractTypes, globalAdmin, "Hovedkontrakt", "Tillægskontrakt", "Snitflade", "Serviceaftale", "Databehandleraftale");

                AddOptions<ItContractTemplateType, ItContract>(context.ItContractTemplateTypes, globalAdmin, "K01", "K02", "K03", "Egen", "KOMBIT", "Leverandør", "OPI", "Anden");

                AddOptions<PurchaseFormType, ItContract>(context.PurchaseFormTypes, globalAdmin, "SKI", "SKI 02.18", "SKI 02.19", "Udbud", "EU udbud", "Direkte tildeling", "Annoncering");

                AddOptions<PaymentModelType, ItContract>(context.PaymentModelTypes, globalAdmin, "Licens", "icens - flatrate", "Licens - forbrug", "Licens - indbyggere", "Licens - pr. sag", "Gebyr", "Engangsydelse");

                AddOptions<AgreementElementType, ItContract>(context.AgreementElementTypes, globalAdmin,
                    "Licens", "Udvikling", "Drift", "Vedligehold", "Support",
                    "Serverlicenser", "Serverdrift", "Databaselicenser", "Backup", "Overvågning");

                AddOptions<OptionExtendType, ItContract>(context.OptionExtendTypes, globalAdmin, "2 x 1 år", "1 x 1 år", "1 x ½ år");

                AddOptions<PaymentFreqencyType, ItContract>(context.PaymentFreqencyTypes, globalAdmin, "Månedligt", "Kvartalsvis", "Halvårligt", "Årligt");

                AddOptions<PriceRegulationType, ItContract>(context.PriceRegulationTypes, globalAdmin, "TSA", "KL pris og lønskøn", "Nettoprisindeks");

                AddOptions<ProcurementStrategyType, ItContract>(context.ProcurementStrategyTypes, globalAdmin, "Direkte tildeling", "Annoncering", "Udbud", "EU udbud", "Mini-udbud", "SKI - direkte tildeling", "SKI - mini-udbud", "Underhåndsbud");

                AddOptions<TerminationDeadlineType, ItContract>(context.TerminationDeadlineTypes, globalAdmin, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12");

                AddOptions<HandoverTrialType, HandoverTrial>(context.HandoverTrialTypes, globalAdmin, "Funktionsprøve", "Driftovertagelsesprøve");

                AddOptions<ReportCategoryType, Report>(context.ReportCategoryTypes, globalAdmin, "IT Kontrakt", "IT Projekt", "IT System", "Organisation", "Andet", "Tværgående");

                AddOptions<ItSystemCategories, ItSystemUsage>(context.ItSystemCategories, globalAdmin, "Offentlige data", "Interne data", "Fortrolige data", "Hemmelige data");

                context.SaveChanges();

                #endregion

                #region ORG ROLES

                var boss = new OrganizationUnitRole()
                {
                    IsLocallyAvailable = true,
                    Name = "Chef",
                    Description = "Lederen af en organisationsenhed",
                    HasWriteAccess = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 7
                };

                var resourcePerson = new OrganizationUnitRole()
                {
                    IsLocallyAvailable = true,
                    Name = "Ressourceperson",
                    Description = "...",
                    HasWriteAccess = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 6
                };

                var employee = new OrganizationUnitRole()
                {
                    IsLocallyAvailable = true,
                    Name = "Medarbejder",
                    Description = "...",
                    HasWriteAccess = false,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 5
                };

                var digitalConsultant = new OrganizationUnitRole()
                {
                    IsLocallyAvailable = true,
                    Name = "Digitaliseringskonsulent",
                    Description = "...",
                    HasWriteAccess = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 4
                };

                var itConsultant = new OrganizationUnitRole()
                {
                    IsLocallyAvailable = true,
                    Name = "IT konsulent",
                    Description = "...",
                    HasWriteAccess = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 3
                };

                var leader = new OrganizationUnitRole()
                {
                    IsLocallyAvailable = true,
                    Name = "Leder",
                    Description = "...",
                    HasWriteAccess = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 2
                };

                var director = new OrganizationUnitRole()
                {
                    IsLocallyAvailable = true,
                    Name = "Direktør",
                    Description = "...",
                    HasWriteAccess = false,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 1
                };

                try
                {
                    var count = context.OrganizationUnitRoles.Count();
                    foreach (var organizationUnitRole in context.OrganizationUnitRoles)
                    {
                        organizationUnitRole.Priority = count;
                        count--;
                    }
                    context.OrganizationUnitRoles.AddOrUpdate(role => role.Name, boss, resourcePerson, employee, digitalConsultant, itConsultant, leader, director);
                    context.SaveChanges();
                }
                catch
                {
                    // we don't really care about duplicates
                    // just do nothing
                }

                #endregion

                #region PROJECT ROLES

                context.ItProjectRoles.AddOrUpdate(r => r.Name,
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Projektejer",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 20
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Projektleder",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 19
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Delprojektleder",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 18
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Projektdeltager",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 17
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Teknisk projektleder",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 16
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "IT konsulent",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 15
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Implementeringskonsulent",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 14
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Proceskonsulent",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 13
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Juridisk konsulent",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 12
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "IT arkitekt",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 11
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Testansvarlig",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 10
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Support",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 9
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Programleder",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 8
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Styregruppeformand",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 7
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Styregruppemedlem",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 6
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Forretningsejer",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 5
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Forretningsansvarlig",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 4
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Gevinstejer",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 3
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Gevinsansvarlig",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 2
                    },
                    new ItProjectRole()
                    {
                        HasWriteAccess = true,
                        IsLocallyAvailable = true,
                        Name = "Rådgiver",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id,
                        Priority = 1
                    });

                var itProjectRolesCount = context.ItProjectRoles.Count();
                foreach (var role in context.ItProjectRoles)
                {
                    role.Priority = itProjectRolesCount;
                    itProjectRolesCount--;
                }

                context.SaveChanges();

                #endregion

                #region SYSTEM ROLES

                var systemOwnerRole = new ItSystemRole()
                {
                    HasReadAccess = true,
                    HasWriteAccess = true,
                    IsLocallyAvailable = true,
                    Name = "Systemejer",
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 9
                };

                var systemResponsibleRole = new ItSystemRole()
                {
                    HasReadAccess = true,
                    HasWriteAccess = true,
                    IsLocallyAvailable = true,
                    Name = "Systemansvarlig",
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 8
                };

                var businessOwnerRole = new ItSystemRole()
                {
                    HasReadAccess = true,
                    HasWriteAccess = true,
                    IsLocallyAvailable = true,
                    Name = "Forretningsejer",
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 7
                };

                var superuserResponsibleRole = new ItSystemRole()
                {
                    HasReadAccess = true,
                    HasWriteAccess = true,
                    IsLocallyAvailable = true,
                    Name = "Superbrugeransvarlig",
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 6
                };

                var superuserRole = new ItSystemRole()
                {
                    HasReadAccess = true,
                    HasWriteAccess = true,
                    IsLocallyAvailable = true,
                    Name = "Superbruger",
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 5
                };

                var securityResponsibleRole = new ItSystemRole()
                {
                    HasReadAccess = true,
                    HasWriteAccess = true,
                    IsLocallyAvailable = true,
                    Name = "Sikkerhedsansvarlig",
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 4
                };

                var chanceManagerRole = new ItSystemRole()
                {
                    HasReadAccess = true,
                    HasWriteAccess = true,
                    IsLocallyAvailable = true,
                    Name = "Changemanager",
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 3
                };

                var dataOwnerRole = new ItSystemRole()
                {
                    HasReadAccess = true,
                    HasWriteAccess = true,
                    IsLocallyAvailable = true,
                    Name = "Dataejer",
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 2
                };

                var systemAdminRole = new ItSystemRole()
                {
                    HasReadAccess = true,
                    HasWriteAccess = true,
                    IsLocallyAvailable = true,
                    Name = "Systemadminstrator",
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 1
                };

                context.ItSystemRoles.AddOrUpdate(x => x.Name, systemOwnerRole, systemResponsibleRole, businessOwnerRole, superuserResponsibleRole, superuserRole, securityResponsibleRole, chanceManagerRole, dataOwnerRole, systemAdminRole);

                var itSystemRolesCount = context.ItSystemRoles.Count();
                foreach (var role in context.ItSystemRoles)
                {
                    role.Priority = itSystemRolesCount;
                    itSystemRolesCount--;
                }

                context.SaveChanges();

                #endregion

                #region CONTRACT ROLES

                context.ItContractRoles.AddOrUpdate(x => x.Name, new ItContractRole()
                {
                    HasWriteAccess = true,
                    Name = "Kontraktejer",
                    IsObligatory = false,
                    IsLocallyAvailable = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 6
                }, new ItContractRole()
                {
                    HasWriteAccess = true,
                    Name = "Kontraktmanager",
                    IsObligatory = false,
                    IsLocallyAvailable = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 5
                }, new ItContractRole()
                {
                    HasWriteAccess = true,
                    Name = "Juridisk rådgiver",
                    IsObligatory = false,
                    IsLocallyAvailable = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 4
                }, new ItContractRole()
                {
                    HasWriteAccess = true,
                    Name = "Konsulent",
                    IsObligatory = false,
                    IsLocallyAvailable = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 3
                }, new ItContractRole()
                {
                    HasWriteAccess = true,
                    Name = "Fakturamodtager",
                    IsObligatory = false,
                    IsLocallyAvailable = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 2
                }, new ItContractRole()
                {
                    HasWriteAccess = true,
                    Name = "Budgetansvarlig",
                    IsObligatory = false,
                    IsLocallyAvailable = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 1
                });

                var itContractRolesCount = context.ItContractRoles.Count();
                foreach (var role in context.ItContractRoles)
                {
                    role.Priority = itContractRolesCount;
                    itContractRolesCount--;
                }

                context.SaveChanges();

                #endregion

                #region ORGANIZATIONS

                var muniType = new OrganizationType { Name = "Kommune", Category = OrganizationCategory.Municipality };
                var interestType = new OrganizationType { Name = "Interessefællesskab", Category = OrganizationCategory.Municipality };
                var company = new OrganizationType { Name = "Virksomhed", Category = OrganizationCategory.Other };
                var other = new OrganizationType { Name = "Anden offentlig myndighed", Category = OrganizationCategory.Other };
                context.OrganizationTypes.AddOrUpdate(x => x.Name, muniType, interestType, company, other);

                context.SaveChanges();

                var orgType = context.OrganizationTypes.Single(x => x.Name == "Kommune");

                var commonOrganization = CreateOrganization("Fælles Kommune", orgType, globalAdmin);
                //var muni1 = CreateOrganization("Test kommune1", OrganizationType.Municipality, globalAdmin);
                //var muni2 = CreateOrganization("Test kommune2", OrganizationType.Municipality, globalAdmin);

                context.Organizations.AddOrUpdate(x => x.Name, commonOrganization/*, muni1, muni2*/);
                context.SaveChanges();

                //commonOrganization = context.Organizations.Single(x => x.Name == "Fælles Kommune");

                //SetUserDefaultOrganizationUnit(globalAdmin, commonOrganization);
                //SetUserDefaultOrganizationUnit(user1, commonOrganization);
                //SetUserDefaultOrganizationUnit(user2, muni1);
                //SetUserDefaultOrganizationUnit(user3, muni2);


                #endregion

                #region TEXTS

                if (!context.Texts.Any(x => x.Id == 1))
                {
                    context.Texts.AddOrUpdate(new Text()
                    {
                        Value = @"<h3>Om KITOS?</h3> <p>KITOS - Kommunernes IT OverbliksSystem er et IT System, som er udviklet i de 3 første kvartaler i 2014 af Roskilde, Sorø, Ringsted, Syddjurs og Ballerup kommune.
Et væsentligt formål med projektet er at skabe et ensartet grundlag for hvordan vi som kommuner kan øge vores modenhed og evne til fremadrettet at 1) skabe overblik over 2) dokumentere og 3) analysere på vores samlede IT portefølje m.v. I forlængelse heraf er det en løsning, som skal hjælpe os med at understøtte det vidensbehov, som vi mener at monopolbruddet kræver – herunder kvalificere vores evne til at udnytte rammearkitekturen.
KITOS er bygget op omkring flg. moduler:
<ol>
    <li>IT understøttelse af organisation</li>
    <li>IT Projekter</li>
    <li>IT Systemer</li>
    <li>IT Kontrakter</li>
</ol>
 Løsningen er ’overdraget’ til det digitale fællesskab OS2, som vil sørge for forvaltning af KITOS med hensyn til hosting, vedligeholdelse, videreudvikling, administration etc, så den også i praksis vil være mulig for andre kommuner at bruge.
De første kommuner tager KITOS i brug i oktober 2014.
Du kan læse mere om OS2KITOS på os2web.dk > Projekter > KITOS
Kontakt: info@kitos.dk</p><p><a href='https://os2.eu/produkt/os2kitos'>Klik her for at læse mere</a></p>",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                if (!context.Texts.Any(x => x.Id == 2))
                {
                    context.Texts.AddOrUpdate(new Text() { Value = "<h3>Driftstatus</h3> Der er p.t ingen driftsforstyrrelser", ObjectOwnerId = globalAdmin.Id, LastChangedByUserId = globalAdmin.Id });
                }

                if (!context.Texts.Any(x => x.Id == 3))
                {
                    context.Texts.AddOrUpdate(new Text()
                    {
                        Value = @"<h3>Hjælp og vejledning til KITOS</h3> <ul class='list-unstyled'>
                        <li><a href='/docs/Vejledning til slutbrugeren.pdf' target='_blank'>Vejledning til login og brugerkonto</a></li>
                        <li><a href='/docs/Vejledning_Masseopret.pdf' target='_blank'>Vejledning til lokale administratorer</a></li>
                        <li><a href='/docs/Vejledning til roller og rettigheder.pdf' target='_blank'>Vejledning til roller og rettigheder</a></li>
                        <li><a href='/docs/Vejledning til tabeller på overblik.pdf' target='_blank'>Vejledning til tabeller på overblik</a></li>
                        <li><a href='/docs/vejledningerOrganisation.pdf' target='_blank'>Vejledning til modulet Organisation</a></li>
                        <li><a href='/docs/vejledningerITProjekter.pdf' target='_blank'>Vejledning til modulet IT Projekter</a></li>
                        <li><a href='/docs/vejledningerITSystemer.pdf' target='_blank'>Vejledning til modulet IT Systemer</a></li>
                        <li><a href='/docs/vejledningerITKontrakter.pdf' target='_blank'>Vejledning til modulet IT Kontrakter</a></li>
                        <li><a href='/docs/html/index.html' target='_blank'>Kitos kode dokumentation</a></li>
                        <li><a href='/Content/excel/Kontrakt_Indgåelse_Skabelon.xlsx' target='_blank'>Kontrakt-skabelon</a></li>
                    </ul>",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                if (!context.Texts.Any(x => x.Id == 4))
                {
                    context.Texts.AddOrUpdate(new Text() { Value = "<h3>Support</h3> <p>Hvis du oplever problemer med KITOS skal du kontakte din lokale adm.</p> <p><a href='https://kitostest.miracle.dk/docs/LokaladministratoreroversigtKitos.pdf'>Klik her for kontaktinfo på din lokale administrator</p>", ObjectOwnerId = globalAdmin.Id, LastChangedByUserId = globalAdmin.Id });
                }

                if (!context.Texts.Any(x => x.Id == 5))
                {
                    context.Texts.AddOrUpdate(new Text()
                    {
                        Value = @"<h3>Hvordan kommer I med på løsningen?</h3> <p>Gør følgende for at tilslutte jer KITOS:</p>
                    <ol>
                        <li>Læs tilslutningsaftalen <a href='http://os2.eu/dokument/kitos-tilslutningsaftale-publiceret'>her</a>.</li>
                        <li>
                            Udfyld og underskriv tilslutningsaftalen og send den til <a href='mailto:info@kitos.dk'>info@kitos.dk</a> for at tilmelde jer KITOS. Skriv gerne hvornår
                            I ønsker, at tilslutningen skal træde i kraft.
                        </li>
                        <li>Herefter kontakter vi jer, så vi kan få en snak om, hvad der skal ske fremover, og hvordan I bedst muligt tager KITOS i brug i jeres kommune.</li>
                    </ol>
                    <p>Ønsker I mere information om tilslutningsprocessen og KITOS generelt:</p>
                    <ol>
                        <li>Læs mere om KITOS <a href='http://os2.eu/produkt/os2kitos'>her</a>.</li>
                        <li>Skriv til <a href='mailto:info@kitos.dk'>info@kitos.dk</a> for at få besvaret eventuelle spørgsmål. Oplys et telefonnr., hvis I ønsker at blive ringet op.</li>
                    </ol>",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                #endregion

                #region Global Config

                if (!context.GlobalConfigs.Any(x => x.key == "CanGlobalAdminOnlyEditReports"))
                {
                    var globalConfig = new GlobalConfig { key = "CanGlobalAdminOnlyEditReports", value = "true" };
                    context.GlobalConfigs.AddOrUpdate(globalConfig);

                    context.SaveChanges();
                }

                #endregion

                //#region KLE

                //var orgUnit = context.Organizations.Single(x => x.Name == "Fælles Kommune").OrgUnits.First();
                //var kle = GenerateAllTasks(globalAdmin, orgUnit);
                //context.TaskRefs.AddRange(kle);

                //#endregion
            }
        }

        #region Helper methods

        private static List<TaskRef> GenerateTasks(User objectOwner, OrganizationUnit orgUnitOwner, string type, int n = 20,
                                                     TaskRef parent = null, string parentTaskKey = null)
        {
            var result = new List<TaskRef>();

            if (parentTaskKey != null) parentTaskKey = parentTaskKey + ".";

            for (var i = 0; i < n; i++)
            {
                var taskKey = parentTaskKey + i.ToString().PadLeft(2, '0');

                result.Add(new TaskRef()
                {
                    Type = type,
                    Description = "...",
                    TaskKey = taskKey,
                    OwnedByOrganizationUnit = orgUnitOwner,
                    ObjectOwner = objectOwner,
                    LastChangedByUser = objectOwner,
                    AccessModifier = AccessModifier.Public,
                    Parent = parent
                });
            }

            return result;
        }

        private static IEnumerable<TaskRef> GenerateAllTasks(User objectOwner, OrganizationUnit orgUnitOwner)
        {
            var maingroups = GenerateTasks(objectOwner, orgUnitOwner, "KLE-Hovedgruppe", 3);
            var subgroups =
                maingroups.SelectMany(
                    parent => GenerateTasks(objectOwner, orgUnitOwner, "KLE-Gruppe", 5, parent, parent.TaskKey)).ToList();

            var leafs = subgroups.SelectMany(parent => GenerateTasks(objectOwner, orgUnitOwner, "KLE-Emne", 10, parent, parent.TaskKey)).ToList();

            var result = new List<TaskRef>();
            result.AddRange(maingroups);
            result.AddRange(subgroups);
            result.AddRange(leafs);

            return result;
        }

        /// <summary>
        /// Creates and returns an Option entity.
        /// </summary>
        /// <typeparam name="T">Type of option to be added.</typeparam>
        /// <typeparam name="TReference">Reference type for the option</typeparam>
        /// <param name="name">The name of the new option entity</param>
        /// <param name="objectOwner">Object owner of the new entity</param>
        /// <param name="note">Description for the entity</param>
        /// <param name="isActive">Is the option active</param>
        /// <param name="isSuggestion">Is the option an suggestion</param>
        /// <returns></returns>
        private static T CreateOption<T, TReference>(string name, User objectOwner, string description = "...", bool isActive = true, bool isSuggestion = false)
            where T : OptionEntity<TReference>, new()
        {
            return new T()
            {
                IsLocallyAvailable = isActive,
                IsSuggestion = isSuggestion,
                Name = name,
                Description = description,
                ObjectOwnerId = objectOwner.Id,
                LastChangedByUserId = objectOwner.Id
            };
        }

        /// <summary>
        /// Helper method for quickly adding a lot of Option entities
        /// </summary>
        /// <typeparam name="T">Type of option to be added.</typeparam>
        /// <typeparam name="TReference">Reference type for the option</typeparam>
        /// <param name="dbSet">The db set to add too</param>
        /// <param name="objectOwner">Object owner of the new entities</param>
        /// <param name="names">The names of the new options</param>
        private static void AddOptions<T, TReference>(IDbSet<T> dbSet, User objectOwner, params string[] names) where T : OptionEntity<TReference>, new()
        {
            var options = names.Select(name => CreateOption<T, TReference>(name, objectOwner)).ToList();
            try
            {
                dbSet.AddOrUpdate(x => x.Name, options.ToArray());
            }
            catch
            {
                // we don't really care about duplicates
                // just do nothing
            }
            var i = dbSet.Count();
            foreach (var option in dbSet)
            {
                option.Priority = i;
                i--;
            }
        }

        /// <summary>
        /// Creates and returns a User
        /// </summary>
        /// <param name="name">User name</param>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <param name="cryptoService">The cryptoservice used to encrypt the password of the user</param>
        /// <param name="objectOwner"></param>
        /// <returns></returns>
        private static User CreateUser(string name, string email, string password, CryptoService cryptoService, User objectOwner = null)
        {
            var salt = cryptoService.Encrypt(name + "salt");
            return new User()
            {
                Name = name,
                Email = email,
                Salt = salt,
                Password = cryptoService.Encrypt(password + salt),
                ObjectOwnerId = objectOwner?.Id,
                LastChangedByUserId = objectOwner?.Id
            };
        }

        private static Organization CreateOrganization(string name, OrganizationType organizationType, User objectOwner = null)
        {
            var org = new Organization
            {
                Name = name,
                Config = Config.Default(objectOwner),
                TypeId = organizationType.Id,
                ObjectOwnerId = objectOwner?.Id,
                LastChangedByUserId = objectOwner?.Id
            };

            org.OrgUnits.Add(new OrganizationUnit
            {
                Name = org.Name,
                ObjectOwnerId = objectOwner?.Id,
                LastChangedByUserId = objectOwner?.Id
            });

            return org;
        }

        /// <summary>
        /// Helper function for setting the CreatedIn and DefaultOrganizationUnit properties.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="organization"></param>
        private static void SetUserDefaultOrganizationUnit(User user, Organization organization)
        {
            var rootOrgUnit = organization.GetRoot();
            var right = organization.Rights.FirstOrDefault(x => x.UserId == user.Id);
            Debug.Assert(right != null, "right != null");
            right.DefaultOrgUnitId = rootOrgUnit.Id;
        }

        #endregion
    }
}

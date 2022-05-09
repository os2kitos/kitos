using System;
using System.Data.Entity;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Infrastructure.Services.Cryptography;

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
            var cleanDatabase = GetEnvironmentVariable("SeedNewDb") == "yes";

            if (cleanDatabase)
            {
                Console.Out.WriteLine("Seeding initial data into kitos database");
                #region USERS

                // don't overwrite global admin if it already exists
                // cause it'll overwrite UUID
                var salt = $"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}";
                string password;
                using (var cryptoService = new CryptoService())
                {
                    password = cryptoService.Encrypt($"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}" + salt);
                }

                const string rootUserEmail = "support@kitos.dk";
                var globalAdmin = context.Users.FirstOrDefault(x => x.Email == rootUserEmail) ?? context.Users.Add(
                                      new User
                                      {
                                          Name = "Global",
                                          LastName = "admin",
                                          Email = rootUserEmail,
                                          Salt = salt,
                                          Password = password,
                                          IsGlobalAdmin = true
                                      });

                context.SaveChanges();

                #endregion

                #region OPTIONS

                Console.Out.WriteLine("Initializing options");

                AddOptions<ItProjectType, ItProject>(context.ItProjectTypes, globalAdmin, "F�llesoffentlig", "F�lleskommunal", "Lokal", "Tv�rkommunal", "SKAL", "Udvikling", "Implementering");

                AddOptions<GoalType, Goal>(context.GoalTypes, globalAdmin, "Effektivitet", "Kvalitet", "Service");

                AddOptions<BusinessType, ItSystem>(context.BusinessTypes, globalAdmin, "Desing, visualisering og grafik", "Kommunikation", "Hjemmesider og portaler", "Selvbetjening og indberetning", "E-l�ring", "ESDH og Journalisering", "Specialsystemer", "Processtyring", "IT management", "�konomi og betaling", "L�n, personale og HR", "BI og ledelsesinformation", "Master data og registre", "GIS", "Bruger- og rettighedsstyring", "Sikkerhed og overv�gning", "Sagsb�rende", "Administrativt");

                AddOptions<ArchiveType, ItSystemUsage>(context.ArchiveTypes, globalAdmin, "Arkiveret", "Ikke arkiveret", "Arkiveringspligt", "Ikke arkiveringspligt", "�jebliksbillede", "Periode", "L�bende");

                AddOptions<ArchiveLocation, ItSystemUsage>(context.ArchiveLocation, globalAdmin, "Aalborg", "Aabenraa", "Vejle", "Vejen", "T�rnby", "T�nder", "Thisted", "S�nderborg", "Syddjurs", "Struer", "Slagelse", "Skive", "Silkeborg", "Rudersdal", "Roskilde", "Randers", "Odense", "N�stved", "Norddjurs", "Mariagerfjord", "L�s�", "Lyngby-Taarb�k", "Lolland", "K�benhavn", "Kolding", "Ish�j", "H�rsholm", "Horsens", "Holb�k", "Hj�rring", "Helsing�r", "Hedensted", "Haderslev", "Guldborgsund", "Gribskov", "Greve", "Gladsaxe", "Gentofte", "Fures�", "Frederikssund", "Frederikshavn", "Fredensborg", "Faxe", "Esbjerg", "Egedal", "Drag�r", "Br�ndby", "Bornholm", "Billund");

                AddOptions<DataType, DataRow>(context.DataTypes, globalAdmin, "Person", "Virksomhed", "Sag", "Dokument", "Organisation", "Klassikfikation", "Ejendom", "GIS", "Andet");

                AddOptions<RelationFrequencyType, SystemRelation>(context.RelationFrequencyTypes, globalAdmin, "Dagligt", "Ugentligt", "M�nedligt", "�rligt", "Kvartal", "Halv�rligt");

                AddOptions<InterfaceType, ItInterface>(context.InterfaceTypes, globalAdmin, "CSV", "WS SOAP", "WS REST", "MOX", "OIO REST", "LDAP", "User interface", "ODBC (SQL)", "Andet");

                AddOptions<SensitiveDataType, ItSystemUsage>(context.SensitiveDataTypes, globalAdmin, "Ja", "Nej");

                AddOptions<ItContractType, ItContract>(context.ItContractTypes, globalAdmin, "Hovedkontrakt", "Till�gskontrakt", "Snitflade", "Serviceaftale", "Databehandleraftale");

                AddOptions<ItContractTemplateType, ItContract>(context.ItContractTemplateTypes, globalAdmin, "K01", "K02", "K03", "Egen", "KOMBIT", "Leverand�r", "OPI", "Anden");

                AddOptions<PurchaseFormType, ItContract>(context.PurchaseFormTypes, globalAdmin, "SKI", "SKI 02.18", "SKI 02.19", "Udbud", "EU udbud", "Direkte tildeling", "Annoncering");

                AddOptions<PaymentModelType, ItContract>(context.PaymentModelTypes, globalAdmin, "Licens", "icens - flatrate", "Licens - forbrug", "Licens - indbyggere", "Licens - pr. sag", "Gebyr", "Engangsydelse");

                AddOptions<AgreementElementType, ItContract>(context.AgreementElementTypes, globalAdmin,
                    "Licens", "Udvikling", "Drift", "Vedligehold", "Support",
                    "Serverlicenser", "Serverdrift", "Databaselicenser", "Backup", "Overv�gning");

                AddOptions<OptionExtendType, ItContract>(context.OptionExtendTypes, globalAdmin, "2 x 1 �r", "1 x 1 �r", "1 x � �r");

                AddOptions<PaymentFreqencyType, ItContract>(context.PaymentFreqencyTypes, globalAdmin, "M�nedligt", "Kvartalsvis", "Halv�rligt", "�rligt");

                AddOptions<PriceRegulationType, ItContract>(context.PriceRegulationTypes, globalAdmin, "TSA", "KL pris og l�nsk�n", "Nettoprisindeks");

                AddOptions<ProcurementStrategyType, ItContract>(context.ProcurementStrategyTypes, globalAdmin, "Direkte tildeling", "Annoncering", "Udbud", "EU udbud", "Mini-udbud", "SKI - direkte tildeling", "SKI - mini-udbud", "Underh�ndsbud");

                AddOptions<TerminationDeadlineType, ItContract>(context.TerminationDeadlineTypes, globalAdmin, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12");

                AddOptions<HandoverTrialType, HandoverTrial>(context.HandoverTrialTypes, globalAdmin, "Funktionspr�ve", "Driftovertagelsespr�ve");

                AddOptions<ItSystemCategories, ItSystemUsage>(context.ItSystemCategories, globalAdmin, "Offentlige data", "Interne data", "Fortrolige data", "Hemmelige data");

                AddOptions<ArchiveTestLocation, ItSystemUsage>(context.ArchiveTestLocation, globalAdmin, "TestLocation1", "TestLocation2");

                AddOptions<RegisterType, ItSystemUsage>(context.RegisterTypes, globalAdmin, "TestRegisterType1", "TestRegisterType2");

                context.SaveChanges();

                #endregion

                #region ORG ROLES
                Console.Out.WriteLine("Initializing org roles");

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
                    Name = "Direkt�r",
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

                Console.Out.WriteLine("Initializing project roles");

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
                        Name = "R�dgiver",
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
                Console.Out.WriteLine("Initializing system roles");

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
                Console.Out.WriteLine("Initializing contract roles");

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
                    Name = "Juridisk r�dgiver",
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

                #region DPA ROLES
                Console.Out.WriteLine("Initializing dpa roles");
                context.DataProcessingRegistrationRoles.AddOrUpdate(x => x.Name, new DataProcessingRegistrationRole
                {
                    HasWriteAccess = true,
                    Name = "Standard Skriverolle",
                    IsObligatory = true,
                    IsLocallyAvailable = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 1
                });
                context.DataProcessingRegistrationRoles.AddOrUpdate(x => x.Name, new DataProcessingRegistrationRole
                {
                    HasWriteAccess = false,
                    Name = "Standard L�serolle",
                    IsObligatory = true,
                    IsLocallyAvailable = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 2
                });

                var dpaRolesCount = context.DataProcessingRegistrationRoles.Count();
                foreach (var role in context.ItContractRoles)
                {
                    role.Priority = dpaRolesCount;
                    dpaRolesCount--;
                }

                context.SaveChanges();

                #endregion

                #region ORGANIZATIONS
                Console.Out.WriteLine("Initializing organizations");

                var muniType = new OrganizationType { Name = "Kommune", Category = OrganizationCategory.Municipality };
                var interestType = new OrganizationType { Name = "Interessef�llesskab", Category = OrganizationCategory.Municipality };
                var company = new OrganizationType { Name = "Virksomhed", Category = OrganizationCategory.Other };
                var other = new OrganizationType { Name = "Anden offentlig myndighed", Category = OrganizationCategory.Other };
                context.OrganizationTypes.AddOrUpdate(x => x.Name, muniType, interestType, company, other);

                context.SaveChanges();

                var orgType = context.OrganizationTypes.Single(x => x.Name == "Kommune");

                var commonOrganization = CreateOrganization("F�lles Kommune", orgType, globalAdmin, true);

                context.Organizations.AddOrUpdate(x => x.Name, commonOrganization/*, muni1, muni2*/);
                context.SaveChanges();

                


                #endregion

                #region TEXTS
                Console.Out.WriteLine("Initializing texts");

                if (!context.Texts.Any(x => x.Id == 1))
                {
                    context.Texts.AddOrUpdate(new Text()
                    {
                        Value = @"Forside - blok 1",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                if (!context.Texts.Any(x => x.Id == 2))
                {
                    context.Texts.AddOrUpdate(new Text() { Value = "Forside - blok 2", ObjectOwnerId = globalAdmin.Id, LastChangedByUserId = globalAdmin.Id });
                }

                if (!context.Texts.Any(x => x.Id == 3))
                {
                    context.Texts.AddOrUpdate(new Text()
                    {
                        Value = @"Forside - blok 3",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                if (!context.Texts.Any(x => x.Id == 4))
                {
                    context.Texts.AddOrUpdate(new Text() { Value = "Forside - blok 4", ObjectOwnerId = globalAdmin.Id, LastChangedByUserId = globalAdmin.Id });
                }

                if (!context.Texts.Any(x => x.Id == 5))
                {
                    context.Texts.AddOrUpdate(new Text()
                    {
                        Value = @"Forside - blok 5",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                #endregion
            }
        }

        private static string GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }

        #region Helper methods

        /// <summary>
        /// Creates and returns an Option entity.
        /// </summary>
        /// <typeparam name="T">Type of option to be added.</typeparam>
        /// <typeparam name="TReference">Reference type for the option</typeparam>
        /// <param name="name">The name of the new option entity</param>
        /// <param name="objectOwner">Object owner of the new entity</param>
        /// <param name="note">Description for the entity</param>
        /// <param name="isActive">Is the option active</param>
        /// <returns></returns>
        private static T CreateOption<T, TReference>(string name, User objectOwner, string description = "...", bool isActive = true, bool isObligatory = true, bool isEnabled = true)
            where T : OptionEntity<TReference>, new()
        {
            return new T()
            {
                IsObligatory = isObligatory,
                IsEnabled = isEnabled,
                IsLocallyAvailable = isActive,
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

        private static Organization CreateOrganization(string name, OrganizationType organizationType, User objectOwner = null, bool isDefault = false)
        {
            var org = new Organization
            {
                Name = name,
                Config = Config.Default(objectOwner),
                TypeId = organizationType.Id,
                ObjectOwnerId = objectOwner?.Id,
                LastChangedByUserId = objectOwner?.Id,
                AccessModifier = AccessModifier.Public,
                IsDefaultOrganization = isDefault
            };

            org.OrgUnits.Add(new OrganizationUnit
            {
                Name = org.Name,
                ObjectOwnerId = objectOwner?.Id,
                LastChangedByUserId = objectOwner?.Id
            });

            return org;
        }
       
        #endregion
    }
}

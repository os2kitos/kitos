using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Infrastructure.Services.Cryptography;
using System;
using System.Data.Entity;

namespace Infrastructure.DataAccess.Migrations
{
    using Core.DomainModel.PublicMessage;
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

                AddOptions<BusinessType, ItSystem>(context.BusinessTypes, globalAdmin, "Desing, visualisering og grafik", "Kommunikation", "Hjemmesider og portaler", "Selvbetjening og indberetning", "E-læring", "ESDH og Journalisering", "Specialsystemer", "Processtyring", "IT management", "Økonomi og betaling", "Løn, personale og HR", "BI og ledelsesinformation", "Master data og registre", "GIS", "Bruger- og rettighedsstyring", "Sikkerhed og overvågning", "Sagsbærende", "Administrativt");

                AddOptions<ArchiveType, ItSystemUsage>(context.ArchiveTypes, globalAdmin, "Arkiveret", "Ikke arkiveret", "Arkiveringspligt", "Ikke arkiveringspligt", "Øjebliksbillede", "Periode", "Løbende");

                AddOptions<ArchiveLocation, ItSystemUsage>(context.ArchiveLocation, globalAdmin, "Aalborg", "Aabenraa", "Vejle", "Vejen", "Tårnby", "Tønder", "Thisted", "Sønderborg", "Syddjurs", "Struer", "Slagelse", "Skive", "Silkeborg", "Rudersdal", "Roskilde", "Randers", "Odense", "Næstved", "Norddjurs", "Mariagerfjord", "Læsø", "Lyngby-Taarbæk", "Lolland", "København", "Kolding", "Ishøj", "Hørsholm", "Horsens", "Holbæk", "Hjørring", "Helsingør", "Hedensted", "Haderslev", "Guldborgsund", "Gribskov", "Greve", "Gladsaxe", "Gentofte", "Furesø", "Frederikssund", "Frederikshavn", "Fredensborg", "Faxe", "Esbjerg", "Egedal", "Dragør", "Brøndby", "Bornholm", "Billund");

                AddOptions<DataType, DataRow>(context.DataTypes, globalAdmin, "Person", "Virksomhed", "Sag", "Dokument", "Organisation", "Klassikfikation", "Ejendom", "GIS", "Andet");

                AddOptions<RelationFrequencyType, SystemRelation>(context.RelationFrequencyTypes, globalAdmin, "Dagligt", "Ugentligt", "Månedligt", "Årligt", "Kvartal", "Halvårligt");

                AddOptions<InterfaceType, ItInterface>(context.InterfaceTypes, globalAdmin, "CSV", "WS SOAP", "WS REST", "MOX", "OIO REST", "LDAP", "User interface", "ODBC (SQL)", "Andet");

                AddOptions<SensitiveDataType, ItSystemUsage>(context.SensitiveDataTypes, globalAdmin, "Ja", "Nej");

                AddOptions<ItContractType, ItContract>(context.ItContractTypes, globalAdmin, "Hovedkontrakt", "Tillægskontrakt", "Snitflade", "Serviceaftale", "Databehandleraftale");

                AddOptions<ItContractTemplateType, ItContract>(context.ItContractTemplateTypes, globalAdmin, "K01", "K02", "K03", "Egen", "KOMBIT", "Leverandør", "OPI", "Anden");

                AddOptions<PurchaseFormType, ItContract>(context.PurchaseFormTypes, globalAdmin, "SKI", "SKI 02.18", "SKI 02.19", "Udbud", "EU udbud", "Direkte tildeling", "Annoncering");

                AddOptions<CriticalityType, ItContract>(context.CriticalityTypes, globalAdmin, "Kritikalitet 1", "Kritikalitet 2");

                AddOptions<PaymentModelType, ItContract>(context.PaymentModelTypes, globalAdmin, "Licens", "icens - flatrate", "Licens - forbrug", "Licens - indbyggere", "Licens - pr. sag", "Gebyr", "Engangsydelse");

                AddOptions<AgreementElementType, ItContract>(context.AgreementElementTypes, globalAdmin,
                    "Licens", "Udvikling", "Drift", "Vedligehold", "Support",
                    "Serverlicenser", "Serverdrift", "Databaselicenser", "Backup", "Overvågning");

                AddOptions<OptionExtendType, ItContract>(context.OptionExtendTypes, globalAdmin, "2 x 1 år", "1 x 1 år", "1 x ½ år");

                AddOptions<PaymentFreqencyType, ItContract>(context.PaymentFreqencyTypes, globalAdmin, "Månedligt", "Kvartalsvis", "Halvårligt", "Årligt");

                AddOptions<PriceRegulationType, ItContract>(context.PriceRegulationTypes, globalAdmin, "TSA", "KL pris og lønskøn", "Nettoprisindeks");

                AddOptions<ProcurementStrategyType, ItContract>(context.ProcurementStrategyTypes, globalAdmin, "Direkte tildeling", "Annoncering", "Udbud", "EU udbud", "Mini-udbud", "SKI - direkte tildeling", "SKI - mini-udbud", "Underhåndsbud");

                AddOptions<TerminationDeadlineType, ItContract>(context.TerminationDeadlineTypes, globalAdmin, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12");

                AddOptions<ItSystemCategories, ItSystemUsage>(context.ItSystemCategories, globalAdmin, "Offentlige data", "Interne data", "Fortrolige data", "Hemmelige data");

                AddOptions<ArchiveTestLocation, ItSystemUsage>(context.ArchiveTestLocation, globalAdmin, "TestLocation1", "TestLocation2");

                AddOptions<RegisterType, ItSystemUsage>(context.RegisterTypes, globalAdmin, "TestRegisterType1", "TestRegisterType2");

                AddOptions<CountryCode, Organization>(context.CountryCodes, globalAdmin, "NO", "SE", "UA");
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
                    Name = "Direktør",
                    Description = "...",
                    HasWriteAccess = false,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 1
                };

                try
                {
                    context.OrganizationUnitRoles.AddOrUpdate(role => role.Name, boss, resourcePerson, employee, digitalConsultant, itConsultant, leader, director);
                    context.SaveChanges();
                }
                catch
                {
                    // we don't really care about duplicates
                    // just do nothing
                }

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
                    Name = "Standard Læserolle",
                    IsObligatory = true,
                    IsLocallyAvailable = true,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Priority = 2
                });

                context.SaveChanges();

                #endregion

                #region ORGANIZATIONS
                Console.Out.WriteLine("Initializing organizations");

                var muniType = new OrganizationType { Name = "Kommune", Category = OrganizationCategory.Municipality };
                var interestType = new OrganizationType { Name = "Interessefællesskab", Category = OrganizationCategory.Municipality };
                var company = new OrganizationType { Name = "Virksomhed", Category = OrganizationCategory.Other };
                var other = new OrganizationType { Name = "Anden offentlig myndighed", Category = OrganizationCategory.Other };
                context.OrganizationTypes.AddOrUpdate(x => x.Name, muniType, interestType, company, other);

                context.SaveChanges();

                var orgType = context.OrganizationTypes.Single(x => x.Name == "Kommune");

                var commonOrganization = CreateOrganization("Fælles Kommune", orgType, globalAdmin, true);

                context.Organizations.AddOrUpdate(x => x.Name, commonOrganization/*, muni1, muni2*/);
                context.SaveChanges();




                #endregion

                #region TEXTS
                Console.Out.WriteLine("Initializing texts");

                if (!context.Texts.Any(x => x.Id == 1))
                {
                    context.Texts.AddOrUpdate(new Text()
                    {
                        Value = @"Introduktion til kitos",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                if (!context.Texts.Any(x => x.Id == 2))
                {
                    context.Texts.AddOrUpdate(new Text() { Value = "Misc text, links osv", ObjectOwnerId = globalAdmin.Id, LastChangedByUserId = globalAdmin.Id });
                }

                if (!context.Texts.Any(x => x.Id == 3))
                {
                    context.Texts.AddOrUpdate(new Text()
                    {
                        Value = @"Guides",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                if (!context.Texts.Any(x => x.Id == 4))
                {
                    context.Texts.AddOrUpdate(new Text() { Value = "Statusbeskeder", ObjectOwnerId = globalAdmin.Id, LastChangedByUserId = globalAdmin.Id });
                }

                if (!context.Texts.Any(x => x.Id == 5))
                {
                    context.Texts.AddOrUpdate(new Text()
                    {
                        Value = @"Kontaktinformationer",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                const string longDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer tortor ligula, malesuada nec molestie sit amet, tempor quis elit. Suspendisse porttitor imperdiet lacus, sed ultrices augue pharetra sed. Maecenas rutrum metus non dui blandit, vel dignissim lacus pellentesque. Etiam magna nunc, lacinia blandit diam in, laoreet gravida urna. Suspendisse potenti. Nunc ornare sapien sit amet faucibus eleifend. Nunc eros sapien, hendrerit in lacus sit amet, posuere auctor augue. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Integer imperdiet ante id nunc tincidunt lacinia. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Vivamus pharetra lorem ut libero fringilla iaculis. Curabitur et suscipit leo, non fermentum nibh. Proin id leo vehicula, dignissim felis vitae, imperdiet leo.";

                if (!context.PublicMessages.Any(x => x.Id == 1))
                {
                    context.PublicMessages.AddOrUpdate(new PublicMessage()
                    {
                        Title = @"Vejledninger",
                        LongDescription = longDescription,
                        ShortDescription = "Skabeloner til brug ved oprettelse af IT-Systemer, leverandører og snitflader finder du her.",
                        Link = "https://google.com",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                if (!context.PublicMessages.Any(x => x.Id == 2))
                {
                    context.PublicMessages.AddOrUpdate(new PublicMessage()
                    {
                        Title = @"Skabeloner",
                        LongDescription = longDescription,
                        ShortDescription = "Brugerklubben i Kitos har et filarkiv, hvor du og dine kolleger kan tilgå materiale om Kitos.",
                        Link = "https://google.com",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                if (!context.PublicMessages.Any(x => x.Id == 4))
                {
                    context.PublicMessages.AddOrUpdate(new PublicMessage()
                    {
                        Title = @"Driftsstatus",
                        LongDescription = longDescription,
                        ShortDescription = "Hvis der opleves fejl i Kitos vil du kunne se en begrundelse på denne side.",
                        Link = "https://google.com",
                        Status = PublicMessageStatus.Active,
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                if (!context.PublicMessages.Any(x => x.Id == 3))
                {
                    context.PublicMessages.AddOrUpdate(new PublicMessage()
                    {
                        Title = @"Arrangementer",
                        LongDescription = longDescription,
                        ShortDescription = "Brugerklubben holder løbende arrangementer, som du har mulighed for at tilmelde dig.",
                        Link = "https://google.com",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }


                if (!context.PublicMessages.Any(x => x.Id == 5))
                {
                    context.PublicMessages.AddOrUpdate(new PublicMessage()
                    {
                        Title = @"OS2Kitos kontaktpersoner",
                        LongDescription = longDescription,
                        ShortDescription = "Se hvem, der er Kitos kontaktpersoner i kommunerne og spar med hinanden.",
                        Link = "https://google.com",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                if (!context.PublicMessages.Any(x => x.Id == 6))
                {
                    context.PublicMessages.AddOrUpdate(new PublicMessage()
                    {
                        Title = @"Kontakt",
                        LongDescription = longDescription,
                        ShortDescription = "Har du nogen spørgsmål til Kitos? Kontakt os og vi vil hjælpe dig hurtigst muligt.",
                        Link = "https://google.com",
                        ObjectOwnerId = globalAdmin.Id,
                        LastChangedByUserId = globalAdmin.Id
                    });
                }

                if (!context.PublicMessages.Any(x => x.Id == 7))
                {
                    context.PublicMessages.AddOrUpdate(new PublicMessage
                    {
                        Title = "Kitos er Kommunernes IT Overblikssystem",
                        ShortDescription = "Kitos er en open-source web-baseret løsning, der anvendes af 76 kommuner. Kitos skaber overblik over den samlede kommunale IT-portefølje.",
                        Link = "https://www.os2.eu/os2kitos",
                        IsMain = true,
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
            var i = options.Count;
            foreach (var option in options)
            {
                option.Priority = i;
                i--;
            }
            try
            {
                dbSet.AddOrUpdate(x => x.Name, options.ToArray());
            }
            catch
            {
                // we don't really care about duplicates
                // just do nothing
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

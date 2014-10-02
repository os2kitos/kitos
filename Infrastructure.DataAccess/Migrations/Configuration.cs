using System.Collections.Generic;
using System.Data.Entity;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

namespace Infrastructure.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<KitosContext>
    {

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());

            //Use a smaller key size for our migration history table. See MySqlHistoryContext.cs
            SetHistoryContextFactory("MySql.Data.MySqlClient", (conn, schema) => new MySqlHistoryContext(conn, schema));
        }

        /// <summary>
        /// Seeds the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void Seed(KitosContext context)
        {
            #region USERS

            var cryptoService = new CryptoService();

            var globalAdmin = CreateUser("Global admin", "support@kitos.dk", "KitosAgent", cryptoService);
            globalAdmin.IsGlobalAdmin = true;
            
            context.Users.AddOrUpdate(x => x.Email, globalAdmin);
            context.SaveChanges();

            globalAdmin = context.Users.Single(x => x.Email == "support@kitos.dk");
            
            #endregion

            #region OPTIONS

            AddOptions<ItProjectType, ItProject>(context.ProjectTypes, globalAdmin, "Fællesoffentlig", "Fælleskommunal", "Lokal", "Tværkommunal", "SKAL", "Udvikling", "Implementering");

            AddOptions<GoalType, Goal>(context.GoalTypes, globalAdmin, "Effektivitet", "Kvalitet", "Service");

            AddOptions<ItSystemTypeOption, ItSystem>(context.ItSystemTypeOptions, globalAdmin, "Fagsystem", "Selvbetjening", "Applikation", "Brugerinterface", "Programmeringsinterface", "Applikationsservice", "Applikationskomponent", "Applikationsfunktion", "Applikationsmodul");

            AddOptions<BusinessType, ItSystem>(context.BusinessTypes, globalAdmin, "Desing, visualisering og grafik", "Kommunikation", "Hjemmesider og portaler", "Selvbetjening og indberetning", "E-læring", "ESDH og Journalisering", "Specialsystemer", "Processtyring", "IT management", "Økonomi og betaling", "Løn, personale og HR", "BI og ledelsesinformation", "Master data og registre", "GIS", "Bruger- og rettighedsstyring", "Sikkerhed og overvågning", "Sagsbærende", "Administrativt");

            AddOptions<ArchiveType, ItSystemUsage>(context.ArchiveTypes, globalAdmin, "Arkiveret", "Ikke arkiveret", "Arkiveringspligt", "ikke arkiveringspligt");

            AddOptions<DataType, DataRow>(context.DataTypes, globalAdmin, "Person", "Virksomhed", "Sag", "Dokument", "Organisation", "Klassikfikation", "Ejendom", "GIS", "Andet");

            AddOptions<Frequency, DataRowUsage>(context.Frequencies, globalAdmin, "Dagligt", "Ugentligt", "Månedligt", "Årligt", "Kvartal", "Halvårligt"); 

            AddOptions<InterfaceType, ItInterface>(context.InterfaceTypes, globalAdmin, "Webservice", "API", "iFrame", "Link", "Link - dybt", "Andet");
            
            AddOptions<Interface, ItInterface>(context.Interfaces, globalAdmin, "CSV", "WS SOAP", "WS REST", "MOX", "OIO REST", "LDAP", "User interface", "ODBC (SQL)", "Andet");

            AddOptions<Method, ItInterface>(context.Methods, globalAdmin, "Batch", "Request-Response", "Store and forward", "Publish-subscribe", "App interface", "Andet");

            AddOptions<SensitiveDataType, ItSystemUsage>(context.SensitiveDataTypes, globalAdmin, "Ja", "Nej");
            
            AddOptions<Tsa, ItInterface>(context.Tsas, globalAdmin, "Ja", "Nej");

            AddOptions<ContractType, ItContract>(context.ContractTypes, globalAdmin, "Hovedkontrakt", "Tillægskontrakt", "Snitflade", "Serviceaftale", "Databehandleraftale");

            AddOptions<ContractTemplate, ItContract>(context.ContractTemplates, globalAdmin, "K01", "K02", "K03", "Egen", "KOMBIT", "Leverandør", "OPI", "Anden");

            AddOptions<PurchaseForm, ItContract>(context.PurchaseForms, globalAdmin, "SKI", "SKI 02.18", "SKI 02.19", "Udbud", "EU udbud", "Direkte tildeling", "Annoncering");

            AddOptions<PaymentModel, ItContract>(context.PaymentModels, globalAdmin, "Licens", "icens - flatrate", "Licens - forbrug", "Licens - indbyggere", "Licens - pr. sag", "Gebyr", "Engangsydelse");
            
            AddOptions<AgreementElement, ItContract>(context.AgreementElements, globalAdmin, 
                "Licens", "Udvikling", "Drift", "Vedligehold", "Support", 
                "Serverlicenser", "Serverdrift", "Databaselicenser", "Backup", "Overvågning");

            AddOptions<OptionExtend, ItContract>(context.OptionExtention, globalAdmin, "2 x 1 år", "1 x 1 år", "1 x ½ år");

            AddOptions<PaymentFreqency, ItContract>(context.PaymentFreqencies, globalAdmin, "Månedligt", "Kvartalsvis", "Halvårligt", "Årligt");

            AddOptions<PriceRegulation, ItContract>(context.PriceRegulations, globalAdmin, "TSA", "KL pris og lønskøn", "Nettoprisindeks");

            AddOptions<ProcurementStrategy, ItContract>(context.ProcurementStrategies, globalAdmin, "Direkte tildeling", "Annoncering", "Udbud", "EU udbud", "Mini-udbud", "SKI - direkte tildeling", "SKI - mini-udbud", "Underhåndsbud");

            AddOptions<TerminationDeadline, ItContract>(context.TerminationDeadlines, globalAdmin, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12");

            AddOptions<HandoverTrialType, HandoverTrial>(context.HandoverTrialTypes, globalAdmin, "Funktionsprøve", "Driftovertagelsesprøve"); 

            context.SaveChanges();

            #endregion
            
            #region ADMIN ROLES

            var localAdmin = new AdminRole
            {
                Name = "LocalAdmin",
                IsActive = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };
            context.AdminRoles.AddOrUpdate(x => x.Name, localAdmin);
            context.SaveChanges();

            #endregion
            
            #region ORG ROLES

            var boss = new OrganizationRole()
            {
                IsActive = true,
                Name = "Chef",
                Note = "Lederen af en organisationsenhed",
                HasWriteAccess = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var resourcePerson = new OrganizationRole()
            {
                IsActive = true,
                Name = "Ressourceperson",
                Note = "...",
                HasWriteAccess = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var employee = new OrganizationRole()
            {
                IsActive = true,
                Name = "Medarbejder",
                Note = "...",
                HasWriteAccess = false,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var digitalConsultant = new OrganizationRole()
            {
                IsActive = true,
                Name = "Digitaliseringskonsulent",
                Note = "...",
                HasWriteAccess = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var itConsultant = new OrganizationRole()
            {
                IsActive = true,
                Name = "IT konsulent",
                Note = "...",
                HasWriteAccess = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var leader = new OrganizationRole()
            {
                IsActive = true,
                Name = "Leder",
                Note = "...",
                HasWriteAccess = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var director = new OrganizationRole()
            {
                IsActive = true,
                Name = "Direktør",
                Note = "...",
                HasWriteAccess = false,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            context.OrganizationRoles.AddOrUpdate(role => role.Name, boss, resourcePerson, employee, digitalConsultant, itConsultant, leader, director);
            context.SaveChanges();

            #endregion

            #region PROJECT ROLES

            context.ItProjectRoles.AddOrUpdate(r => r.Name,
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Projektejer",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Projektleder",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Delprojektleder",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Projektdeltager",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Teknisk projektleder",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "IT konsulnet",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Implementeringskonsulent",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Proceskonsulent",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Juridisk konsulent",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "IT arkitekt",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Testansvarlig",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Support",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Programleder",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Styregruppeformand",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Styregruppemedlem",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Forretningsejer",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Forretningsansvarlig",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Gevinstejer",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Gevinsansvarlig",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                },
                new ItProjectRole()
                {
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Rådgiver",
                    ObjectOwner = globalAdmin,
                    LastChangedByUser = globalAdmin
                });
            context.SaveChanges();

            #endregion
            
            #region SYSTEM ROLES

            var systemOwnerRole = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = true,
                IsActive = true,
                Name = "Systemejer",
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var systemResponsibleRole = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = true,
                IsActive = true,
                Name = "Systemansvarlig",
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var businessOwnerRole = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = true,
                IsActive = true,
                Name = "Forretningsejer",
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var superuserResponsibleRole = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = true,
                IsActive = true,
                Name = "Superbrugeransvarlig",
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var superuserRole = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = true,
                IsActive = true,
                Name = "Superbruger",
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var securityResponsibleRole = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = true,
                IsActive = true,
                Name = "Sikkerhedsansvarlig",
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var chanceManagerRole = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = true,
                IsActive = true,
                Name = "Changemanager",
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var dataOwnerRole = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = true,
                IsActive = true,
                Name = "Dataejer",
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            var systemAdminRole = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = true,
                IsActive = true,
                Name = "Systemadminstrator",
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            };

            context.ItSystemRoles.AddOrUpdate(x => x.Name, systemOwnerRole, systemResponsibleRole, businessOwnerRole, superuserResponsibleRole, superuserRole, securityResponsibleRole, chanceManagerRole, dataOwnerRole, systemAdminRole);
            context.SaveChanges();

            #endregion

            #region CONTRACT ROLES

            context.ItContractRoles.AddOrUpdate(x => x.Name, new ItContractRole()
            {
                HasWriteAccess = true,
                Name = "Kontraktejer",
                IsActive = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            }, new ItContractRole()
            {
                HasWriteAccess = true,
                Name = "Kontraktmanager",
                IsActive = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            }, new ItContractRole()
            {
                HasWriteAccess = true,
                Name = "Juridisk rådgiver",
                IsActive = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            }, new ItContractRole()
            {
                HasWriteAccess = true,
                Name = "Konsulent",
                IsActive = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            }, new ItContractRole()
            {
                HasWriteAccess = true,
                Name = "Fakturamodtager",
                IsActive = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            }, new ItContractRole()
            {
                HasWriteAccess = true,
                Name = "Budgetansvarlig",
                IsActive = true,
                ObjectOwner = globalAdmin,
                LastChangedByUser = globalAdmin
            });
            context.SaveChanges();

            #endregion

            #region ORGANIZATIONS

            var commonOrganization = CreateOrganization("Fælles Kommune", OrganizationType.Municipality, globalAdmin);

            context.Organizations.AddOrUpdate(x => x.Name, commonOrganization);
            context.SaveChanges();

            commonOrganization = context.Organizations.Single(x => x.Name == "Fælles Kommune");

            SetUserCreatedOrganization(globalAdmin, commonOrganization);

            #endregion

            #region TEXTS

            context.Texts.AddOrUpdate(x => x.Id,
                                      new Text() { Value = @"KITOS - Kommunernes IT OverbliksSystem er et IT System, som er udviklet i de 3 første kvartaler i 2014 af Roskilde, Sorø, Ringsted, Syddjurs og Ballerup kommune. 
Et væsentligt formål med projektet er at skabe et ensartet grundlag for hvordan vi som kommuner kan øge vores modenhed og evne til fremadrettet at 1) skabe overblik over 2) dokumentere og 3) analysere på vores samlede IT portefølje m.v. I forlængelse heraf er det en løsning, som skal hjælpe os med at understøtte det vidensbehov, som vi mener at monopolbruddet kræver – herunder kvalificere vores evne til at udnytte rammearkitekturen. 
KITOS er bygget op omkring flg. moduler: 
1.	IT understøttelse af organisation
2.	IT Projekter
3.	IT Systemer
4.	IT Kontrakter
 Løsningen er ’overdraget’ til det digitale fællesskab OS2, som vil sørge for forvaltning af KITOS med hensyn til hosting, vedligeholdelse, videreudvikling, administration etc, så den også i praksis vil være mulig for andre kommuner at bruge.
De første kommuner tager KITOS i brug i oktober 2014.
Du kan læse mere om OS2KITOS på os2web.dk > Projekter > KITOS
Kontakt: info@kitos.dk", ObjectOwner = globalAdmin, LastChangedByUser = globalAdmin },
                                      new Text() { Value = "Der er p.t ingen driftsforstyrrelser", ObjectOwner = globalAdmin, LastChangedByUser = globalAdmin });

            #endregion
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
        /// <param name="note">Note for the entity</param>
        /// <param name="isActive">Is the option active</param>
        /// <param name="isSuggestion">Is the option an suggestion</param>
        /// <returns></returns>
        private static T CreateOption<T, TReference>(string name, User objectOwner, string note = "...", bool isActive = true, bool isSuggestion = false)
            where T : Entity, IOptionEntity<TReference>, new()
        {
            return new T()
                {
                    IsActive = isActive,
                    IsSuggestion = isSuggestion,
                    Name = name,
                    Note = note,
                    ObjectOwner = objectOwner,
                    LastChangedByUser = objectOwner
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
        private static void AddOptions<T, TReference>(IDbSet<T> dbSet, User objectOwner, params string[] names)
            where T : Entity, IOptionEntity<TReference>, new()
        {
            var options = names.Select(name => CreateOption<T, TReference>(name, objectOwner)).ToArray();
            dbSet.AddOrUpdate(x => x.Name, options);
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
                    ObjectOwner = objectOwner,
                    LastChangedByUser = objectOwner
                };
        }

        private static Organization CreateOrganization(string name, OrganizationType organizationType, User objectOwner = null)
        {
            var org = new Organization
            {
                Name = name,
                Config = Config.Default(objectOwner),
                Type = organizationType,
                ObjectOwner = objectOwner,
                LastChangedByUser = objectOwner
            };

            org.OrgUnits.Add(new OrganizationUnit()
            {
                Name = org.Name,
                ObjectOwner = objectOwner,
                LastChangedByUser = objectOwner
            });

            return org;
        }
        
        /// <summary>
        /// Helper function for setting the CreatedIn and DefaultOrganizationUnit properties.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="organization"></param>
        private static void SetUserCreatedOrganization(User user, Organization organization)
        {
            user.CreatedIn = organization;
            user.DefaultOrganizationUnit = organization.GetRoot();
        }

        #endregion
    }
}

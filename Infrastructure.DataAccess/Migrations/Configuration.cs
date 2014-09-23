using System;
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

    internal sealed class Configuration : DbMigrationsConfiguration<Infrastructure.DataAccess.KitosContext>
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
        protected override void Seed(Infrastructure.DataAccess.KitosContext context)
        {
            #region USERS

            var cryptoService = new CryptoService();

            var brian = CreateUser("Brian", "briana2604@hotmail.com", "123", cryptoService);
            brian.IsGlobalAdmin = true;
            
            var agent = CreateUser("Agent", "erik.helweg@gmail.com", "123", cryptoService);
            agent.IsGlobalAdmin = true;

            context.Users.AddOrUpdate(x => x.Email, brian, agent);
            context.SaveChanges();
            
            #endregion

            #region OPTIONS
            
            AddOptions<ItProjectType, ItProject>(context.ProjectTypes, brian, "Fællesoffentlig", "Fælleskommunal");
            
            AddOptions<ItSystemTypeOption, ItSystem>(context.ItSystemTypeOptions, brian, "Fagsystem", "Selvbetjening");

            AddOptions<BusinessType, ItSystem>(context.BusinessTypes, brian, "Forretningstype 1", "Forretningstype 2", "Forretningstype 3");

            AddOptions<Interface, ItInterface>(context.Interfaces, brian, "Grænseflade 1", "Grænseflade 2", "Grænseflade 3");

            AddOptions<Tsa, ItInterface>(context.Tsas, brian, "Ja", "Nej");

            AddOptions<InterfaceType, ItInterface>(context.InterfaceTypes, brian, "WS");

            AddOptions<DataType, DataRow>(context.DataTypes, brian, "Datatype 1", "Datatype 2", "Datatype 3");

            AddOptions<Method, ItInterface>(context.Methods, brian, "Batch", "Request-Response");

            AddOptions<ContractType, ItContract>(context.ContractTypes, brian, "Hovedkontrakt", "Tillægskontrakt", "Snitflade");

            AddOptions<ContractTemplate, ItContract>(context.ContractTemplates, brian, "K01", "K02", "K03");

            AddOptions<PurchaseForm, ItContract>(context.PurchaseForms, brian, "SKI", "SKI 02.19", "Udbud");

            AddOptions<ProcurementStrategy, ItContract>(context.ProcurementStrategies, brian, "Strategi 1", "Strategi 2", "Strategi 3");

            AddOptions<AgreementElement, ItContract>(context.AgreementElements, brian, 
                "Licens", "Udvikling", "Drift", "Vedligehold", "Support", 
                "Serverlicenser", "Serverdrift", "Databaselicenser", "Backup", "Overvågning");

            AddOptions<Frequency, DataRowUsage>(context.Frequencies, brian, "Dagligt", "Ugentligt", "Månedligt", "Årligt"); 
            
            AddOptions<ArchiveType, ItSystemUsage>(context.ArchiveTypes, brian, "Arkiveret", "Ikke arkiveret");

            AddOptions<SensitiveDataType, ItSystemUsage>(context.SensitiveDataTypes, brian, "Ja", "Nej");

            AddOptions<GoalType, Goal>(context.GoalTypes, brian, "Måltype 1", "Måltype 2");

            AddOptions<OptionExtend, ItContract>(context.OptionExtention, brian, "2 x 1 år");

            AddOptions<TerminationDeadline, ItContract>(context.TerminationDeadlines, brian, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12");

            AddOptions<PaymentFreqency, ItContract>(context.PaymentFreqencies, brian, "Frekvens A", "Frekvens B");

            AddOptions<PaymentModel, ItContract>(context.PaymentModels, brian, "Model A", "Model B");

            AddOptions<PriceRegulation, ItContract>(context.PriceRegulations, brian, "Pris regulering A", "Pris regulering B"); 

            AddOptions<HandoverTrialType, HandoverTrial>(context.HandoverTrialTypes, brian, "Prøve 1", "Prøve 2"); 

            context.SaveChanges();

            #endregion
            
            #region ADMIN ROLES

            var localAdmin = new AdminRole
            {
                Name = "LocalAdmin",
                IsActive = true,
                ObjectOwner = brian,
                LastChangedByUser = brian
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
                ObjectOwner = brian,
                LastChangedByUser = brian
            };

            var resourcePerson = new OrganizationRole()
            {
                IsActive = true,
                Name = "Ressourceperson",
                Note = "...",
                HasWriteAccess = true,
                ObjectOwner = brian,
                LastChangedByUser = brian
            };

            var employee = new OrganizationRole()
            {
                IsActive = true,
                Name = "Medarbejder",
                Note = "...",
                HasWriteAccess = false,
                ObjectOwner = brian,
                LastChangedByUser = brian
            };

            context.OrganizationRoles.AddOrUpdate(role => role.Name, boss, resourcePerson, employee);
            context.SaveChanges();

            #endregion

            #region PROJECT ROLES

            context.ItProjectRoles.AddOrUpdate(r => r.Name,
                new ItProjectRole()
                {
                    IsActive = true,
                    Name = "Projektejer",
                    ObjectOwner = brian,
                    LastChangedByUser = brian
                },
                new ItProjectRole()
                {
                    IsActive = true,
                    Name = "Projektleder",
                    ObjectOwner = brian,
                    LastChangedByUser = brian
                },
                new ItProjectRole()
                {
                    IsActive = true,
                    Name = "Delprojektleder",
                    ObjectOwner = brian,
                    LastChangedByUser = brian
                },
                new ItProjectRole()
                {
                    IsActive = true,
                    Name = "Projektdeltager",
                    ObjectOwner = brian,
                    LastChangedByUser = brian
                });
            context.SaveChanges();

            #endregion
            
            #region SYSTEM ROLES

            var systemRole1 = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = true,
                IsActive = true,
                Name = "Systemrolle 1",
                ObjectOwner = brian,
                LastChangedByUser = brian
            };

            var systemRole2 = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = false,
                IsActive = true,
                Name = "Systemrolle 2",
                ObjectOwner = brian,
                LastChangedByUser = brian
            };

            context.ItSystemRoles.AddOrUpdate(x => x.Name, systemRole1, systemRole2);
            context.SaveChanges();

            #endregion

            #region CONTRACT ROLES

            context.ItContractRoles.AddOrUpdate(x => x.Name, new ItContractRole()
            {
                HasWriteAccess = true,
                Name = "Kontraktrolle A",
                IsActive = true,
                ObjectOwner = brian,
                LastChangedByUser = brian
            }, new ItContractRole()
            {
                HasWriteAccess = true,
                Name = "Kontraktrolle B",
                IsActive = true,
                ObjectOwner = brian,
                LastChangedByUser = brian
            }, new ItContractRole()
            {
                HasWriteAccess = false,
                Name = "Kontraktrolle C",
                IsActive = true,
                ObjectOwner = brian,
                LastChangedByUser = brian
            });
            context.SaveChanges();

            #endregion

            #region ORGANIZATIONS

            var commonOrganization = CreateOrganization("Fælles Kommune", OrganizationType.Municipality, brian);

            context.Organizations.AddOrUpdate(x => x.Name, commonOrganization);
            context.SaveChanges();

            SetUserCreatedOrganization(brian, commonOrganization);
            SetUserCreatedOrganization(agent, commonOrganization);

            #endregion

            #region TEXTS

            context.Texts.AddOrUpdate(x => x.Id,
                                      new Text() { Value = "Om kitos blablabla", ObjectOwner = brian, LastChangedByUser = brian },
                                      new Text() { Value = "Status blablabla", ObjectOwner = brian, LastChangedByUser = brian });

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

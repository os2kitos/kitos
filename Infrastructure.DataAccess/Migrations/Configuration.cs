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


        private List<TaskRef> GenerateTasks(User objectOwner, OrganizationUnit orgUnitOwner, string type, int n = 20,
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

        private IEnumerable<TaskRef> GenerateAllTasks(User objectOwner, OrganizationUnit orgUnitOwner)
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

        
        protected override void Seed(Infrastructure.DataAccess.KitosContext context)
        {
            #region USERS

            var cryptoService = new CryptoService();

            var globalUser = CreateUser("Global", "g@test", "test", cryptoService);
            globalUser.IsGlobalAdmin = true;

            var localUser = CreateUser("Local Test Bruger", "l@test", "test", cryptoService, globalUser);

            var simon = CreateUser("Simon Lynn-Pedersen", "slp@it-minds.dk", "slp123", cryptoService, globalUser);
            simon.IsGlobalAdmin = true;

            var eskild = CreateUser("Eskild", "esd@it-minds.dk", "123", cryptoService, globalUser);
            eskild.IsGlobalAdmin = true;

            var brian = CreateUser("Brian", "briana@roskilde.dk", "123", cryptoService, globalUser);
            brian.IsGlobalAdmin = true;

            var user1 = CreateUser("Pia", "pia@it-minds.dk", "arne123", cryptoService, globalUser);
            var user2 = CreateUser("Morten", "morten@it-minds.dk", "arne123", cryptoService, globalUser);
            var user3 = CreateUser("Anders", "anders@it-minds.dk", "arne123", cryptoService, globalUser);
            var user4 = CreateUser("Peter", "peter@it-minds.dk", "arne123", cryptoService, globalUser);
            var user5 = CreateUser("Jesper", "jesper@it-minds.dk", "arne123", cryptoService, globalUser);

            var erik = CreateUser("Erik", "ehl@kl.dk", "123", cryptoService);

            context.Users.AddOrUpdate(x => x.Email, simon, globalUser, localUser, user1, user2, user3, user4, user5, brian, erik);
            context.SaveChanges();
            
            #endregion

            #region OPTIONS
            
            AddOptions<ItProjectType, ItProject>(context.ProjectTypes, globalUser, "Fællesoffentlig", "Fælleskommunal");
            
            //AddOptions<AppType, ItSystem>(context.AppTypes, globalUser, "Snitflade", "Fagsystem", "Selvbetjening");

            AddOptions<BusinessType, ItSystem>(context.BusinessTypes, globalUser, "Forretningstype 1", "Forretningstype 2", "Forretningstype 3");

            AddOptions<Interface, ItInterface>(context.Interfaces, globalUser, "Grænseflade 1", "Grænseflade 2", "Grænseflade 3");

            AddOptions<Tsa, ItInterface>(context.Tsas, globalUser, "Ja", "Nej");

            AddOptions<InterfaceType, ItInterface>(context.InterfaceTypes, globalUser, "WS");

            AddOptions<DataType, DataRow>(context.DataTypes, globalUser, "Datatype 1", "Datatype 2", "Datatype 3");

            AddOptions<Method, ItInterface>(context.Methods, globalUser, "Batch", "Request-Response");

            AddOptions<ContractType, ItContract>(context.ContractTypes, globalUser, "Hovedkontrakt", "Tillægskontrakt", "Snitflade");

            AddOptions<ContractTemplate, ItContract>(context.ContractTemplates, globalUser, "K01", "K02", "K03");

            AddOptions<PurchaseForm, ItContract>(context.PurchaseForms, globalUser, "SKI", "SKI 02.19", "Udbud");

            AddOptions<ProcurementStrategy, ItContract>(context.ProcurementStrategies, globalUser, "Strategi 1", "Strategi 2", "Strategi 3");

            AddOptions<AgreementElement, ItContract>(context.AgreementElements, globalUser, 
                "Licens", "Udvikling", "Drift", "Vedligehold", "Support", 
                "Serverlicenser", "Serverdrift", "Databaselicenser", "Backup", "Overvågning");

            AddOptions<Frequency, DataRowUsage>(context.Frequencies, globalUser, "Dagligt", "Ugentligt", "Månedligt", "Årligt"); 
            
            AddOptions<ArchiveType, ItSystemUsage>(context.ArchiveTypes, globalUser, "Arkiveret", "Ikke arkiveret");

            AddOptions<SensitiveDataType, ItSystemUsage>(context.SensitiveDataTypes, globalUser, "Ja", "Nej");

            AddOptions<GoalType, Goal>(context.GoalTypes, globalUser, "Måltype 1", "Måltype 2");

            AddOptions<OptionExtend, ItContract>(context.OptionExtention, globalUser, "2 x 1 år");

            AddOptions<TerminationDeadline, ItContract>(context.TerminationDeadlines, globalUser, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12");

            AddOptions<PaymentFreqency, ItContract>(context.PaymentFreqencies, globalUser, "Frekvens A", "Frekvens B");

            AddOptions<PaymentModel, ItContract>(context.PaymentModels, globalUser, "Model A", "Model B");

            AddOptions<PriceRegulation, ItContract>(context.PriceRegulations, globalUser, "Pris regulering A", "Pris regulering B"); 

            AddOptions<HandoverTrialType, HandoverTrial>(context.HandoverTrialTypes, globalUser, "Prøve 1", "Prøve 2"); 

            context.SaveChanges();

            #endregion
            
            #region ADMIN ROLES

            var localAdmin = new AdminRole
            {
                Name = "LocalAdmin",
                IsActive = true,
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser
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
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser
            };

            var resourcePerson = new OrganizationRole()
            {
                IsActive = true,
                Name = "Ressourceperson",
                Note = "...",
                HasWriteAccess = true,
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser
            };

            var employee = new OrganizationRole()
            {
                IsActive = true,
                Name = "Medarbejder",
                Note = "...",
                HasWriteAccess = false,
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser
            };

            context.OrganizationRoles.AddOrUpdate(role => role.Id, boss, resourcePerson, employee);
            context.SaveChanges();

            #endregion

            #region PROJECT ROLES

            context.ItProjectRoles.AddOrUpdate(r => r.Id,
                new ItProjectRole()
                {
                    IsActive = true,
                    Name = "Projektejer",
                    ObjectOwner = globalUser,
                    LastChangedByUser = globalUser
                },
                new ItProjectRole()
                {
                    IsActive = true,
                    Name = "Projektleder",
                    ObjectOwner = globalUser,
                    LastChangedByUser = globalUser
                },
                new ItProjectRole()
                {
                    IsActive = true,
                    Name = "Delprojektleder",
                    ObjectOwner = globalUser,
                    LastChangedByUser = globalUser
                },
                new ItProjectRole()
                {
                    IsActive = true,
                    Name = "Projektdeltager",
                    ObjectOwner = globalUser,
                    LastChangedByUser = globalUser
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
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser
            };

            var systemRole2 = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = false,
                IsActive = true,
                Name = "Systemrolle 2",
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser
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
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser
            }, new ItContractRole()
            {
                HasWriteAccess = true,
                Name = "Kontraktrolle B",
                IsActive = true,
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser
            }, new ItContractRole()
            {
                HasWriteAccess = false,
                Name = "Kontraktrolle C",
                IsActive = true,
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser
            });
            context.SaveChanges();

            #endregion

            #region ORGANIZATIONS

            var roskilde = CreateOrganization("Roskilde", OrganizationType.Municipality, globalUser);
            var sorø = CreateOrganization("Sorø", OrganizationType.Municipality, globalUser);
            var kl = CreateOrganization("KL", OrganizationType.Municipality, globalUser);
            var companyA = CreateOrganization("Firma A", OrganizationType.Company, globalUser);
            var companyB = CreateOrganization("Firma B", OrganizationType.Company, globalUser);
            var companyC = CreateOrganization("Firma C", OrganizationType.Company, globalUser);

            context.Organizations.AddOrUpdate(x => x.Name, roskilde, sorø, kl, companyA, companyB, companyC);
            context.SaveChanges();

            foreach (var user in context.Users)
            {
                SetUserCreatedOrganization(user, roskilde);
            }

            SetUserCreatedOrganization(erik, kl);

            #endregion

            #region ADMIN RIGHTS

            context.AdminRights.AddOrUpdate(right => new { right.ObjectId, right.RoleId, right.UserId },
                                            new AdminRight()
                                            {
                                                Object = roskilde,
                                                Role = localAdmin,
                                                User = user1,
                                                ObjectOwner = globalUser,
                                                LastChangedByUser = globalUser
                                            },
                                            new AdminRight()
                                            {
                                                Object = kl,
                                                Role = localAdmin,
                                                User = erik,
                                                ObjectOwner = globalUser,
                                                LastChangedByUser = globalUser
                                            },
                                            new AdminRight()
                                            {
                                                Object = roskilde,
                                                Role = localAdmin,
                                                User = localUser,
                                                ObjectOwner = globalUser,
                                                LastChangedByUser = globalUser
                                            });

            context.SaveChanges();

            #endregion

            #region ROSKILDE ORG UNITS

            //LEVEL 0
            var roskildeRoot = roskilde.OrgUnits.First();

            //LEVEL 1
            var munChief = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = roskildeRoot,
                Name = "Kommunaldirektøren"
            };
            var wellfare = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = roskildeRoot,
                Name = "Velfærd"
            };

            //LEVEL 2
            var digi = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = munChief,
                Name = "Digitalisering og Borgerservice"
            };

            var hrcouncil = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = munChief,
                Name = "HR og Byråd"
            };

            var elderArea = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = wellfare,
                Name = "Ældreområdet"
            };

            //LEVEL 3
            var itservice = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = digi,
                Name = "IT Service"
            };
            var projectunit = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = digi,
                Name = "Projektenheden"
            };
            var citizenservice = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = digi,
                Name = "Borgerservice"
            };
            var hr = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = hrcouncil,
                Name = "HR"
            };
            var nursinghome = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = elderArea,
                Name = "Plejehjem"
            };

            //LEVEL 4
            var infra = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = itservice,
                Name = "Infrastruktur"
            };
            var teamcontact = new OrganizationUnit()
            {
                Organization = roskilde, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = citizenservice,
                Name = "Team Kontaktcenter"
            };

            context.OrganizationUnits.AddOrUpdate(o => o.Name, munChief, wellfare, digi, hrcouncil, elderArea, itservice, projectunit, citizenservice, hr, nursinghome, infra, teamcontact);
            context.SaveChanges();

            #endregion

            #region SORØ ORG UNITS

            //LEVEL 0
            var sorøRoot = sorø.OrgUnits.First();

            //LEVEL 1
            var level1a = new OrganizationUnit()
            {
                Organization = sorø, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = sorøRoot,
                Name = "Direktørområde"
            };

            //LEVEL 2
            var level2a = new OrganizationUnit()
            {
                Organization = sorø, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = level1a,
                Name = "Afdeling 1"
            };

            var level2b = new OrganizationUnit()
            {
                Organization = sorø, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = level1a,
                Name = "Afdeling 2"
            };

            var level2c = new OrganizationUnit()
            {
                Organization = sorø, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = level1a,
                Name = "Afdeling 3"
            };

            //LEVEL 2
            var level3a = new OrganizationUnit()
            {
                Organization = sorø, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = level2a,
                Name = "Afdeling 1a"
            };

            var level3b = new OrganizationUnit()
            {
                Organization = sorø, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = level2b,
                Name = "Afdeling 2a"
            };

            var level3c = new OrganizationUnit()
            {
                Organization = sorø, 
                ObjectOwner = globalUser,
                LastChangedByUser = globalUser,
                Parent = level2b,
                Name = "Afdeling 2b"
            };

            context.OrganizationUnits.AddOrUpdate(o => o.Name, level1a, level2a, level2b, level2c, level3a, level3b, level3c);
            context.SaveChanges();

            #endregion

            #region KL ORG UNITS

            var klRootUnit = kl.OrgUnits.First();

            #endregion

            #region TEXTS

            context.Texts.AddOrUpdate(x => x.Id,
                                      new Text() { Value = "Om kitos blablabla", ObjectOwner = globalUser, LastChangedByUser = globalUser },
                                      new Text() { Value = "Status blablabla", ObjectOwner = globalUser, LastChangedByUser = globalUser });

            #endregion

            #region KLE

            var kle = GenerateAllTasks(globalUser, klRootUnit);
            context.TaskRefs.AddRange(kle);

            #endregion
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
        private T CreateOption<T, TReference>(string name, User objectOwner, string note = "...", bool isActive = true, bool isSuggestion = false)
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
        private void AddOptions<T, TReference>(DbSet<T> dbSet, User objectOwner, params string[] names)
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
        private User CreateUser(string name, string email, string password, CryptoService cryptoService, User objectOwner = null)
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

        private Organization CreateOrganization(string name, OrganizationType organizationType, User objectOwner = null)
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
        private void SetUserCreatedOrganization(User user, Organization organization)
        {
            user.CreatedIn = organization;
            user.DefaultOrganizationUnit = organization.GetRoot();
        }
    }
}

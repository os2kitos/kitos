using System;
using System.Collections.Generic;
using System.Data.Entity;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

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
                        IsPublic = true,
                        Parent = parent
                    });
            }

            return result;
        }

        private List<TaskRef> GenerateAllTasks(User objectOwner, OrganizationUnit orgUnitOwner)
        {
            var maingroups = GenerateTasks(objectOwner, orgUnitOwner, "KLE-Hovedgruppe", 5);
            var subgroups =
                maingroups.SelectMany(
                    parent => GenerateTasks(objectOwner, orgUnitOwner, "KLE-Gruppe", 10, parent, parent.TaskKey)).ToList();

            var leafs = subgroups.SelectMany(parent => GenerateTasks(objectOwner, orgUnitOwner, "KLE-Emne", 20, parent, parent.TaskKey)).ToList();

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
            
            AddOptions<ItProjectCategory, ItProject>(context.ProjectCategories, globalUser, "Fællesoffentlig", "Fælleskommunal");

            AddOptions<ItProjectType, ItProject>(context.ProjectTypes, globalUser, "IT Projekt", "IT Program", "Indsatsområde");
            
            AddOptions<AppType, ItSystem>(context.AppTypes, globalUser, "Snitflade", "Fagsystem", "Selvbetjening");

            AddOptions<BusinessType, ItSystem>(context.BusinessTypes, globalUser, "Forretningstype 1", "Forretningstype 2", "Forretningstype 3");

            AddOptions<Interface, ItSystem>(context.Interfaces, globalUser, "Grænseflade 1", "Grænseflade 2", "Grænseflade 3");
            
            AddOptions<Tsa, ItSystem>(context.Tsas, globalUser, "Ja", "Nej");
            
            AddOptions<InterfaceType, ItSystem>(context.InterfaceTypes, globalUser, "WS");

            AddOptions<DataType, DataRow>(context.DataTypes, globalUser, "Datatype 1", "Datatype 2", "Datatype 3");

            AddOptions<Method, ItSystem>(context.Methods, globalUser, "Batch", "Request-Response");

            AddOptions<ContractType, ItContract>(context.ContractTypes, globalUser, "Hovedkontrakt", "Tillægskontrakt", "Snitflade");

            AddOptions<ContractTemplate, ItContract>(context.ContractTemplates, globalUser, "K01", "K02", "K03");

            AddOptions<PurchaseForm, ItContract>(context.PurchaseForms, globalUser, "SKI", "SKI 02.19", "Udbud");

            AddOptions<ProcurementStrategy, ItContract>(context.ProcurementStrategies, globalUser, "Strategi 1", "Strategi 2", "Strategi 3");

            AddOptions<AgreementElement, ItContract>(context.AgreementElements, globalUser, 
                "Licens", "Udvikling", "Drift", "Vedligehold", "Support", 
                "Serverlicenser", "Serverdrift", "Databaselicenser", "Backup", "Overvågning");

            AddOptions<ItSupportModuleName, Config>(context.ItSupportModuleNames, globalUser, "IT Understøttelse", "Organisation");

            AddOptions<ItProjectModuleName, Config>(context.ItProjectModuleNames, globalUser, "IT Projekter", "Projekter");

            AddOptions<ItSystemModuleName, Config>(context.ItSystemModuleNames, globalUser, "IT Systemer", "Systemer");

            AddOptions<ItContractModuleName, Config>(context.ItContractModuleNames, globalUser, "IT Kontrakter", "Kontrakter");

            AddOptions<Frequency, DataRowUsage>(context.Frequencies, globalUser, "Dagligt", "Ugentligt", "Månedligt", "Årligt"); 
            
            AddOptions<ArchiveType, ItSystemUsage>(context.ArchiveTypes, globalUser, "Arkiveret", "Ikke arkiveret");

            AddOptions<SensitiveDataType, ItSystemUsage>(context.SensitiveDataTypes, globalUser, "Ja", "Nej");

            AddOptions<GoalType, Goal>(context.GoalTypes, globalUser, "Måltype 1", "Måltype 2");

            AddOptions<OptionExtend, ItContract>(context.OptionExtention, globalUser, "2 x 1 år");

            AddOptions<TerminationDeadline, ItContract>(context.TerminationDeadlines, globalUser, "Frist 1", "Frist 2");

            AddOptions<PaymentFreqency, ItContract>(context.PaymentFreqencies, globalUser, "Frekvens A", "Frekvens B");

            AddOptions<PaymentModel, ItContract>(context.PaymentModels, globalUser, "Model A", "Model B");

            AddOptions<PriceRegulation, ItContract>(context.PriceRegulations, globalUser, "Pris regulering A", "Pris regulering B"); 

            context.SaveChanges();

            #endregion
            
            #region ADMIN ROLES

            var localAdmin = new AdminRole
            {
                Name = "LocalAdmin",
                IsActive = true,
                ObjectOwner = globalUser
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
                ObjectOwner = globalUser
            };

            var resourcePerson = new OrganizationRole()
            {
                IsActive = true,
                Name = "Ressourceperson",
                Note = "...",
                HasWriteAccess = true,
                ObjectOwner = globalUser
            };

            var employee = new OrganizationRole()
            {
                IsActive = true,
                Name = "Medarbejder",
                Note = "...",
                HasWriteAccess = false,
                ObjectOwner = globalUser
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
                    ObjectOwner = globalUser
                },
                new ItProjectRole()
                {
                    IsActive = true,
                    Name = "Projektleder",
                    ObjectOwner = globalUser
                },
                new ItProjectRole()
                {
                    IsActive = true,
                    Name = "Delprojektleder",
                    ObjectOwner = globalUser
                },
                new ItProjectRole()
                {
                    IsActive = true,
                    Name = "Projektdeltager",
                    ObjectOwner = globalUser
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
                ObjectOwner = globalUser
            };

            var systemRole2 = new ItSystemRole()
            {
                HasReadAccess = true,
                HasWriteAccess = false,
                IsActive = true,
                Name = "Systemrolle 2",
                ObjectOwner = globalUser
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
                ObjectOwner = globalUser
            }, new ItContractRole()
            {
                HasWriteAccess = true,
                Name = "Kontraktrolle B",
                IsActive = true,
                ObjectOwner = globalUser
            }, new ItContractRole()
            {
                HasWriteAccess = false,
                Name = "Kontraktrolle C",
                IsActive = true,
                ObjectOwner = globalUser
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
                                                ObjectOwner = globalUser
                                            },
                                            new AdminRight()
                                            {
                                                Object = kl,
                                                Role = localAdmin,
                                                User = erik,
                                                ObjectOwner = globalUser
                                            },
                                            new AdminRight()
                                            {
                                                Object = roskilde,
                                                Role = localAdmin,
                                                User = localUser,
                                                ObjectOwner = globalUser
                                            });

            context.SaveChanges();

            #endregion

            #region ROSKILDE ORG UNITS

            //LEVEL 0
            var roskildeRoot = roskilde.OrgUnits.First();

            //LEVEL 1
            var munChief = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
                Parent = roskildeRoot,
                Name = "Kommunaldirektøren"
            };
            var wellfare = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
                Parent = roskildeRoot,
                Name = "Velfærd"
            };

            //LEVEL 2
            var digi = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
                Parent = munChief,
                Name = "Digitalisering og Borgerservice"
            };

            var hrcouncil = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
                Parent = munChief,
                Name = "HR og Byråd"
            };

            var elderArea = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
                Parent = wellfare,
                Name = "Ældreområdet"
            };

            //LEVEL 3
            var itservice = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
                Parent = digi,
                Name = "IT Service"
            };
            var projectunit = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
                Parent = digi,
                Name = "Projektenheden"
            };
            var citizenservice = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
                Parent = digi,
                Name = "Borgerservice"
            };
            var hr = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
                Parent = hrcouncil,
                Name = "HR"
            };
            var nursinghome = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
                Parent = elderArea,
                Name = "Plejehjem"
            };

            //LEVEL 4
            var infra = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
                Parent = itservice,
                Name = "Infrastruktur"
            };
            var teamcontact = new OrganizationUnit()
            {
                Organization = roskilde, ObjectOwner = globalUser,
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
                Organization = sorø, ObjectOwner = globalUser,
                Parent = sorøRoot,
                Name = "Direktørområde"
            };

            //LEVEL 2
            var level2a = new OrganizationUnit()
            {
                Organization = sorø, ObjectOwner = globalUser,
                Parent = level1a,
                Name = "Afdeling 1"
            };

            var level2b = new OrganizationUnit()
            {
                Organization = sorø, ObjectOwner = globalUser,
                Parent = level1a,
                Name = "Afdeling 2"
            };

            var level2c = new OrganizationUnit()
            {
                Organization = sorø, ObjectOwner = globalUser,
                Parent = level1a,
                Name = "Afdeling 3"
            };

            //LEVEL 2
            var level3a = new OrganizationUnit()
            {
                Organization = sorø, ObjectOwner = globalUser,
                Parent = level2a,
                Name = "Afdeling 1a"
            };

            var level3b = new OrganizationUnit()
            {
                Organization = sorø, ObjectOwner = globalUser,
                Parent = level2b,
                Name = "Afdeling 2a"
            };

            var level3c = new OrganizationUnit()
            {
                Organization = sorø, ObjectOwner = globalUser,
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
                                      new Text() { Value = "Head", ObjectOwner = globalUser },
                                      new Text() { Value = "Body", ObjectOwner = globalUser });

            #endregion

            #region KLE

            /*var task00 = new TaskRef()
            {
                TaskKey = "00",
                Description = "Kommunens styrelse",
                Type = "KLE-Hovedgruppe",
                IsPublic = true,
                OwnedByOrganizationUnit = klRootUnit,
                ObjectOwner = globalUser
            };
            var task0001 = new TaskRef()
            {
                TaskKey = "00.01",
                Description = "Kommunens styrelse",
                Type = "KLE-Gruppe",
                Parent = task00,
                IsPublic = true,
                OwnedByOrganizationUnit = klRootUnit,
                ObjectOwner = globalUser
            };
            var task0003 = new TaskRef()
            {
                TaskKey = "00.03",
                Description = "International virksomhed og EU",
                Type = "KLE-Gruppe",
                Parent = task00,
                IsPublic = true,
                OwnedByOrganizationUnit = klRootUnit,
                ObjectOwner = globalUser
            };
            context.TaskRefs.AddOrUpdate(x => x.TaskKey,
                                         task00,
                                         task0001,
                                         new TaskRef()
                                         {
                                             TaskKey = "00.01.00",
                                             Description = "Kommunens styrelse i almindelighed",
                                             Type = "KLE-Emne",
                                             Parent = task0001,
                                             IsPublic = true,
                                             OwnedByOrganizationUnit = klRootUnit,
                                             ObjectOwner = globalUser
                                         },
                                         new TaskRef()
                                         {
                                             TaskKey = "00.01.10",
                                             Description = "Opgaver der dækker flere hovedgrupper",
                                             Type = "KLE-Emne",
                                             Parent = task0001,
                                             IsPublic = true,
                                             OwnedByOrganizationUnit = klRootUnit,
                                             ObjectOwner = globalUser
                                         },
                                         task0003,
                                         new TaskRef()
                                         {
                                             TaskKey = "00.03.00",
                                             Description = "International virksomhed og EU i almindelighed",
                                             Type = "KLE-Emne",
                                             Parent = task0003,
                                             IsPublic = true,
                                             OwnedByOrganizationUnit = klRootUnit,
                                             ObjectOwner = globalUser
                                         },
                                         new TaskRef()
                                         {
                                             TaskKey = "00.03.02",
                                             Description = "Internationale organisationers virksomhed",
                                             Type = "KLE-Emne",
                                             Parent = task0003,
                                             IsPublic = true,
                                             OwnedByOrganizationUnit = klRootUnit,
                                             ObjectOwner = globalUser
                                         },
                                         new TaskRef()
                                         {
                                             TaskKey = "00.03.04",
                                             Description = "Regionaludvikling EU",
                                             Type = "KLE-Emne",
                                             Parent = task0003,
                                             IsPublic = true,
                                             OwnedByOrganizationUnit = klRootUnit,
                                             ObjectOwner = globalUser
                                         },
                                         new TaskRef()
                                         {
                                             TaskKey = "00.03.08",
                                             Description = "EU-interessevaretagelse",
                                             Type = "KLE-Emne",
                                             Parent = task0003,
                                             IsPublic = true,
                                             OwnedByOrganizationUnit = klRootUnit,
                                             ObjectOwner = globalUser
                                         },
                                         new TaskRef()
                                         {
                                             TaskKey = "00.03.10",
                                             Description = "Internationalt samarbejde",
                                             Type = "KLE-Emne",
                                             Parent = task0003,
                                             IsPublic = true,
                                             OwnedByOrganizationUnit = klRootUnit,
                                             ObjectOwner = globalUser
                                         });*/

            context.TaskRefs.AddOrUpdate(x => x.TaskKey, GenerateAllTasks(globalUser, klRootUnit).ToArray());

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
                    ObjectOwner = objectOwner
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
                    ObjectOwner = objectOwner
                };
        }

        private Organization CreateOrganization(string name, OrganizationType organizationType, User objectOwner = null)
        {
            var org = new Organization
            {
                Name = name,
                Config = Config.Default(objectOwner),
                Type = organizationType,
                ObjectOwner = objectOwner
            };

            org.OrgUnits.Add(new OrganizationUnit()
            {
                Name = org.Name,
                ObjectOwner = objectOwner
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

        //TODO REMOVE THIS
        private void OldSeed(Infrastructure.DataAccess.KitosContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            var cryptoService = new CryptoService();
            var organizationService = new OrganizationService(null, null, null); // TODO needs a refactor as this is a bit hacky!

            #region AdminRoles

            var localAdmin = new AdminRole { Name = "LocalAdmin", IsActive = true};
            context.AdminRoles.AddOrUpdate(x => x.Name, localAdmin);

            #endregion

            #region Drop Down Data

            var itProjectCategoryPublic = new ItProjectCategory() { IsActive = true, Note = "...", Name = "Fællesoffentlig" };
            var itProjectCategoryMunipalicity = new ItProjectCategory() { IsActive = true, Note = "...", Name = "Fælleskommunal" };
            context.ProjectCategories.AddOrUpdate(x => x.Name, itProjectCategoryPublic, itProjectCategoryMunipalicity);

            var itProjectTypeProject = new ItProjectType() { IsActive = true, Note = "...", Name = "IT Projekt" };
            var itProjectTypeProgram = new ItProjectType() { IsActive = true, Note = "...", Name = "IT Program" };
            context.ProjectTypes.AddOrUpdate(x => x.Name,
                                             itProjectTypeProject,
                                             itProjectTypeProgram,
                                             new ItProjectType() { IsActive = true, Note = "En samlebetegnelse for projekter, som ikke er et IT Program", Name = "Indsatsområde" });

            var appType1 = new AppType() { IsActive = true, Note = "...", Name = "Snitflade" };
            var appType2 = new AppType() { IsActive = true, Note = "...", Name = "Fagsystem" };
            context.AppTypes.AddOrUpdate(x => x.Name,
                                            appType1, appType2,
                                            new AppType() { IsActive = true, Note = "...", Name = "Selvbetjening" }
                                            );

            var businessType1 = new BusinessType() { IsActive = true, Note = "...", Name = "Forretningstype 1" };
            var businessType2 = new BusinessType() { IsActive = true, Note = "...", Name = "Forretningstype 2" };
            context.BusinessTypes.AddOrUpdate(x => x.Name,
                                              businessType1, businessType2);

            context.Interfaces.AddOrUpdate(x => x.Name,
                                           new Interface() { IsActive = true, Note = "...", Name = "Grænseflade 1" },
                                           new Interface() { IsActive = true, Note = "...", Name = "Grænseflade 2" },
                                           new Interface() { IsActive = true, Note = "...", Name = "Grænseflade 3" });

            context.Tsas.AddOrUpdate(x => x.Name,
                                     new Tsa() {IsActive = true, Note = "...", Name = "Ja"},
                                     new Tsa() {IsActive = true, Note = "...", Name = "Nej"});

            context.InterfaceTypes.AddOrUpdate(x => x.Name,
                                               new InterfaceType() { IsActive = true, Note = "...", Name = "WS" });

            var dataType = new DataType {IsActive = true, Note = "...", Name = "Datatype 1"};
            context.DataTypes.AddOrUpdate(x => x.Name,
                                          dataType,
                                          new DataType() {IsActive = true, Note = "...", Name = "Datatype 2"},
                                          new DataType() {IsActive = true, Note = "...", Name = "Datatype 3"});

            context.Methods.AddOrUpdate(x => x.Name,
                                        new Method() { IsActive = true, Note = "...", Name = "Batch" },
                                        new Method() { IsActive = true, Note = "...", Name = "Request-Response" });

            /*
            context.DatabaseTypes.AddOrUpdate(x => x.Name,
                                              new DatabaseType() { IsActive = true, Note = "...", Name = "MSSQL" },
                                              new DatabaseType() { IsActive = true, Note = "...", Name = "MySQL" });

            context.Environments.AddOrUpdate(x => x.Name,
                                             new Core.DomainModel.ItSystem.Environment()
                                             {
                                                 IsActive = true,
                                                 Note = "...",
                                                 Name = "Citrix"
                                             });*/

            var contractTypeMain = new ContractType() {IsActive = true, Note = "...", Name = "Hovedkontrakt"};
            var contractTypeSupplementary = new ContractType()
                {
                    IsActive = true, Note = "...", Name = "Tilægskontrakt"
                };
            var contractTypeInterface = new ContractType() {IsActive = true, Note = "...", Name = "Snitflade"};
            context.ContractTypes.AddOrUpdate(x => x.Name,
                                              contractTypeMain,
                                              contractTypeSupplementary,
                                              contractTypeInterface,
                                              new ContractType() { IsActive = false, Note = "...", Name = "Tidligere aktiv kontrakttype" },
                                              new ContractType() { IsSuggestion = true, Note = "...", Name = "Forslag1" },
                                              new ContractType() { IsSuggestion = true, Note = "...", Name = "Forslag2" });

            var contractTemplateK1 = new ContractTemplate() {IsActive = true, Note = "...", Name = "K01"};
            var contractTemplateK2 = new ContractTemplate() {IsActive = true, Note = "...", Name = "K02"};
            var contractTemplateK3 = new ContractTemplate() {IsActive = true, Note = "...", Name = "K03"};
            context.ContractTemplates.AddOrUpdate(x => x.Name,
                                                  contractTemplateK1,
                                                  contractTemplateK2,
                                                  contractTemplateK3);

            var purchaseForm1 = new PurchaseForm() {IsActive = true, Note = "...", Name = "SKI"};
            var purchaseForm2 = new PurchaseForm() {IsActive = true, Note = "...", Name = "SKI 02.19"};
            var purchaseForm3 = new PurchaseForm() {IsActive = true, Note = "...", Name = "Udbud"};
            context.PurchaseForms.AddOrUpdate(x => x.Name,
                                              purchaseForm1,
                                              purchaseForm2,
                                              purchaseForm3);

            var procurementStrategy1 = new ProcurementStrategy() { IsActive = true, Name = "Strategi 1" };
            var procurementStrategy2 = new ProcurementStrategy() { IsActive = true, Name = "Strategi 2" };
            var procurementStrategy3 = new ProcurementStrategy() { IsActive = true, Name = "Strategi 3" };
            context.ProcurementStrategies.AddOrUpdate(x => x.Name, procurementStrategy1, procurementStrategy2, procurementStrategy3);

            context.AgreementElements.AddOrUpdate(x => x.Name, 
                new AgreementElement(){ IsActive = true, Name = "Licens" },
                new AgreementElement(){ IsActive = true, Name = "Udvikling" },
                new AgreementElement(){ IsActive = true, Name = "Drift" },
                new AgreementElement(){ IsActive = true, Name = "Vedligehold" },
                new AgreementElement(){ IsActive = true, Name = "Support" },
                new AgreementElement(){ IsActive = true, Name = "Serverlicenser" },
                new AgreementElement(){ IsActive = true, Name = "Serverdrift" },
                new AgreementElement(){ IsActive = true, Name = "Databaselicenser" },
                new AgreementElement(){ IsActive = true, Name = "Backup" },
                new AgreementElement(){ IsActive = true, Name = "Overvågning" });

            var itSupportName = new ItSupportModuleName() { Name = "IT Understøttelse" };
            context.ItSupportModuleNames.AddOrUpdate(x => x.Name,
                new ItSupportModuleName() { Name = "IT Understøttelse af organisation" },
                itSupportName,
                new ItSupportModuleName() { Name = "Organisation" });

            var itProjectName = new ItProjectModuleName() { Name = "IT Projekter" };
            context.ItProjectModuleNames.AddOrUpdate(x => x.Name,
                itProjectName,
                new ItProjectModuleName() { Name = "Projekter" });

            var itSystemName = new ItSystemModuleName() { Name = "IT Systemer" };
            context.ItSystemModuleNames.AddOrUpdate(x => x.Name,
                itSystemName,
                new ItSystemModuleName() { Name = "Systemer" });

            var itContractName = new ItContractModuleName() { Name = "IT Kontrakter" };
            context.ItContractModuleNames.AddOrUpdate(x => x.Name,
                itContractName,
                new ItContractModuleName() { Name = "Kontrakter" });

            var frequency1 = new Frequency() { IsActive = true, Note = "...", Name = "Dagligt" };
            var frequency2 = new Frequency() { IsActive = true, Note = "...", Name = "Ugentligt" };
            var frequency3 = new Frequency() { IsActive = true, Note = "...", Name = "Månedligt" };
            var frequency4 = new Frequency() { IsActive = true, Note = "...", Name = "Årligt" };

            context.Frequencies.AddOrUpdate(x => x.Name, frequency1, frequency2, frequency3, frequency4);
            
            context.SaveChanges();


            var archiveTypeYes = new ArchiveType()
                {
                    IsActive = true,
                    Name = "Arkiveret",
                };
            var archiveTypeNo = new ArchiveType()
                {
                    IsActive = true,
                    Name = "Ikke arkiveret",
                };
            context.ArchiveTypes.AddOrUpdate(x => x.Name, archiveTypeYes, archiveTypeNo);

            var sensitiveDataYes = new SensitiveDataType()
                {
                    IsActive = true,
                    Name = "Ja"
                };
            var sensitiveDataNo = new SensitiveDataType()
            {
                IsActive = true,
                Name = "Nej"
            };
            context.SensitiveDataTypes.AddOrUpdate(x => x.Name, sensitiveDataYes, sensitiveDataNo);


            context.GoalTypes.AddOrUpdate(x => x.Name, new GoalType()
                {
                    IsActive = true,
                    Name = "Måltype 1"
                }, new GoalType()
                {
                    IsActive = true,
                    Name = "Måltype 2"
                });

            context.OptionExtention.AddOrUpdate(x => x.Name,
                                                new OptionExtend() {IsActive = true, Name = "Option A"},
                                                new OptionExtend() {IsActive = true, Name = "Option B"},
                                                new OptionExtend() {IsActive = true, Name = "Option C"});

            context.TerminationDeadlines.AddOrUpdate(x => x.Name,
                                                     new TerminationDeadline() {IsActive = true, Name = "Frist A"},
                                                     new TerminationDeadline() {IsActive = true, Name = "Frist B"},
                                                     new TerminationDeadline() {IsActive = true, Name = "Frist C"});

            context.PaymentFreqencies.AddOrUpdate(x => x.Name,
                                                  new PaymentFreqency() {IsActive = true, Name = "Frekvens A"},
                                                  new PaymentFreqency() {IsActive = true, Name = "Frekvens B"},
                                                  new PaymentFreqency() {IsActive = true, Name = "Frekvens C"});

            context.PaymentModels.AddOrUpdate(x => x.Name,
                                              new PaymentModel() {IsActive = true, Name = "Model A"},
                                              new PaymentModel() {IsActive = true, Name = "Model B"},
                                              new PaymentModel() {IsActive = true, Name = "Model C"});

            context.PriceRegulations.AddOrUpdate(x => x.Name,
                                                 new PriceRegulation() {IsActive = true, Name = "Pris regulering A"},
                                                 new PriceRegulation() {IsActive = true, Name = "Pris regulering B"},
                                                 new PriceRegulation() {IsActive = true, Name = "Pris regulering C"});

            context.SaveChanges();
            
            #endregion

            #region Organizations

            var roskilde = CreateOrganization("Roskilde", OrganizationType.Municipality, null);
            var sorø = CreateOrganization("Sorø", OrganizationType.Municipality, null);
            var kl = CreateOrganization("KL", OrganizationType.Municipality, null);
            var companyA = CreateOrganization("Firma A", OrganizationType.Company, null);
            var companyB = CreateOrganization("Firma B", OrganizationType.Company, null);
            var companyC = CreateOrganization("Firma C", OrganizationType.Company, null);

            context.Organizations.AddOrUpdate(x => x.Name, roskilde, sorø, kl, companyA, companyB, companyC);

            context.SaveChanges();

            #endregion

            #region Roskilde OrgUnits

            //LEVEL 0
            var roskildeRoot = roskilde.OrgUnits.First();

            //LEVEL 1
            var munChief = new OrganizationUnit()
                {
                    Organization = roskilde,
                    Parent = roskildeRoot,
                    Name = "Kommunaldirektøren"
                };
            var wellfare = new OrganizationUnit()
            {
                Organization = roskilde,
                Parent = roskildeRoot,
                Name = "Velfærd"
            };

            //LEVEL 2
            var digi = new OrganizationUnit()
                {
                    Organization = roskilde,
                    Parent = munChief,
                    Name = "Digitalisering og Borgerservice"
                };

            var hrcouncil = new OrganizationUnit()
                {
                    Organization = roskilde,
                    Parent = munChief,
                    Name = "HR og Byråd"
                };

            var elderArea = new OrganizationUnit()
            {
                Organization = roskilde,
                Parent = wellfare,
                Name = "Ældreområdet"
            };

            //LEVEL 3
            var itservice = new OrganizationUnit()
            {
                Organization = roskilde,
                Parent = digi,
                Name = "IT Service"
            };
            var projectunit = new OrganizationUnit()
            {
                Organization = roskilde,
                Parent = digi,
                Name = "Projektenheden"
            };
            var citizenservice = new OrganizationUnit()
            {
                Organization = roskilde,
                Parent = digi,
                Name = "Borgerservice"
            };
            var hr = new OrganizationUnit()
            {
                Organization = roskilde,
                Parent = hrcouncil,
                Name = "HR"
            };
            var nursinghome = new OrganizationUnit()
            {
                Organization = roskilde,
                Parent = elderArea,
                Name = "Plejehjem"
            };

            //LEVEL 4
            var infra = new OrganizationUnit()
            {
                Organization = roskilde,
                Parent = itservice,
                Name = "Infrastruktur"
            };
            var teamcontact = new OrganizationUnit()
            {
                Organization = roskilde,
                Parent = citizenservice,
                Name = "Team Kontaktcenter"
            };

            context.OrganizationUnits.AddOrUpdate(o => o.Name, munChief, wellfare, digi, hrcouncil, elderArea, itservice, projectunit, citizenservice, hr, nursinghome, infra, teamcontact);
            context.SaveChanges();

            #endregion

            #region Sorø OrgUnits

            //LEVEL 0
            var sorøRoot = sorø.OrgUnits.First();

            //LEVEL 1
            var level1a = new OrganizationUnit()
            {
                Organization = sorø,
                Parent = sorøRoot,
                Name = "Direktørområde"
            };

            //LEVEL 2
            var level2a = new OrganizationUnit()
            {
                Organization = sorø,
                Parent = level1a,
                Name = "Afdeling 1"
            };

            var level2b = new OrganizationUnit()
            {
                Organization = sorø,
                Parent = level1a,
                Name = "Afdeling 2"
            };

            var level2c = new OrganizationUnit()
            {
                Organization = sorø,
                Parent = level1a,
                Name = "Afdeling 3"
            };

            //LEVEL 2
            var level3a = new OrganizationUnit()
            {
                Organization = sorø,
                Parent = level2a,
                Name = "Afdeling 1a"
            };

            var level3b = new OrganizationUnit()
            {
                Organization = sorø,
                Parent = level2b,
                Name = "Afdeling 2a"
            };

            var level3c = new OrganizationUnit()
            {
                Organization = sorø,
                Parent = level2b,
                Name = "Afdeling 2b"
            };

            context.OrganizationUnits.AddOrUpdate(o => o.Name, level1a, level2a, level2b, level2c, level3a, level3b,
                                                  level3c);
            context.SaveChanges();

            #endregion

            #region KL OrgUnits

            var klRootUnit = kl.OrgUnits.First();

            #endregion

            #region OrganizationUnit roles

            var boss = new OrganizationRole()
                {
                    IsActive = true,
                    Name = "Chef",
                    Note = "Lederen af en organisationsenhed",
                    HasWriteAccess = true
                };

            var resourcePerson = new OrganizationRole()
                {
                    IsActive = true,
                    Name = "Ressourceperson",
                    Note = "...",
                    HasWriteAccess = true
                };

            var employee = new OrganizationRole()
                {
                    IsActive = true,
                    Name = "Medarbejder",
                    Note = "...",
                    HasWriteAccess = false
                };

            context.OrganizationRoles.AddOrUpdate(role => role.Id,
                                                boss, resourcePerson, employee,
                                                new OrganizationRole() { IsActive = false, Name = "Inaktiv Org rolle" },
                                                new OrganizationRole()
                                                    {
                                                        IsActive = false,
                                                        IsSuggestion = true,
                                                        Name = "Forslag til org rolle"
                                                    }
                );

            #endregion

            #region Project roles

            context.ItProjectRoles.AddOrUpdate(r => r.Id,
                                               new ItProjectRole() { IsActive = true, Name = "Projektejer" },
                                               new ItProjectRole() { IsActive = true, Name = "Projektleder" },
                                               new ItProjectRole() { IsActive = true, Name = "Delprojektleder" },
                                               new ItProjectRole() { IsActive = true, Name = "Projektdeltager" },
                                               new ItProjectRole() { IsActive = false, Name = "Inaktiv projektrolle" },
                                               new ItProjectRole()
                                                   {
                                                       IsActive = false,
                                                       IsSuggestion = true,
                                                       Name = "Foreslået projektrolle"
                                                   }
                );

            #endregion

            #region Users

            var simon = CreateUser("Simon Lynn-Pedersen", "slp@it-minds.dk", "slp123", cryptoService);
            simon.IsGlobalAdmin = true;

            var eskild = CreateUser("Eskild", "esd@it-minds.dk", "123", cryptoService);
            eskild.IsGlobalAdmin = true;

            var globalUser = CreateUser("Global Test Bruger", "g@test", "test", cryptoService);
            globalUser.IsGlobalAdmin = true;

            var localUser = CreateUser("Local Test Bruger", "l@test", "test", cryptoService);

            var roskildeUser1 = CreateUser("Pia", "pia@it-minds.dk", "arne123", cryptoService);
            var roskildeUser2 = CreateUser("Morten", "morten@it-minds.dk", "arne123", cryptoService);
            var roskildeUser3 = CreateUser("Anders", "anders@it-minds.dk", "arne123", cryptoService);
            var roskildeUser4 = CreateUser("Peter", "peter@it-minds.dk", "arne123", cryptoService);
            var roskildeUser5 = CreateUser("Jesper", "jesper@it-minds.dk", "arne123", cryptoService);
            var roskildeUser6 = CreateUser("Brian", "briana@roskilde.dk", "123", cryptoService);
            roskildeUser6.IsGlobalAdmin = true;
            var roskildeUser7 = CreateUser("Erik", "ehl@kl.dk", "123", cryptoService);

            context.Users.AddOrUpdate(x => x.Email, simon, eskild, globalUser, localUser, roskildeUser1, roskildeUser2, roskildeUser3, roskildeUser4, roskildeUser5, roskildeUser6, roskildeUser7);

            context.SaveChanges();

            #endregion

            #region Admin rights

            context.AdminRights.AddOrUpdate(right => new {right.ObjectId, right.RoleId, right.UserId},
                                            new AdminRight()
                                                {
                                                    Object = roskilde,
                                                    Role = localAdmin,
                                                    User = roskildeUser1
                                                },
                                            new AdminRight()
                                                {
                                                    Object = kl,
                                                    Role = localAdmin,
                                                    User = roskildeUser7
                                                },
                                            new AdminRight()
                                                {
                                                    Object = roskilde,
                                                    Role = localAdmin,
                                                    User = localUser
                                                });

            context.SaveChanges();

            #endregion

            #region Org rights

            context.OrganizationRights.AddOrUpdate(right => new { right.ObjectId, right.RoleId, right.UserId },
                    new OrganizationRight()
                        {
                            Object = itservice,
                            Role = resourcePerson,
                            User = roskildeUser1
                        },
                    new OrganizationRight()
                        {
                            Object = digi,
                            Role = boss,
                            User = roskildeUser2
                        },
                    new OrganizationRight()
                    {
                        Object = hr,
                        Role = resourcePerson,
                        User = roskildeUser2
                    },
                    new OrganizationRight()
                    {
                        Object = teamcontact,
                        Role = resourcePerson,
                        User = roskildeUser4
                    }
                );

            context.SaveChanges();

            #endregion

            #region Password Reset Requests

            /*
            var simonId = context.Users.Single(x => x.Email == "slp@it-minds.dk").Id;

            context.PasswordResetRequests.AddOrUpdate(x => x.Id,
                                                      new PasswordResetRequest
                                                      {
                                                          //This reset request is fine
                                                          Id = "workingRequest", //ofcourse, this should be a hashed string or something obscure
                                                          Time = DateTime.Now.AddYears(+20), //.MaxValue also seems to be out-of-range, but this should hopefully be good enough
                                                          UserId = simonId
                                                      }
                );

             */
            #endregion

            #region Texts

            context.Texts.AddOrUpdate(x => x.Id,
                                      new Text() { Value = "Head" },
                                      new Text() { Value = "Body" });

            #endregion

            #region KLE

            var task00 = new TaskRef()
                {
                    TaskKey = "00", Description = "Kommunens styrelse", Type = "KLE-Hovedgruppe", IsPublic = true, OwnedByOrganizationUnit = klRootUnit
                };
            var task0001 = new TaskRef()
                {
                    TaskKey = "00.01",
                    Description = "Kommunens styrelse",
                    Type = "KLE-Gruppe",
                    Parent = task00,
                    IsPublic = true,
                    OwnedByOrganizationUnit = klRootUnit
                };
            var task0003 = new TaskRef()
                {
                    TaskKey = "00.03",
                    Description = "International virksomhed og EU",
                    Type = "KLE-Gruppe",
                    Parent = task00,
                    IsPublic = true,
                    OwnedByOrganizationUnit = klRootUnit
                };
            context.TaskRefs.AddOrUpdate(x => x.TaskKey,
                                         task00,
                                         task0001,
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.01.00",
                                                 Description = "Kommunens styrelse i almindelighed",
                                                 Type = "KLE-Emne",
                                                 Parent = task0001,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.01.10",
                                                 Description = "Opgaver der dækker flere hovedgrupper",
                                                 Type = "KLE-Emne",
                                                 Parent = task0001,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         task0003,
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.03.00",
                                                 Description = "International virksomhed og EU i almindelighed",
                                                 Type = "KLE-Emne",
                                                 Parent = task0003,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.03.02",
                                                 Description = "Internationale organisationers virksomhed",
                                                 Type = "KLE-Emne",
                                                 Parent = task0003,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.03.04",
                                                 Description = "Regionaludvikling EU",
                                                 Type = "KLE-Emne",
                                                 Parent = task0003,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.03.08",
                                                 Description = "EU-interessevaretagelse",
                                                 Type = "KLE-Emne",
                                                 Parent = task0003,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.03.10",
                                                 Description = "Internationalt samarbejde",
                                                 Type = "KLE-Emne",
                                                 Parent = task0003,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             });

            #endregion

            #region IT systems

            var system1 = new ItSystem()
                {
                    AccessModifier = AccessModifier.Public,
                    AppType = appType2,
                    BusinessType = businessType1,
                    Organization = kl,
                    BelongsTo = kl,
                    ObjectOwner = simon,
                    Version = "7.0",
                    Name = "TM Sund",
                    SystemId = "TMSUND1",
                    Description = "TM Sund er en ...",
                    Url = "http://kitos.dk",
                    Parent = null
                };

            var system2 = new ItSystem()
                {
                    AccessModifier = AccessModifier.Public,
                    AppType = appType1,
                    BusinessType = businessType1,
                    Organization = roskilde,
                    BelongsTo = kl,
                    ObjectOwner = roskildeUser1,
                    Version = "0.1",
                    Name = "Vækstkurver",
                    SystemId = "VÆKST",
                    Description = "Snitflade for ...",
                    Url = "http://kitos.dk",
                    Parent = null,
                    ExposedBy = system1,
                    DataRows = new List<DataRow>()
                        {
                            new DataRow(){Data = "Højde på barn", DataType = dataType}
                        },
                    MethodId = 1,
                    InterfaceId = 1,
                    InterfaceTypeId = 1,
                    TsaId = 1
                };

            var system3 = new ItSystem()
                {
                    AccessModifier = AccessModifier.Public,
                    AppType = appType2,
                    BusinessType = businessType1,
                    Organization = kl,
                    BelongsTo = kl,
                    ObjectOwner = simon,
                    Version = "1.0",
                    Name = "A",
                    SystemId = "Root",
                    Description = "...",
                    Url = "http://kitos.dk",
                    Parent = null
                };
            var system31 = new ItSystem()
            {
                AccessModifier = AccessModifier.Public,
                AppType = appType2,
                BusinessType = businessType1,
                Organization = kl,
                BelongsTo = kl,
                ObjectOwner = simon,
                Version = "1.0",
                Name = "AA",
                SystemId = "Barn til root",
                Description = "...",
                Url = "http://kitos.dk",
                Parent = system3
            };
            var system32 = new ItSystem()
            {
                AccessModifier = AccessModifier.Public,
                AppType = appType2,
                BusinessType = businessType1,
                Organization = kl,
                BelongsTo = kl,
                ObjectOwner = simon,
                Version = "1.0",
                Name = "AB",
                SystemId = "Barn til root",
                Description = "...",
                Url = "http://kitos.dk",
                Parent = system3
            };
            var system311 = new ItSystem()
            {
                AccessModifier = AccessModifier.Public,
                AppType = appType2,
                BusinessType = businessType1,
                Organization = kl,
                BelongsTo = kl,
                ObjectOwner = simon,
                Version = "1.0",
                Name = "AAA",
                SystemId = "Barn til AA",
                Description = "...",
                Url = "http://kitos.dk",
                Parent = system31
            };
            var system3111 = new ItSystem()
            {
                AccessModifier = AccessModifier.Public,
                AppType = appType2,
                BusinessType = businessType1,
                Organization = kl,
                BelongsTo = kl,
                ObjectOwner = simon,
                Version = "1.0",
                Name = "AAAA",
                SystemId = "Barn til AAA",
                Description = "...",
                Url = "http://kitos.dk",
                Parent = system311
            };

            context.ItSystems.AddOrUpdate(x => x.Name, system1, system2, system3, system31, system311, system3111, system32);

            #endregion

            #region IT System Usage

            /* TODO: this should be created through ItSystemUsageService to ensure 
             * that the local usages or exposure of interfaces are created.
             * For now, it's better not to create any usage.
            var systemUsage1 = new ItSystemUsage()
                {
                    ArchiveType = archiveTypeNo,
                    SensitiveDataType = sensitiveDataNo,
                    AdOrIdmRef = "ad",
                    CmdbRef = "cmdb",
                    EsdhRef = "esdh",
                    DirectoryOrUrlRef = "x:/foo/bar",
                    Note = "note...",
                    ItSystem = system1,
                    IsStatusActive = true,
                    Organization = roskilde,
                    ObjectOwner = simon
                };

            context.ItSystemUsages.AddOrUpdate(x => x.Note, systemUsage1); // TODO probably not the best identifier
             * 
             */

            #endregion

            #region IT System roles

            var systemRole1 = new ItSystemRole()
                {
                    HasReadAccess = true,
                    HasWriteAccess = true,
                    IsActive = true,
                    Name = "Systemrolle 1"
                };

            var systemRole2 = new ItSystemRole()
                {
                    HasReadAccess = true,
                    HasWriteAccess = false,
                    IsActive = true,
                    Name = "Systemrolle 2"
                };

            context.ItSystemRoles.AddOrUpdate(x => x.Name, systemRole1, systemRole2);
            context.SaveChanges();

            #endregion

            #region IT system rights

            /*
            var sysRight1 = new ItSystemRight()
                {
                    Object = systemUsage1,
                    Role = systemRole1,
                    User = roskildeUser1
                };

            var sysRight2 = new ItSystemRight()
                {
                    Object = systemUsage1,
                    Role = systemRole2,
                    User = roskildeUser2
                };

            context.ItSystemRights.AddOrUpdate(x => x.UserId, sysRight1, sysRight2);
            context.SaveChanges();
             * */

            #endregion

            #region Wishes

            /*
            var wish1 = new Wish()
                {
                    IsPublic = true,
                    Text = "Public test ønske",
                    User = globalUser,
                    ItSystemUsage = systemUsage1
                };
            var wish2 = new Wish()
            {
                IsPublic = false,
                Text = "Ikke public test ønske",
                User = globalUser,
                ItSystemUsage = systemUsage1
            };

            context.Wishes.AddOrUpdate(x => x.Text, wish1, wish2); // TODO probably not the best identifier
            */

            #endregion

            #region IT Project

            var phase11 = new Activity()
                {
                    Name = "Afventer",
                    ObjectOwner = globalUser,
                    StartDate = DateTime.Now.AddDays(76),
                    EndDate = DateTime.Now.AddDays(102)
                };
            var phase12 = new Activity()
                {
                    Name = "Afventer",
                    ObjectOwner = simon,
                    StartDate = DateTime.Now.AddDays(76),
                    EndDate = DateTime.Now.AddDays(102)
                };
            var phase13 = new Activity()
                {
                    Name = "Afventer",
                    ObjectOwner = globalUser,
                    StartDate = DateTime.Now.AddDays(76),
                    EndDate = DateTime.Now.AddDays(102)
                };
            context.Activities.AddOrUpdate(x => x.Id, phase11, phase12, phase13);
            
            context.SaveChanges();

            #endregion

            #region IT Contract roles

            context.ItContractRoles.AddOrUpdate(x => x.Name, new ItContractRole()
                {
                    HasWriteAccess = true,
                    Name = "Kontraktrolle A",
                    IsActive = true
                }, new ItContractRole(){
                    HasWriteAccess = true,
                    Name = "Kontraktrolle B",
                    IsActive = true
                }, new ItContractRole()
                {
                    HasWriteAccess = false,
                    Name = "Kontraktrolle C",
                    IsActive = true
                });
            context.SaveChanges();

            #endregion

            #region IT Contract

            var itContractA = new ItContract()
                {
                    ObjectOwner = globalUser,
                    Name = "Test kontrakt",
                    Note = "En bemærkning!",
                    ItContractId = "Et id",
                    Esdh = "esdh",
                    Folder = "mappe",
                    SupplierContractSigner = "ext underskriver",
                    ContractType = contractTypeMain,
                    PurchaseForm = purchaseForm1,
                    ProcurementStrategy = procurementStrategy1,
                    ContractTemplate = contractTemplateK1,
                    Organization = roskilde,
                    ProcurementPlanHalf = 1, 
                    ProcurementPlanYear = 2016
                };

            var itContractRoot = new ItContract() { ObjectOwner = globalUser, Organization = roskilde, Name = "root" };
            var itContractNode1 = new ItContract() { ObjectOwner = globalUser, Organization = roskilde, Name = "node 1", Parent = itContractRoot };
            var itContractNode2 = new ItContract() { ObjectOwner = globalUser, Organization = roskilde, Name = "node 2", Parent = itContractRoot };
            var itContractNode11 = new ItContract() { ObjectOwner = globalUser, Organization = roskilde, Name = "node 11", Parent = itContractNode1 };
            var itContractNode12 = new ItContract() { ObjectOwner = globalUser, Organization = roskilde, Name = "node 12", Parent = itContractNode1 };
            var itContractNode111 = new ItContract() { ObjectOwner = globalUser, Organization = roskilde, Name = "node 111", Parent = itContractNode11 };
            var itContractNode121 = new ItContract() { ObjectOwner = globalUser, Organization = roskilde, Name = "node 121", Parent = itContractNode12 };

            context.ItContracts.AddOrUpdate(x => x.Name, itContractA, itContractNode1, itContractNode11, itContractNode111, itContractNode12, itContractNode121, itContractNode2);

            #endregion
            
            base.Seed(context);
        }
        
    }
}

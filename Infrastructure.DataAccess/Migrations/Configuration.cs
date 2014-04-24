using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity;
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

        protected override void Seed(Infrastructure.DataAccess.KitosContext context)
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
            var municipalityService = new OrganizationService();

            #region AdminRoles

            var localAdmin = new AdminRole { Name = "LocalAdmin", IsActive = true};
            context.AdminRoles.AddOrUpdate(x => x.Name, localAdmin);

            #endregion

            #region Drop Down Data

            context.ProjectCategories.AddOrUpdate(x => x.Name,
                                             new ProjectCategory() { IsActive = true, Note = "...", Name = "Light" },
                                             new ProjectCategory() { IsActive = true, Note = "...", Name = "Lokal" },
                                             new ProjectCategory() { IsActive = true, Note = "...", Name = "Tværkommunalt" },
                                             new ProjectCategory() { IsActive = true, Note = "...", Name = "SKAL" });

            context.ProjectTypes.AddOrUpdate(x => x.Name,
                                             new ProjectType() { IsActive = true, Note = "...", Name = "IT Projekt" },
                                             new ProjectType() { IsActive = true, Note = "...", Name = "IT Program" },
                                             new ProjectType() { IsActive = true, Note = "En samlebetegnelse for projekter, som ikke er et IT Program", Name = "Indsatsområde" });

            var appType1 = new AppType() {IsActive = true, Note = "...", Name = "Snitflade"};
            var appType2 = new AppType() {IsActive = true, Note = "...", Name = "Fagsystem"};
            context.AppTypes.AddOrUpdate(x => x.Name,
                                            appType1, appType2,
                                            new AppType() { IsActive = true, Note = "...", Name = "Selvbetjening" }
                                            );

            var businessType1 = new BusinessType() { IsActive = true, Note = "...", Name = "Forretningstype 1" };
            var businessType2 = new BusinessType() { IsActive = true, Note = "...", Name = "Forretningstype 2" };
            context.BusinessTypes.AddOrUpdate(x => x.Name,
                                              businessType1, businessType2);

            context.Interfaces.AddOrUpdate(x => x.Name,
                                           new Interface() {IsActive = true, Note = "...", Name = "Grænseflade 1"},
                                           new Interface() {IsActive = true, Note = "...", Name = "Grænseflade 2"},
                                           new Interface() { IsActive = true, Note = "...", Name = "Grænseflade 3" });

            context.Tsas.AddOrUpdate(x => x.Name,
                                     new Tsa() {IsActive = true, Note = "...", Name = "Ja"},
                                     new Tsa() {IsActive = true, Note = "...", Name = "Nej"});

            context.InterfaceTypes.AddOrUpdate(x => x.Name,
                                               new InterfaceType() { IsActive = true, Note = "...", Name = "WS" });

            context.DataTypes.AddOrUpdate(x => x.Name,
                                          new DataType() {IsActive = true, Note = "...", Name = "Datatype 1"},
                                          new DataType() {IsActive = true, Note = "...", Name = "Datatype 2"},
                                          new DataType() {IsActive = true, Note = "...", Name = "Datatype 3"});

            context.ProtocolTypes.AddOrUpdate(x => x.Name,
                                              new ProtocolType() { IsActive = true, Note = "...", Name = "OIORES" },
                                              new ProtocolType() { IsActive = true, Note = "...", Name = "WS SOAP" });

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

            context.ContractTypes.AddOrUpdate(x => x.Name,
                                              new ContractType() { IsActive = true, Note = "...", Name = "Hovedkontrakt" },
                                              new ContractType()
                                              {
                                                  IsActive = true,
                                                  Note = "...",
                                                  Name = "Tilægskontrakt"
                                              },
                                              new ContractType() { IsActive = true, Note = "...", Name = "Snitflade" },
                                              new ContractType() { IsActive = false, Note = "...", Name = "Tidligere aktiv kontrakttype" },
                                              new ContractType() { IsSuggestion = true, Note = "...", Name = "Forslag1" },
                                              new ContractType() { IsSuggestion = true, Note = "...", Name = "Forslag2" });

            context.ContractTemplates.AddOrUpdate(x => x.Name,
                                                  new ContractTemplate() { IsActive = true, Note = "...", Name = "K01" },
                                                  new ContractTemplate() { IsActive = true, Note = "...", Name = "K02" },
                                                  new ContractTemplate() { IsActive = true, Note = "...", Name = "K03" });

            context.PurchaseForms.AddOrUpdate(x => x.Name,
                                              new PurchaseForm() { IsActive = true, Note = "...", Name = "SKI" },
                                              new PurchaseForm() { IsActive = true, Note = "...", Name = "SKI 02.19" },
                                              new PurchaseForm() { IsActive = true, Note = "...", Name = "Udbud" });

            context.PaymentModels.AddOrUpdate(x => x.Name,
                                              new PaymentModel() { IsActive = true, Note = "...", Name = "Licens" });

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

            context.SaveChanges();


            var extRef1 = new ExtReferenceType()
                            {
                                IsActive = true,
                                Name = "ESDH Ref",
                                Note = "Ref. til ESDH system, hvor der er projektdokumenter"
                            };
            var extRef2 = new ExtReferenceType()
                {
                    IsActive = true,
                    Name = "CMDB Ref",
                    Note = "Ref. til CMDB o.l system, hvor der er projektdokumenter"
                };
            var extRef3 = new ExtReferenceType()
                {
                    IsActive = true,
                    Name = "Mappe Ref",
                    Note = "Ref. til andre steder, hvor der er projektdokumenter"
                };
            context.ExtReferenceTypes.AddOrUpdate(x => x.Name, extRef1, extRef2, extRef3);

            var projPhase1 = new ProjectPhase()
                {
                    IsActive = true,
                    Name = "Afventer",
                    Note = "..."
                };

            context.ProjectPhases.AddOrUpdate(x => x.Name, projPhase1,
                new ProjectPhase()
                    {
                        IsActive = true,
                        Name = "Foranalyse",
                        Note = "..."
                    },
                new ProjectPhase()
                    {
                        IsActive = true,
                        Name = "Gennemførsel",
                        Note = "..."
                    },
                new ProjectPhase()
                    {
                        IsActive = true,
                        Name = "Overlevering",
                        Note = "..."
                    },
                new ProjectPhase()
                    {
                        IsActive = true,
                        Name = "Drift",
                        Note = "..."
                    }
                );

            #endregion

            #region Organizations

            var roskilde = municipalityService.CreateMunicipality("Roskilde");
            var sorø = municipalityService.CreateMunicipality("Sorø");
            var kl = municipalityService.CreateMunicipality("KL");

            context.Organizations.AddOrUpdate(x => x.Name, roskilde, sorø, kl);

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

            var simon = SimpleUser("Simon Lynn-Pedersen", "slp@it-minds.dk", "slp123", cryptoService);
            simon.IsGlobalAdmin = true;

            var globalUser = SimpleUser("Global Test Bruger", "g@test", "123", cryptoService);
            globalUser.IsGlobalAdmin = true;

            var localUser = SimpleUser("Local Test Bruger", "l@test", "123", cryptoService);

            var roskildeUser1 = SimpleUser("Pia", "pia@it-minds.dk", "arne123", cryptoService);
            var roskildeUser2 = SimpleUser("Morten", "morten@it-minds.dk", "arne123", cryptoService);
            var roskildeUser3 = SimpleUser("Anders", "anders@it-minds.dk", "arne123", cryptoService);
            var roskildeUser4 = SimpleUser("Peter", "peter@it-minds.dk", "arne123", cryptoService);
            var roskildeUser5 = SimpleUser("Jesper", "jesper@it-minds.dk", "arne123", cryptoService);
            var roskildeUser6 = SimpleUser("Brian", "briana@roskilde.dk", "123", cryptoService);
            roskildeUser6.IsGlobalAdmin = true;
            var roskildeUser7 = SimpleUser("Erik", "ehl@kl.dk", "123", cryptoService);

            context.Users.AddOrUpdate(x => x.Email, simon, globalUser, localUser, roskildeUser1, roskildeUser2, roskildeUser3, roskildeUser4, roskildeUser5, roskildeUser6, roskildeUser7);

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
                                      new Text() { Id = "intro-head", Value = "Head" },
                                      new Text() { Id = "intro-body", Value = "Body" });

            #endregion

            #region KLE

            var task00 = new TaskRef()
                {
                    TaskKey = "00", Description = "Kommunens styrelse", Type = "KLE", IsPublic = true, OwnedByOrganizationUnit = klRootUnit
                };
            var task0001 = new TaskRef()
                {
                    TaskKey = "00.01",
                    Description = "Kommunens styrelse",
                    Type = "KLE",
                    Parent = task00,
                    IsPublic = true,
                    OwnedByOrganizationUnit = klRootUnit
                };
            var task0003 = new TaskRef()
                {
                    TaskKey = "00.03",
                    Description = "International virksomhed og EU",
                    Type = "KLE",
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
                                                 Type = "KLE",
                                                 Parent = task0001,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.01.10",
                                                 Description = "Opgaver der dækker flere hovedgrupper",
                                                 Type = "KLE",
                                                 Parent = task0001,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         task0003,
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.03.00",
                                                 Description = "International virksomhed og EU i almindelighed",
                                                 Type = "KLE",
                                                 Parent = task0003,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.03.02",
                                                 Description = "Internationale organisationers virksomhed",
                                                 Type = "KLE",
                                                 Parent = task0003,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.03.04",
                                                 Description = "Regionaludvikling EU",
                                                 Type = "KLE",
                                                 Parent = task0003,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.03.08",
                                                 Description = "EU-interessevaretagelse",
                                                 Type = "KLE",
                                                 Parent = task0003,
                                                 IsPublic = true,
                                                 OwnedByOrganizationUnit = klRootUnit
                                             },
                                         new TaskRef()
                                             {
                                                 TaskKey = "00.03.10",
                                                 Description = "Internationalt samarbejde",
                                                 Type = "KLE",
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
                    User = simon,
                    Version = "7.0",
                    Name = "TM Sund",
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
                    User = roskildeUser1,
                    Version = "0.1",
                    Name = "Vækstkurver",
                    Description = "Snitflade for ...",
                    Url = "http://kitos.dk",
                    Parent = null,
                    ExposedBy = system1
                };

            context.ItSystems.AddOrUpdate(x => x.Name, system1, system2);

            #endregion



            context.ProjectPhaseLocales.AddOrUpdate(x => new { x.MunicipalityId, x.OriginalId },
                                                    new ProjPhaseLocale()
                                                        {
                                                            Organization = roskilde,
                                                            Original = projPhase1,
                                                            Name = "Pending"
                                                        }
                );

            base.Seed(context);
        }

        private User SimpleUser(string name, string email, string password, CryptoService cryptoService)
        {
            var salt = cryptoService.Encrypt(name + "salt");
            return new User()
                {
                    Name = name,
                    Email = email,
                    Salt = salt,
                    Password = cryptoService.Encrypt(password + salt)
                };
        }
    }
}

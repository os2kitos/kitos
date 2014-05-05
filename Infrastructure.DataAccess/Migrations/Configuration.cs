using System.Collections.Generic;
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

            var itProjectCategoryPublic = new ItProjectCategory() {IsActive = true, Note = "...", Name = "Fællesoffentlig"};
            var itProjectCategoryMunipalicity = new ItProjectCategory() { IsActive = true, Note = "...", Name = "Fælleskommunal" };
            context.ProjectCategories.AddOrUpdate(x => x.Name, itProjectCategoryPublic, itProjectCategoryMunipalicity);

            var itProjectTypeProject = new ItProjectType() {IsActive = true, Note = "...", Name = "IT Projekt"};
            var itProjectTypeProgram = new ItProjectType() {IsActive = true, Note = "...", Name = "IT Program"};
            context.ProjectTypes.AddOrUpdate(x => x.Name,
                                             itProjectTypeProject,
                                             itProjectTypeProgram,
                                             new ItProjectType() { IsActive = true, Note = "En samlebetegnelse for projekter, som ikke er et IT Program", Name = "Indsatsområde" });

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

            var frequency1 = new Frequency() { IsActive = true, Note = "...", Name = "Dagligt" };
            var frequency2 = new Frequency() { IsActive = true, Note = "...", Name = "Ugentligt" };
            var frequency3 = new Frequency() { IsActive = true, Note = "...", Name = "Månedligt" };
            var frequency4 = new Frequency() { IsActive = true, Note = "...", Name = "Årligt" };

            context.Frequencies.AddOrUpdate(x => x.Name, frequency1, frequency2, frequency3, frequency4);

            context.InterfaceCategories.AddOrUpdate(x => x.Name, 
                new InterfaceCategory(){ IsActive = true, Note = "...", Name = "Kategori 1" },
                new InterfaceCategory(){ IsActive = true, Note = "...", Name = "Kategori 2" });

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

            var globalUser = SimpleUser("Global Test Bruger", "g@test", "test", cryptoService);
            globalUser.IsGlobalAdmin = true;

            var localUser = SimpleUser("Local Test Bruger", "l@test", "test", cryptoService);

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
                    User = simon,
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
                    User = roskildeUser1,
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
                    User = simon,
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
                User = simon,
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
                User = simon,
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
                User = simon,
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
                User = simon,
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
                    Organization = roskilde
                };

            context.ItSystemUsages.AddOrUpdate(x => x.Note, systemUsage1); // TODO probably not the best identifier

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

            #endregion

            #region Wishes

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

            #endregion

            #region IT Project

            var itProject1 = new ItProject()
                {
                    ObjectOwner = globalUser,
                    Name = "Test Projekt",
                    AccessModifier = AccessModifier.Normal,
                    Note = "Test",
                    Background = "Baggrund",
                    ItProjectCategory = itProjectCategoryPublic,
                    ItProjectType = itProjectTypeProject,
                    Organization = roskilde
                };

            context.ItProjects.AddOrUpdate(itProject1);

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

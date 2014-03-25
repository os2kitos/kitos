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

            var localAdmin = new AdminRole { Name = "LocalAdmin" };
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

            context.SystemTypes.AddOrUpdate(x => x.Name,
                                            new SystemType() { IsActive = true, Note = "...", Name = "Fag" },
                                            new SystemType() { IsActive = true, Note = "...", Name = "ESDH" },
                                            new SystemType() { IsActive = true, Note = "...", Name = "Støttesystemer" });

            context.InterfaceTypes.AddOrUpdate(x => x.Name,
                                               new InterfaceType() { IsActive = true, Note = "...", Name = "WS" });

            context.ProtocolTypes.AddOrUpdate(x => x.Name,
                                              new ProtocolType() { IsActive = true, Note = "...", Name = "OIORES" },
                                              new ProtocolType() { IsActive = true, Note = "...", Name = "WS SOAP" });

            context.Methods.AddOrUpdate(x => x.Name,
                                        new Method() { IsActive = true, Note = "...", Name = "Batch" },
                                        new Method() { IsActive = true, Note = "...", Name = "Request-Response" });

            context.DatabaseTypes.AddOrUpdate(x => x.Name,
                                              new DatabaseType() { IsActive = true, Note = "...", Name = "MSSQL" },
                                              new DatabaseType() { IsActive = true, Note = "...", Name = "MySQL" });

            context.Environments.AddOrUpdate(x => x.Name,
                                             new Core.DomainModel.ItSystem.Environment()
                                             {
                                                 IsActive = true,
                                                 Note = "...",
                                                 Name = "Citrix"
                                             });

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

            #region Municipalities

            var globalMunicipality = municipalityService.CreateMunicipality("Fælleskommune");

            var roskilde = municipalityService.CreateMunicipality("Roskilde");

            context.Organizations.AddOrUpdate(x => x.Name, globalMunicipality, roskilde);

            context.SaveChanges();

            #endregion

            #region Roskilde OrgUnits

            //LEVEL 0
            var rootUnit = roskilde.OrgUnits.First();

            //LEVEL 1
            var munChief = new OrganizationUnit()
                {
                    Organization = roskilde,
                    Parent = rootUnit,
                    Name = "Kommunaldirektøren"
                };
            var wellfare = new OrganizationUnit()
            {
                Organization = roskilde,
                Parent = rootUnit,
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

            #region OrganizationUnit roles

            var boss = new OrganizationRole()
                {
                    IsActive = true,
                    Name = "Chef",
                    Note = "Lederen af en organisationsenhed"
                };

            var resourcePerson = new OrganizationRole()
                {
                    IsActive = true,
                    Name = "Ressourceperson",
                    Note = "..."
                };

            context.OrganizationRoles.AddOrUpdate(role => role.Id,
                                                boss, resourcePerson,
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

            var simonSalt = cryptoService.Encrypt("simonsalt");
            var simon = new User
            {
                Name = "Simon Lynn-Pedersen",
                Email = "slp@it-minds.dk",
                Salt = simonSalt,
                Password = cryptoService.Encrypt("slp123" + simonSalt),
                IsGlobalAdmin = true
            };

            var eskildSalt = cryptoService.Encrypt("eskildsalt");
            var eskild = new User()
                {
                    Name = "Eskild",
                    Email = "esd@it-minds.dk",
                    Salt = eskildSalt,
                    Password = cryptoService.Encrypt("arne123" + eskildSalt)
                };

            var brianSalt = cryptoService.Encrypt("brian-foobarbaz");
            var brian = new User()
            {
                Name = "Brian",
                Email = "brian@it-minds.dk",
                Salt = brianSalt,
                Password = cryptoService.Encrypt("brian123" + brianSalt)
            };

            context.Users.AddOrUpdate(x => x.Email, simon, eskild, brian);

            context.SaveChanges();

            #endregion

            #region Admin rights

            context.AdminRights.AddOrUpdate(right => new {right.Object_Id, right.Role_Id, right.User_Id},
                                            new AdminRight()
                                                {
                                                    Object = roskilde,
                                                    Role = localAdmin,
                                                    User = brian
                                                });

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
                                                          User_Id = simonId
                                                      }
                );

             */
            #endregion

            #region Texts

            context.Texts.AddOrUpdate(x => x.Id,
                                      new Text() { Id = "intro-head", Value = "Head" },
                                      new Text() { Id = "intro-body", Value = "Body" });

            #endregion



            context.ProjectPhaseLocales.AddOrUpdate(x => new { x.Municipality_Id, x.Original_Id },
                                                    new ProjPhaseLocale()
                                                        {
                                                            Organization = roskilde,
                                                            Original = projPhase1,
                                                            Name = "Pending"
                                                        }
                );

            base.Seed(context);
        }
    }
}

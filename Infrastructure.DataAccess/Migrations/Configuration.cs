using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            #region Roles
            var globalAdmin = new Role { Name = "GlobalAdmin" };
            var localAdmin = new Role { Name = "LocalAdmin" };

            context.Roles.AddOrUpdate(x => x.Name, globalAdmin, localAdmin);
            
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

            context.SaveChanges();

            #endregion

            #region Global municipality

            var globalMunicipality = new Municipality()
                {
                    Name = "Fælleskommune"
                };

            context.Municipalities.AddOrUpdate(x => x.Name, globalMunicipality);

            var itSupportName = new ItSupportModuleName() {Name = "IT Understøttelse"};
            context.ItSupportModuleNames.AddOrUpdate(x => x.Name,
                new ItSupportModuleName() {Name = "IT Understøttelse af organisation"},
                itSupportName,
                new ItSupportModuleName() {Name = "Organisation"});

            var itProjectName = new ItProjectModuleName() {Name = "IT Projekter"};
            context.ItProjectModuleNames.AddOrUpdate(x => x.Name, 
                itProjectName,
                new ItProjectModuleName() {Name = "Projekter"});

            var itSystemName = new ItSystemModuleName() {Name = "IT Systemer"};
            context.ItSystemModuleNames.AddOrUpdate(x => x.Name,
                itSystemName,
                new ItSystemModuleName() {Name = "Systemer"});

            var itContractName = new ItContractModuleName() {Name = "IT Kontrakter"};
            context.ItContractModuleNames.AddOrUpdate(x => x.Name,
                itContractName,
                new ItContractModuleName() {Name = "Kontrakter"});

            context.Configs.AddOrUpdate(x => x.Municipality,
                new Config() {
                    Municipality = globalMunicipality,
                    ItContractModuleName = itContractName,
                    ItProjectModuleName = itProjectName,
                    ItSupportModuleName = itSupportName,
                    ItSystemModuleName = itSystemName,
                    ShowItContractModule = true,
                    ShowItProjectModule = true,
                    ShowItSystemModule = true,
                });

            context.ItSupportConfigs.AddOrUpdate(x => x.Municipality,
                new ItSupportConfig()
                    {
                        Municipality = globalMunicipality, 
                        ItSupportGuide = ".../itunderstøttelsesvejledning"
                    });

            context.ItProjectConfigs.AddOrUpdate(x => x.Municipality,
                new ItProjectConfig()
                    {
                        Municipality = globalMunicipality,
                        ItProjectGuide = ".../itprojektvejledning"
                    });

            context.ItSystemConfigs.AddOrUpdate(x => x.Municipality,
                new ItSystemConfig()
                    {
                        Municipality = globalMunicipality, 
                        ItSystemGuide = ".../itsystemvejledning"
                    });

            context.ItContractConfigs.AddOrUpdate(x => x.Municipality,
                new ItContractConfig()
                    {
                        Municipality = globalMunicipality,
                        ItContractGuide = ".../itkontraktvejledning"
                    });

            context.SaveChanges();

            #endregion

            #region Users

            var simon = new User
            {
                Name = "Simon Lynn-Pedersen",
                Email = "slp@it-minds.dk",
                Salt = "uw5BuXBIc52n2pL2MH4NRZMg44SVmw3GmrvOAK5pxz4=", //encryption of "saltsimon"
                Password = "2Pps82r5J0vIjvxJjHPf4mF/t2Q5VySmTiT2ZgV7e8U=", //"slp123" encrypted with salt
                Role = globalAdmin,
                Municipality = globalMunicipality
            };

            context.Users.AddOrUpdate(x => x.Email, simon);

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
                                      new Text() {Id = "intro-head", Value = "Head"},
                                      new Text() {Id = "intro-body", Value = "Body"});

            #endregion

            base.Seed(context);
        }
    }
}

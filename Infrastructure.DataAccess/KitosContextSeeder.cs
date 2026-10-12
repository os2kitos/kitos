using System;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.PublicMessage;
using Infrastructure.Services.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess
{
    /// <summary>
    /// Seeds the initial reference data required for a fresh KITOS database.
    /// Replaces the EF6 Configuration.Seed() that ran automatically via Update-Database.
    /// </summary>
    public static class KitosContextSeeder
    {
        public static void Seed(KitosContext context)
        {
            Console.Out.WriteLine("Seeding initial data into kitos database");

            #region USERS

            var salt = $"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}";
            string password;
            using (var cryptoService = new CryptoService())
            {
                password = cryptoService.Encrypt($"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}" + salt);
            }

            const string rootUserEmail = "support@kitos.dk";
            var globalAdmin = context.Users.FirstOrDefault(x => x.Email == rootUserEmail);
            if (globalAdmin == null)
            {
                globalAdmin = new User
                {
                    Name = "Global",
                    LastName = "admin",
                    Email = rootUserEmail,
                    Salt = salt,
                    Password = password,
                    IsGlobalAdmin = true,
                    Uuid = Guid.NewGuid()
                };
                context.Users.Add(globalAdmin);
                context.SaveChanges();
            }

            #endregion

            #region OPTIONS

            Console.Out.WriteLine("Initializing options");

            AddOptions<BusinessType, ItSystem>(context, context.BusinessTypes, globalAdmin,
                "Desing, visualisering og grafik", "Kommunikation", "Hjemmesider og portaler",
                "Selvbetjening og indberetning", "E-læring", "ESDH og Journalisering", "Specialsystemer",
                "Processtyring", "IT management", "Økonomi og betaling", "Løn, personale og HR",
                "BI og ledelsesinformation", "Master data og registre", "GIS",
                "Bruger- og rettighedsstyring", "Sikkerhed og overvågning", "Sagsbærende", "Administrativt");

            AddOptions<ArchiveType, ItSystemUsage>(context, context.ArchiveTypes, globalAdmin,
                "Arkiveret", "Ikke arkiveret", "Arkiveringspligt", "Ikke arkiveringspligt",
                "Øjebliksbillede", "Periode", "Løbende");

            AddOptions<ArchiveLocation, ItSystemUsage>(context, context.ArchiveLocation, globalAdmin,
                "Aalborg", "Aabenraa", "Vejle", "Vejen", "Tårnby", "Tønder", "Thisted", "Sønderborg",
                "Syddjurs", "Struer", "Slagelse", "Skive", "Silkeborg", "Rudersdal", "Roskilde",
                "Randers", "Odense", "Næstved", "Norddjurs", "Mariagerfjord", "Læsø",
                "Lyngby-Taarbæk", "Lolland", "København", "Kolding", "Ishøj", "Hørsholm",
                "Horsens", "Holbæk", "Hjørring", "Helsingør", "Hedensted", "Haderslev",
                "Guldborgsund", "Gribskov", "Greve", "Gladsaxe", "Gentofte", "Furesø",
                "Frederikssund", "Frederikshavn", "Fredensborg", "Faxe", "Esbjerg", "Egedal",
                "Dragør", "Brøndby", "Bornholm", "Billund");

            AddOptions<SystemUsageCriticalityLevel, ItSystemUsage>(context, context.SystemUsageCriticalityLevelTypes, globalAdmin,
                "TestCriticality1", "TestCriticality2", "TestCriticality3");

            AddOptions<TechnicalSystemType, ItSystemUsage>(context, context.TechnicalSystemTypes, globalAdmin,
                "Teknisk systemtype 1", "Teknisk systemtype 2", "Teknisk systemtype 3");

            AddOptions<DataType, DataRow>(context, context.DataTypes, globalAdmin,
                "Person", "Virksomhed", "Sag", "Dokument", "Organisation", "Klassikfikation",
                "Ejendom", "GIS", "Andet");

            AddOptions<RelationFrequencyType, SystemRelation>(context, context.RelationFrequencyTypes, globalAdmin,
                "Dagligt", "Ugentligt", "Månedligt", "Årligt", "Kvartal", "Halvårligt");

            AddOptions<InterfaceType, ItInterface>(context, context.InterfaceTypes, globalAdmin,
                "CSV", "WS SOAP", "WS REST", "MOX", "OIO REST", "LDAP", "User interface", "ODBC (SQL)", "Andet");

            AddOptions<SensitiveDataType, ItSystemUsage>(context, context.SensitiveDataTypes, globalAdmin,
                "Ja", "Nej");

            AddOptions<ItContractType, ItContract>(context, context.ItContractTypes, globalAdmin,
                "Hovedkontrakt", "Tillægskontrakt", "Snitflade", "Serviceaftale", "Databehandleraftale");

            AddOptions<ItContractTemplateType, ItContract>(context, context.ItContractTemplateTypes, globalAdmin,
                "K01", "K02", "K03", "Egen", "KOMBIT", "Leverandør", "OPI", "Anden");

            AddOptions<PurchaseFormType, ItContract>(context, context.PurchaseFormTypes, globalAdmin,
                "SKI", "SKI 02.18", "SKI 02.19", "Udbud", "EU udbud", "Direkte tildeling", "Annoncering");

            AddOptions<CriticalityType, ItContract>(context, context.CriticalityTypes, globalAdmin,
                "Kritikalitet 1", "Kritikalitet 2", "Kritikalitet 3");

            AddOptions<PaymentModelType, ItContract>(context, context.PaymentModelTypes, globalAdmin,
                "Licens", "icens - flatrate", "Licens - forbrug", "Licens - indbyggere",
                "Licens - pr. sag", "Gebyr", "Engangsydelse");

            AddOptions<AgreementElementType, ItContract>(context, context.AgreementElementTypes, globalAdmin,
                "Licens", "Udvikling", "Drift", "Vedligehold", "Support",
                "Serverlicenser", "Serverdrift", "Databaselicenser", "Backup", "Overvågning");

            AddOptions<OptionExtendType, ItContract>(context, context.OptionExtendTypes, globalAdmin,
                "2 x 1 år", "1 x 1 år", "1 x ½ år");

            AddOptions<PaymentFreqencyType, ItContract>(context, context.PaymentFreqencyTypes, globalAdmin,
                "Månedligt", "Kvartalsvis", "Halvårligt", "Årligt");

            AddOptions<PriceRegulationType, ItContract>(context, context.PriceRegulationTypes, globalAdmin,
                "TSA", "KL pris og lønskøn", "Nettoprisindeks");

            AddOptions<ProcurementStrategyType, ItContract>(context, context.ProcurementStrategyTypes, globalAdmin,
                "Direkte tildeling", "Annoncering", "Udbud", "EU udbud", "Mini-udbud",
                "SKI - direkte tildeling", "SKI - mini-udbud", "Underhåndsbud");

            AddOptions<TerminationDeadlineType, ItContract>(context, context.TerminationDeadlineTypes, globalAdmin,
                "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12");

            AddOptions<ItSystemCategories, ItSystemUsage>(context, context.ItSystemCategories, globalAdmin,
                "Offentlige data", "Interne data", "Fortrolige data", "Hemmelige data");

            AddOptions<ArchiveTestLocation, ItSystemUsage>(context, context.ArchiveTestLocation, globalAdmin,
                "TestLocation1", "TestLocation2");

            AddOptions<RegisterType, ItSystemUsage>(context, context.RegisterTypes, globalAdmin,
                "TestRegisterType1", "TestRegisterType2");

            AddOptions<CountryCode, Organization>(context, context.CountryCodes, globalAdmin, "NO", "SE", "UA");

            context.SaveChanges();

            #endregion

            #region ORG ROLES

            Console.Out.WriteLine("Initializing org roles");

            AddRoleIfMissing<OrganizationUnitRole, OrganizationUnitRight>(context.OrganizationUnitRoles, globalAdmin,
                new OrganizationUnitRole { IsLocallyAvailable = true, Name = "Chef",                    Description = "Lederen af en organisationsenhed", HasWriteAccess = true,  Priority = 7 });
            AddRoleIfMissing<OrganizationUnitRole, OrganizationUnitRight>(context.OrganizationUnitRoles, globalAdmin,
                new OrganizationUnitRole { IsLocallyAvailable = true, Name = "Ressourceperson",         Description = "...", HasWriteAccess = true,  Priority = 6 });
            AddRoleIfMissing<OrganizationUnitRole, OrganizationUnitRight>(context.OrganizationUnitRoles, globalAdmin,
                new OrganizationUnitRole { IsLocallyAvailable = true, Name = "Medarbejder",             Description = "...", HasWriteAccess = false, Priority = 5 });
            AddRoleIfMissing<OrganizationUnitRole, OrganizationUnitRight>(context.OrganizationUnitRoles, globalAdmin,
                new OrganizationUnitRole { IsLocallyAvailable = true, Name = "Digitaliseringskonsulent", Description = "...", HasWriteAccess = true,  Priority = 4 });
            AddRoleIfMissing<OrganizationUnitRole, OrganizationUnitRight>(context.OrganizationUnitRoles, globalAdmin,
                new OrganizationUnitRole { IsLocallyAvailable = true, Name = "IT konsulent",            Description = "...", HasWriteAccess = true,  Priority = 3 });
            AddRoleIfMissing<OrganizationUnitRole, OrganizationUnitRight>(context.OrganizationUnitRoles, globalAdmin,
                new OrganizationUnitRole { IsLocallyAvailable = true, Name = "Leder",                   Description = "...", HasWriteAccess = true,  Priority = 2 });
            AddRoleIfMissing<OrganizationUnitRole, OrganizationUnitRight>(context.OrganizationUnitRoles, globalAdmin,
                new OrganizationUnitRole { IsLocallyAvailable = true, Name = "Direktør",                Description = "...", HasWriteAccess = false, Priority = 1 });

            context.SaveChanges();

            #endregion

            #region SYSTEM ROLES

            Console.Out.WriteLine("Initializing system roles");

            AddRoleIfMissing<ItSystemRole, ItSystemRight>(context.ItSystemRoles, globalAdmin,
                new ItSystemRole { HasReadAccess = true, HasWriteAccess = true, IsLocallyAvailable = true, Name = "Systemejer",            Priority = 9 });
            AddRoleIfMissing<ItSystemRole, ItSystemRight>(context.ItSystemRoles, globalAdmin,
                new ItSystemRole { HasReadAccess = true, HasWriteAccess = true, IsLocallyAvailable = true, Name = "Systemansvarlig",        Priority = 8 });
            AddRoleIfMissing<ItSystemRole, ItSystemRight>(context.ItSystemRoles, globalAdmin,
                new ItSystemRole { HasReadAccess = true, HasWriteAccess = true, IsLocallyAvailable = true, Name = "Forretningsejer",        Priority = 7 });
            AddRoleIfMissing<ItSystemRole, ItSystemRight>(context.ItSystemRoles, globalAdmin,
                new ItSystemRole { HasReadAccess = true, HasWriteAccess = true, IsLocallyAvailable = true, Name = "Superbrugeransvarlig",   Priority = 6 });
            AddRoleIfMissing<ItSystemRole, ItSystemRight>(context.ItSystemRoles, globalAdmin,
                new ItSystemRole { HasReadAccess = true, HasWriteAccess = true, IsLocallyAvailable = true, Name = "Superbruger",            Priority = 5 });
            AddRoleIfMissing<ItSystemRole, ItSystemRight>(context.ItSystemRoles, globalAdmin,
                new ItSystemRole { HasReadAccess = true, HasWriteAccess = true, IsLocallyAvailable = true, Name = "Sikkerhedsansvarlig",    Priority = 4 });
            AddRoleIfMissing<ItSystemRole, ItSystemRight>(context.ItSystemRoles, globalAdmin,
                new ItSystemRole { HasReadAccess = true, HasWriteAccess = true, IsLocallyAvailable = true, Name = "Changemanager",          Priority = 3 });
            AddRoleIfMissing<ItSystemRole, ItSystemRight>(context.ItSystemRoles, globalAdmin,
                new ItSystemRole { HasReadAccess = true, HasWriteAccess = true, IsLocallyAvailable = true, Name = "Dataejer",               Priority = 2 });
            AddRoleIfMissing<ItSystemRole, ItSystemRight>(context.ItSystemRoles, globalAdmin,
                new ItSystemRole { HasReadAccess = true, HasWriteAccess = true, IsLocallyAvailable = true, Name = "Systemadminstrator",     Priority = 1 });

            context.SaveChanges();

            #endregion

            #region CONTRACT ROLES

            Console.Out.WriteLine("Initializing contract roles");

            AddRoleIfMissing<ItContractRole, ItContractRight>(context.ItContractRoles, globalAdmin,
                new ItContractRole { HasWriteAccess = true, Name = "Kontraktejer",       IsObligatory = false, IsLocallyAvailable = true, Priority = 6 });
            AddRoleIfMissing<ItContractRole, ItContractRight>(context.ItContractRoles, globalAdmin,
                new ItContractRole { HasWriteAccess = true, Name = "Kontraktmanager",    IsObligatory = false, IsLocallyAvailable = true, Priority = 5 });
            AddRoleIfMissing<ItContractRole, ItContractRight>(context.ItContractRoles, globalAdmin,
                new ItContractRole { HasWriteAccess = true, Name = "Juridisk rådgiver",  IsObligatory = false, IsLocallyAvailable = true, Priority = 4 });
            AddRoleIfMissing<ItContractRole, ItContractRight>(context.ItContractRoles, globalAdmin,
                new ItContractRole { HasWriteAccess = true, Name = "Konsulent",          IsObligatory = false, IsLocallyAvailable = true, Priority = 3 });
            AddRoleIfMissing<ItContractRole, ItContractRight>(context.ItContractRoles, globalAdmin,
                new ItContractRole { HasWriteAccess = true, Name = "Fakturamodtager",    IsObligatory = false, IsLocallyAvailable = true, Priority = 2 });
            AddRoleIfMissing<ItContractRole, ItContractRight>(context.ItContractRoles, globalAdmin,
                new ItContractRole { HasWriteAccess = true, Name = "Budgetansvarlig",    IsObligatory = false, IsLocallyAvailable = true, Priority = 1 });

            context.SaveChanges();

            #endregion

            #region DPA ROLES

            Console.Out.WriteLine("Initializing dpa roles");

            AddRoleIfMissing<DataProcessingRegistrationRole, DataProcessingRegistrationRight>(context.DataProcessingRegistrationRoles, globalAdmin,
                new DataProcessingRegistrationRole { HasWriteAccess = true,  Name = "Standard Skriverolle", IsObligatory = true, IsLocallyAvailable = true, Priority = 1 });
            AddRoleIfMissing<DataProcessingRegistrationRole, DataProcessingRegistrationRight>(context.DataProcessingRegistrationRoles, globalAdmin,
                new DataProcessingRegistrationRole { HasWriteAccess = false, Name = "Standard Læserolle",   IsObligatory = true, IsLocallyAvailable = true, Priority = 2 });

            context.SaveChanges();

            #endregion

            #region ORGANIZATIONS

            Console.Out.WriteLine("Initializing organizations");

            AddOrganizationTypeIfMissing(context, "Kommune",                     OrganizationCategory.Municipality);
            AddOrganizationTypeIfMissing(context, "Interessefællesskab",         OrganizationCategory.Municipality);
            AddOrganizationTypeIfMissing(context, "Virksomhed",                  OrganizationCategory.Other);
            AddOrganizationTypeIfMissing(context, "Anden offentlig myndighed",   OrganizationCategory.Other);

            context.SaveChanges();

            if (!context.Organizations.Any(x => x.Name == "Fælles Kommune"))
            {
                var orgType = context.OrganizationTypes.Single(x => x.Name == "Kommune");
                var commonOrg = new Organization
                {
                    Name = "Fælles Kommune",
                    Config = Config.Default(globalAdmin),
                    TypeId = orgType.Id,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    AccessModifier = AccessModifier.Public,
                    IsDefaultOrganization = true,
                    Uuid = Guid.NewGuid()
                };
                commonOrg.OrgUnits.Add(new OrganizationUnit
                {
                    Name = commonOrg.Name,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id,
                    Uuid = Guid.NewGuid()
                });
                context.Organizations.Add(commonOrg);
                context.SaveChanges();
            }

            #endregion

            #region TEXTS

            Console.Out.WriteLine("Initializing texts");

            const string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

            AddTextIfMissing(context, globalAdmin, "Introduktion til kitos");
            AddTextIfMissing(context, globalAdmin, "Misc text, links osv");
            AddTextIfMissing(context, globalAdmin, "Guides");
            AddTextIfMissing(context, globalAdmin, "Statusbeskeder");
            AddTextIfMissing(context, globalAdmin, "Kontaktinformationer");

            AddPublicMessageIfMissing(context, globalAdmin, "Vejledninger",
                "Skabeloner til brug ved oprettelse af IT-Systemer, leverandører og snitflader finder du her.", lorem, "https://google.com");
            AddPublicMessageIfMissing(context, globalAdmin, "Skabeloner",
                "Brugerklubben i Kitos har et filarkiv, hvor du og dine kolleger kan tilgå materiale om Kitos.", lorem, "https://google.com");
            AddPublicMessageIfMissing(context, globalAdmin, "Arrangementer",
                "Brugerklubben holder løbende arrangementer, som du har mulighed for at tilmelde dig.", lorem, "https://google.com");
            AddPublicMessageIfMissing(context, globalAdmin, "Driftsstatus",
                "Hvis der opleves fejl i Kitos vil du kunne se en begrundelse på denne side.", lorem, "https://google.com",
                status: PublicMessageStatus.Active);
            AddPublicMessageIfMissing(context, globalAdmin, "OS2Kitos kontaktpersoner",
                "Se hvem, der er Kitos kontaktpersoner i kommunerne og spar med hinanden.", lorem, "https://google.com");
            AddPublicMessageIfMissing(context, globalAdmin, "Kontakt",
                "Har du nogen spørgsmål til Kitos? Kontakt os og vi vil hjælpe dig hurtigst muligt.", lorem, "https://google.com");
            AddPublicMessageIfMissing(context, globalAdmin, "Kitos er Kommunernes IT Overblikssystem",
                "Kitos er en open-source web-baseret løsning, der anvendes af 76 kommuner. Kitos skaber overblik over den samlede kommunale IT-portefølje.",
                null, "https://www.os2.eu/os2kitos", isMain: true);

            context.SaveChanges();

            #endregion
        }

        private static void AddOptions<T, TReference>(KitosContext context, DbSet<T> dbSet, User owner, params string[] names)
            where T : OptionEntity<TReference>, new()
        {
            var existing = dbSet.Select(x => x.Name).ToHashSet();
            var priority = names.Length;
            foreach (var name in names)
            {
                if (!existing.Contains(name))
                {
                    dbSet.Add(new T
                    {
                        IsObligatory = true,
                        IsEnabled = true,
                        IsLocallyAvailable = true,
                        Name = name,
                        Description = "...",
                        ObjectOwnerId = owner.Id,
                        LastChangedByUserId = owner.Id,
                        Priority = priority
                    });
                }
                priority--;
            }
        }

        private static void AddRoleIfMissing<T, TRight>(DbSet<T> dbSet, User owner, T role)
            where T : OptionEntity<TRight>, new()
        {
            if (!dbSet.Any(x => x.Name == role.Name))
            {
                role.ObjectOwnerId = owner.Id;
                role.LastChangedByUserId = owner.Id;
                dbSet.Add(role);
            }
        }

        private static void AddOrganizationTypeIfMissing(KitosContext context, string name, OrganizationCategory category)
        {
            if (!context.OrganizationTypes.Any(x => x.Name == name))
                context.OrganizationTypes.Add(new OrganizationType { Name = name, Category = category });
        }

        private static void AddTextIfMissing(KitosContext context, User owner, string value)
        {
            if (!context.Texts.Any(x => x.Value == value))
                context.Texts.Add(new Text { Value = value, ObjectOwnerId = owner.Id, LastChangedByUserId = owner.Id });
        }

        private static void AddPublicMessageIfMissing(KitosContext context, User owner, string title,
            string shortDescription, string? longDescription, string link,
            bool isMain = false, PublicMessageStatus status = PublicMessageStatus.Inactive)
        {
            if (!context.PublicMessages.Any(x => x.Title == title))
            {
                context.PublicMessages.Add(new PublicMessage
                {
                    Title = title,
                    ShortDescription = shortDescription,
                    LongDescription = longDescription,
                    Link = link,
                    IsMain = isMain,
                    Status = status,
                    ObjectOwnerId = owner.Id,
                    LastChangedByUserId = owner.Id
                });
            }
        }
    }
}

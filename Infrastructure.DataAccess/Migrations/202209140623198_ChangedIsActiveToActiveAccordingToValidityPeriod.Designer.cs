﻿// <auto-generated />
namespace Infrastructure.DataAccess.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode("EntityFramework.Migrations", "6.4.4")]
    public sealed partial class ChangedIsActiveToActiveAccordingToValidityPeriod : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(ChangedIsActiveToActiveAccordingToValidityPeriod));
        
        string IMigrationMetadata.Id
        {
            get { return "202209140623198_ChangedIsActiveToActiveAccordingToValidityPeriod"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}

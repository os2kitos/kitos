using System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Xunit;

namespace Tests.Unit.Core.Model;

public class PersonalDataDeletionTest
{
    [Theory]
    [InlineData(PersonalDataDeletionCapability.IndividualRecords)]
    [InlineData(PersonalDataDeletionCapability.BulkDeletion)]
    public void Can_Set_All_Details_And_Clear(PersonalDataDeletionCapability capability)
    {
        var usage = new ItSystemUsage { OrganizationId = 1 };
        var user = Member();
        var date = new DateTime(2026, 10, 22);
        Assert.True(usage.UpdatePersonalDataDeletion(capability, PersonalDataDeletionMethod.BySupplierOnRequest,
            new PersonalDataDeletionDeadlineType { Id = 12, Name = "30 days" }, "https://procedure", "Procedure", user, date, "https://check", "Check").IsNone);
        Assert.Equal(capability, usage.PersonalDataDeletionCapability);
        Assert.Equal(PersonalDataDeletionMethod.BySupplierOnRequest, usage.PersonalDataDeletionMethod);
        Assert.Equal("30 days", usage.PersonalDataDeletionDeadlineType.Name);
        Assert.Equal(12, usage.PersonalDataDeletionDeadlineTypeId);
        Assert.Same(user, usage.PersonalDataDeletionResponsibleUser);
        Assert.Equal(user.Id, usage.PersonalDataDeletionResponsibleUserId);
        Assert.Equal(date, usage.PersonalDataDeletionVerifiedDate);
        Assert.Equal("https://procedure", usage.PersonalDataDeletionProcedureDocumentationUrl);
        Assert.Equal("Procedure", usage.PersonalDataDeletionProcedureDocumentationUrlName);
        Assert.Equal("https://check", usage.PersonalDataDeletionVerificationDocumentationUrl);
        Assert.Equal("Check", usage.PersonalDataDeletionVerificationDocumentationUrlName);

        Assert.True(usage.UpdatePersonalDataDeletion(PersonalDataDeletionCapability.No, null, null, null, null, null, null, null, null).IsNone);
        Assert.Null(usage.PersonalDataDeletionMethod);
        Assert.Null(usage.PersonalDataDeletionDeadlineType);
        Assert.Null(usage.PersonalDataDeletionDeadlineTypeId);
        Assert.Null(usage.PersonalDataDeletionResponsibleUser);
        Assert.Null(usage.PersonalDataDeletionResponsibleUserId);
        Assert.Null(usage.PersonalDataDeletionVerifiedDate);
        Assert.Null(usage.PersonalDataDeletionProcedureDocumentationUrl);
        Assert.Null(usage.PersonalDataDeletionProcedureDocumentationUrlName);
        Assert.Null(usage.PersonalDataDeletionVerificationDocumentationUrl);
        Assert.Null(usage.PersonalDataDeletionVerificationDocumentationUrlName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(PersonalDataDeletionCapability.No)]
    public void Disabled_Deletion_Rejects_Each_Dependent_Field(PersonalDataDeletionCapability? capability)
    {
        var usage = new ItSystemUsage { OrganizationId = 1 };
        Assert.True(usage.UpdatePersonalDataDeletion(capability, PersonalDataDeletionMethod.AutomaticallyBySystem, null, null, null, null, null, null, null).HasValue);
        Assert.True(usage.UpdatePersonalDataDeletion(capability, null, new PersonalDataDeletionDeadlineType { Id = 12, Name = "deadline" }, null, null, null, null, null, null).HasValue);
        Assert.True(usage.UpdatePersonalDataDeletion(capability, null, null, "url", null, null, null, null, null).HasValue);
        Assert.True(usage.UpdatePersonalDataDeletion(capability, null, null, null, "name", null, null, null, null).HasValue);
        Assert.True(usage.UpdatePersonalDataDeletion(capability, null, null, null, null, Member(), null, null, null).HasValue);
        Assert.True(usage.UpdatePersonalDataDeletion(capability, null, null, null, null, null, DateTime.UtcNow, null, null).HasValue);
        Assert.True(usage.UpdatePersonalDataDeletion(capability, null, null, null, null, null, null, "url", null).HasValue);
        Assert.True(usage.UpdatePersonalDataDeletion(capability, null, null, null, null, null, null, null, "name").HasValue);
        Assert.Null(usage.PersonalDataDeletionCapability);
    }

    [Theory]
    [InlineData(1, false, true)]
    [InlineData(2, false, false)]
    [InlineData(1, true, false)]
    public void Assignment_Requires_Active_Organization_Member(int organizationId, bool deleted, bool accepted)
    {
        var usage = new ItSystemUsage { OrganizationId = 1 };
        var user = Member(organizationId);
        user.Deleted = deleted;
        var result = usage.UpdatePersonalDataDeletion(PersonalDataDeletionCapability.BulkDeletion, null, null, null, null, user, null, null, null);
        Assert.Equal(accepted, result.IsNone);
    }

    [Fact]
    public void Retains_Historical_Assignment_And_Rejects_Invalid_Enums_Atomically()
    {
        var usage = new ItSystemUsage { OrganizationId = 1 };
        var user = Member();
        usage.UpdatePersonalDataDeletion(PersonalDataDeletionCapability.BulkDeletion, null, new PersonalDataDeletionDeadlineType { Id = 12, Name = "original" }, null, null, user, null, null, null);
        user.Deleted = true;
        user.OrganizationRights.Clear();
        Assert.True(usage.UpdatePersonalDataDeletion(PersonalDataDeletionCapability.BulkDeletion, null, new PersonalDataDeletionDeadlineType { Id = 12, Name = "updated" }, null, null, user, null, null, null).IsNone);
        Assert.True(usage.UpdatePersonalDataDeletion((PersonalDataDeletionCapability)99, null, null, null, null, null, null, null, null).HasValue);
        Assert.True(usage.UpdatePersonalDataDeletion(PersonalDataDeletionCapability.BulkDeletion, (PersonalDataDeletionMethod)99, null, null, null, null, null, null, null).HasValue);
        Assert.Equal("updated", usage.PersonalDataDeletionDeadlineType.Name);
        Assert.Same(user, usage.PersonalDataDeletionResponsibleUser);
    }

    [Fact]
    public void Rejects_Overlong_Link_Names()
    {
        var usage = new ItSystemUsage();
        Assert.True(usage.UpdatePersonalDataDeletion(PersonalDataDeletionCapability.BulkDeletion, null, null, null, new string('x', 151), null, null, null, null).HasValue);
        Assert.True(usage.UpdatePersonalDataDeletion(PersonalDataDeletionCapability.BulkDeletion, null, null, null, null, null, null, null, new string('x', 151)).HasValue);
    }

    private static User Member(int organizationId = 1)
    {
        var user = new User { Id = 42 };
        user.OrganizationRights.Add(new OrganizationRight { OrganizationId = organizationId });
        return user;
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class GDPRWriteRequestDTOTest
    {
        private const int UndefinedEnumValue = 99;

        [Theory]
        [InlineData(nameof(GDPRWriteRequestDTO.PersonalDataDeletionCapability))]
        [InlineData(nameof(GDPRWriteRequestDTO.PersonalDataDeletionMethod))]
        public void Validate_Undefined_Deletion_Enum_Rejects_Property(string propertyName)
        {
            var request = new GDPRWriteRequestDTO();
            if (propertyName == nameof(GDPRWriteRequestDTO.PersonalDataDeletionCapability))
                request.PersonalDataDeletionCapability = (PersonalDataDeletionCapabilityChoice)UndefinedEnumValue;
            else
                request.PersonalDataDeletionMethod = (PersonalDataDeletionMethodChoice)UndefinedEnumValue;
            var errors = new List<ValidationResult>();

            Assert.False(Validator.TryValidateObject(request, new ValidationContext(request), errors, true));
            Assert.Contains(propertyName, Assert.Single(errors).MemberNames);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(PersonalDataDeletionCapabilityChoice.No, null)]
        [InlineData(PersonalDataDeletionCapabilityChoice.IndividualRecords, PersonalDataDeletionMethodChoice.ManuallyByOrganization)]
        [InlineData(PersonalDataDeletionCapabilityChoice.BulkDeletion, PersonalDataDeletionMethodChoice.BySupplierOnRequest)]
        [InlineData(PersonalDataDeletionCapabilityChoice.BulkDeletion, PersonalDataDeletionMethodChoice.AutomaticallyBySystem)]
        public void Validate_Defined_Or_Null_Deletion_Enums_Accepts_Properties(
            PersonalDataDeletionCapabilityChoice? capability, PersonalDataDeletionMethodChoice? method)
        {
            var request = new GDPRWriteRequestDTO
            {
                PersonalDataDeletionCapability = capability,
                PersonalDataDeletionMethod = method
            };

            Assert.True(Validator.TryValidateObject(request, new ValidationContext(request), new List<ValidationResult>(), true));
        }
    }
}

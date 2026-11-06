using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class GeneralDataWriteRequestDTOTest
    {
        private const int MaximumPurposeLength = 1000;

        [Theory]
        [InlineData(0, true)]
        [InlineData(MaximumPurposeLength, true)]
        [InlineData(MaximumPurposeLength + 1, false)]
        public void Validate_Purpose_Enforces_Maximum_Length(int length, bool expectedValid)
        {
            var request = new GeneralDataWriteRequestDTO { Purpose = new string('a', length) };
            var errors = new List<ValidationResult>();

            var valid = Validator.TryValidateObject(request, new ValidationContext(request), errors, true);

            Assert.Equal(expectedValid, valid);
            if (!expectedValid)
                Assert.Contains(nameof(GeneralDataWriteRequestDTO.Purpose), Assert.Single(errors).MemberNames);
        }
    }
}

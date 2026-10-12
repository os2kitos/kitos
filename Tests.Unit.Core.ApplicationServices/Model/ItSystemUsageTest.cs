using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model
{
    public class ItSystemUsageTest : WithAutoFixture
    {
        private readonly ItSystemUsage _sut;

        public ItSystemUsageTest()
        {
            _sut = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>()
            };
        }

        [Fact]
        public void SetLicensingAndCodeModels_Returns_Conflict_If_Proprietary_And_Other_Are_Present()
        {
            var models = new[] { LicensingAndCodeModel.OpenSource, LicensingAndCodeModel.Proprietary };

            var result = _sut.SetLicensingAndCodeModels(models);

            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Conflict, result.Value.FailureType);
            Assert.Empty(_sut.LicensingAndCodeModels);
        }

        [Fact]
        public void SetLicensingAndCodeModels_Does_Not_Add_Duplicates()
        {
            var values = new[] { LicensingAndCodeModel.OpenSource, LicensingAndCodeModel.Freeware, LicensingAndCodeModel.Freeware };
            var uniqueValuesCount = values.ToHashSet().Count;
            _sut.SetLicensingAndCodeModels(values);

            var result = _sut.SetLicensingAndCodeModels(values);

            Assert.True(result.IsNone);
            Assert.Equal(uniqueValuesCount, _sut.LicensingAndCodeModels.Count);
        }

        [Fact]
        public void ResetSystemUsageCriticalityLevel_SetsLastChanged_AndSetsValueToNull()
        {
            _sut.UpdateSystemUsageCriticalityLevel(new SystemUsageCriticalityLevel() { Uuid = A<Guid>(), Name = A<string>()});
            var beforeChange = _sut.CriticalityFieldsLastChanged;
            Assert.NotNull(_sut.SystemUsageCriticalityLevel);

            _sut.ResetSystemUsageCriticalityLevel();

            Assert.Null(_sut.SystemUsageCriticalityLevel);
            AssertCriticalityFieldsLastChanged(beforeChange);
        }

        [Fact]
        public void ResetCriticalityLevelDocumentation_SetsLastChanged_AndSetsValuesToNull()
        {
            _sut.UpdateCriticalityLevelDocumentationUrl(A<string>());
            _sut.UpdateCriticalityLevelDocumentationName(A<string>());
            var beforeChange = _sut.CriticalityFieldsLastChanged;
            Assert.NotNull(_sut.CriticalityLevelDocumentationUrl);
            Assert.NotNull(_sut.CriticalityLevelDocumentationName);

            _sut.ResetCriticalityLevelDocumentation();

            Assert.Null(_sut.CriticalityLevelDocumentationUrl);
            Assert.Null(_sut.CriticalityLevelDocumentationName);
            AssertCriticalityFieldsLastChanged(beforeChange);
        }

        [Fact]
        public void UpdateIsBusinessCritical_UpdatesCriticalityFieldsLastChanged_WhenNewValue()
        {
            _sut.UpdateIsBusinessCritical(DataOptions.NO);
            var beforeChange = _sut.CriticalityFieldsLastChanged;
            var expected = DataOptions.YES;

            _sut.UpdateIsBusinessCritical(expected);

            Assert.Equal(expected, _sut.isBusinessCritical);
            AssertCriticalityFieldsLastChanged(beforeChange);
        }

        [Fact]
        public void UpdateCriticalityLevelDocumentationUrl_DoesNotUpdateCriticalityFieldsLastChanged_WhenNotNewValue()
        {
            var existingValue = A<string?>();
            _sut.UpdateCriticalityLevelDocumentationUrl(existingValue);
            
            var beforeChange = _sut.CriticalityFieldsLastChanged;

            _sut.UpdateCriticalityLevelDocumentationUrl(existingValue);

            Assert.Equal(existingValue, _sut.CriticalityLevelDocumentationUrl);
            AssertCriticalityFieldsLastChanged(beforeChange, false);
        }

        [Fact]
        public void UpdateCriticalityLevelDocumentationName_UpdatesCriticalityFieldsLastChanged_WhenNewValue()
        {
            var existing = A<string?>();
            _sut.UpdateCriticalityLevelDocumentationName(existing);
            var expected = A<string?>();
            var beforeChange = _sut.CriticalityFieldsLastChanged;

            _sut.UpdateCriticalityLevelDocumentationName(expected);

            Assert.Equal(expected, _sut.CriticalityLevelDocumentationName);
            AssertCriticalityFieldsLastChanged(beforeChange);
        }

        [Fact]
        public void UpdateCriticalityLevelDocumentationName_DoesNotUpdateCriticalityFieldsLastChanged_WhenNotNewValue()
        {
            var existingValue = A<string?>();
            _sut.UpdateCriticalityLevelDocumentationName(existingValue);
            var beforeChange = _sut.CriticalityFieldsLastChanged;

            _sut.UpdateCriticalityLevelDocumentationName(existingValue);

            Assert.Equal(existingValue, _sut.CriticalityLevelDocumentationName);
            AssertCriticalityFieldsLastChanged(beforeChange, false);
        }

        [Fact]
        public void UpdateCriticalityLevelDocumentationUrl_UpdatesCriticalityFieldsLastChanged_WhenNewValue()
        {
            var existing = A<string?>();
            _sut.UpdateCriticalityLevelDocumentationUrl(existing);
            var expected = A<string?>();
            var beforeChange = _sut.CriticalityFieldsLastChanged;

            _sut.UpdateCriticalityLevelDocumentationUrl(expected);

            Assert.Equal(expected, _sut.CriticalityLevelDocumentationUrl);
            AssertCriticalityFieldsLastChanged(beforeChange);
        }

        [Fact]
        public void UpdateIsBusinessCritical_DoesNotUpdateCriticalityFieldsLastChanged_WhenNotNewValue()
        {
            var existingValue = DataOptions.YES;
            _sut.UpdateIsBusinessCritical(existingValue);

            var beforeChange = _sut.CriticalityFieldsLastChanged;

            _sut.UpdateIsBusinessCritical(existingValue);

            Assert.Equal(existingValue, _sut.isBusinessCritical);
            AssertCriticalityFieldsLastChanged(beforeChange, false);
        }

        [Fact]
        public void UpdateIsSociallyCritical_UpdatesCriticalityFieldsLastChanged_WhenNewValue()
        {
            _sut.UpdateIsSociallyCritical(DataOptions.NO);
            var expected = DataOptions.YES;
            var beforeChange = _sut.CriticalityFieldsLastChanged;

            _sut.UpdateIsSociallyCritical(expected);

            Assert.Equal(expected, _sut.IsSociallyCritical);
            AssertCriticalityFieldsLastChanged(beforeChange);
        }

        [Fact]
        public void UpdateIsSociallyCritical_DoesNotUpdateCriticalityFieldsLastChanged_WhenNotNewValue()
        {
            var existingValue = DataOptions.YES;
            _sut.UpdateIsSociallyCritical(existingValue);

            var beforeChange = _sut.CriticalityFieldsLastChanged;

            _sut.UpdateIsSociallyCritical(existingValue);

            Assert.Equal(existingValue, _sut.IsSociallyCritical);
            AssertCriticalityFieldsLastChanged(beforeChange, false);
        }

        [Theory]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        [InlineData(DataOptions.YES)]
        public void UpdateRetentionPeriodDefined_Only_Clears_Related_Fields_If_Not_Yes(DataOptions retentionPeriodDefined)
        {
            var nextDataRetentionEvaluationDate = A<DateTime>();
            var DataRetentionEvaluationFrequencyInMonths = A<int>();
            _sut.UpdateRetentionPeriodDefined(DataOptions.YES);
            _sut.UpdateNextDataRetentionEvaluationDate(nextDataRetentionEvaluationDate);
            _sut.UpdateDataRetentionEvaluationFrequencyInMonths(DataRetentionEvaluationFrequencyInMonths);

            _sut.UpdateRetentionPeriodDefined(retentionPeriodDefined);

            Assert.Equal(retentionPeriodDefined, _sut.answeringDataDPIA);
            if (retentionPeriodDefined == DataOptions.YES)
            {
                Assert.Equal(retentionPeriodDefined, _sut.answeringDataDPIA);
                Assert.Equal(nextDataRetentionEvaluationDate, _sut.DPIAdeleteDate);
                Assert.Equal(DataRetentionEvaluationFrequencyInMonths, _sut.numberDPIA);
            }
            else
            {
                Assert.Null(_sut.DPIADateFor);
                Assert.Null(_sut.DPIASupervisionDocumentationUrl);
                Assert.Null(_sut.DPIASupervisionDocumentationUrlName);
            }
        }

        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateNextDataRetentionEvaluationDate_Updates_If_Retention_Period_Defined_Is_Yes(DataOptions retentionPeriodDefined)
        {
            _sut.UpdateRetentionPeriodDefined(retentionPeriodDefined);
            var date = A<DateTime>();

            _sut.UpdateNextDataRetentionEvaluationDate(date);

            if (retentionPeriodDefined == DataOptions.YES)
            {
                Assert.Equal(date, _sut.DPIAdeleteDate);
            }
            else
            {
                Assert.Null(_sut.DPIAdeleteDate);
            }
        }

        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateDataRetentionEvaluationFrequencyInMonths_Updates_If_Retention_Period_Defined_Is_Yes(DataOptions retentionPeriodDefined)
        {
            _sut.UpdateRetentionPeriodDefined(retentionPeriodDefined);
            var frequency = A<int>();

            var result = _sut.UpdateDataRetentionEvaluationFrequencyInMonths(frequency);

            if (retentionPeriodDefined == DataOptions.YES)
            {
                Assert.True(result.IsNone);
                Assert.Equal(frequency, _sut.numberDPIA);
            }
            else
            {
                Assert.True(result.HasValue);
                Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
            }
        }

        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateDPIADocumentation_Updates_If_DPIA_Is_Yes(DataOptions dpia)
        {
            _sut.UpdateDPIAConducted(dpia);
            var url = A<string>();
            var name = A<string>();

            _sut.UpdateDPIADocumentation(url, name);

            if (dpia == DataOptions.YES)
            {
                Assert.Equal(url, _sut.DPIASupervisionDocumentationUrl);
                Assert.Equal(name, _sut.DPIASupervisionDocumentationUrlName);
            }
            else
            {
                Assert.Null(_sut.DPIASupervisionDocumentationUrl);
                Assert.Null(_sut.DPIASupervisionDocumentationUrlName);
            }
        }

        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateDPIADate_Updates_If_DPIA_Is_Yes(DataOptions dpia)
        {
            _sut.UpdateDPIAConducted(dpia);
            var date = A<DateTime>();

            _sut.UpdateDPIADate(date);

            if (dpia == DataOptions.YES)
            {
                Assert.Equal(date, _sut.DPIADateFor);
            }
            else
            {
                Assert.Null(_sut.DPIADateFor);
            }
        }


        [Theory]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        [InlineData(DataOptions.YES)]
        public void UpdateDPIAConducted_Only_Clears_Related_Fields_If_Not_Yes(DataOptions dpia)
        {
            var dpiaDateFor = A<DateTime>();
            var dpiaSupervisionDocumentationUrl = A<string>();
            var dpiaSupervisionDocumentationUrlName = A<string>();
            _sut.UpdateDPIAConducted(DataOptions.YES);
            _sut.UpdateDPIADate(dpiaDateFor);
            _sut.UpdateDPIADocumentation(dpiaSupervisionDocumentationUrl, dpiaSupervisionDocumentationUrlName);

            _sut.UpdateDPIAConducted(dpia);

            Assert.Equal(dpia, _sut.DPIA);
            if (dpia == DataOptions.YES)
            {
                Assert.Equal(dpiaDateFor, _sut.DPIADateFor);
                Assert.Equal(dpiaSupervisionDocumentationUrl, _sut.DPIASupervisionDocumentationUrl);
                Assert.Equal(dpiaSupervisionDocumentationUrlName, _sut.DPIASupervisionDocumentationUrlName);
            }
            else
            {
                Assert.Null(_sut.DPIADateFor);
                Assert.Null(_sut.DPIASupervisionDocumentationUrl);
                Assert.Null(_sut.DPIASupervisionDocumentationUrlName);
            }
        }

        [Theory]
        [InlineData(YesNoDontKnowIrrelevant.Yes)]
        [InlineData(YesNoDontKnowIrrelevant.DontKnow)]
        [InlineData(YesNoDontKnowIrrelevant.No)]
        [InlineData(YesNoDontKnowIrrelevant.Undecided)]
        [InlineData(YesNoDontKnowIrrelevant.Irrelevant)]
        public void UpdateRiskAssessmentDate_Updates_If_RiskAssessment_Is_Yes(YesNoDontKnowIrrelevant riskAssessment)
        {
            _sut.UpdateRiskAssessment(riskAssessment);
            var date = A<DateTime>();

            _sut.UpdateRiskAssessmentDate(date);

            if (riskAssessment == YesNoDontKnowIrrelevant.Yes)
            {
                Assert.Equal(date, _sut.riskAssesmentDate);
            }
            else
            {
                Assert.Null(_sut.riskAssesmentDate);
            }
        }

        [Theory]
        [InlineData(YesNoDontKnowIrrelevant.Yes)]
        [InlineData(YesNoDontKnowIrrelevant.DontKnow)]
        [InlineData(YesNoDontKnowIrrelevant.No)]
        [InlineData(YesNoDontKnowIrrelevant.Undecided)]
        [InlineData(YesNoDontKnowIrrelevant.Irrelevant)]
        public void UpdateRiskAssessmentNotes_Updates_If_RiskAssessment_Is_Yes(YesNoDontKnowIrrelevant riskAssessment)
        {
            _sut.UpdateRiskAssessment(riskAssessment);
            var notes = A<string>();

            _sut.UpdateRiskAssessmentNotes(notes);

            if (riskAssessment == YesNoDontKnowIrrelevant.Yes)
            {
                Assert.Equal(notes, _sut.noteRisks);
            }
            else
            {
                Assert.Null(_sut.noteRisks);
            }
        }

        [Fact]
        public void UpdatePlannedRiskAssessmentDate_Updates()
        {
            var date = A<DateTime>();

            _sut.UpdatePlannedRiskAssessmentDate(date);

            Assert.Equal(date, _sut.PlannedRiskAssessmentDate);
        }

        [Theory]
        [InlineData(YesNoDontKnowIrrelevant.Yes)]
        [InlineData(YesNoDontKnowIrrelevant.DontKnow)]
        [InlineData(YesNoDontKnowIrrelevant.No)]
        [InlineData(YesNoDontKnowIrrelevant.Undecided)]
        [InlineData(YesNoDontKnowIrrelevant.Irrelevant)]
        public void UpdateRiskAssessmentLevel_Updates_If_RiskAssessment_Is_Yes(YesNoDontKnowIrrelevant riskAssessment)
        {
            _sut.UpdateRiskAssessment(riskAssessment);
            var level = A<RiskLevel>();

            _sut.UpdateRiskAssessmentLevel(level);

            if (riskAssessment == YesNoDontKnowIrrelevant.Yes)
            {
                Assert.Equal(level, _sut.preriskAssessment);
            }
            else
            {
                Assert.Null(_sut.preriskAssessment);
            }
        }

        [Theory]
        [InlineData(YesNoDontKnowIrrelevant.Yes)]
        [InlineData(YesNoDontKnowIrrelevant.DontKnow)]
        [InlineData(YesNoDontKnowIrrelevant.No)]
        [InlineData(YesNoDontKnowIrrelevant.Undecided)]
        [InlineData(YesNoDontKnowIrrelevant.Irrelevant)]
        public void UpdateRiskAssessmentNote_Updates_If_RiskAssessment_Is_Yes(YesNoDontKnowIrrelevant riskAssessment)
        {
            _sut.UpdateRiskAssessment(riskAssessment);
            var note = A<string>();

            _sut.UpdateRiskAssessmentNotes(note);

            if (riskAssessment == YesNoDontKnowIrrelevant.Yes)
            {
                Assert.Equal(note, _sut.noteRisks);
            }
            else
            {
                Assert.Null(_sut.noteRisks);
            }
        }

        [Theory]
        [InlineData(YesNoDontKnowIrrelevant.Yes)]
        [InlineData(YesNoDontKnowIrrelevant.DontKnow)]
        [InlineData(YesNoDontKnowIrrelevant.No)]
        [InlineData(YesNoDontKnowIrrelevant.Undecided)]
        [InlineData(YesNoDontKnowIrrelevant.Irrelevant)]
        public void UpdateRiskAssessmentDocumentation_Updates_If_RiskAssessment_Is_Yes(YesNoDontKnowIrrelevant riskAssessment)
        {
            _sut.UpdateRiskAssessment(riskAssessment);
            var url = A<string>();
            var name = A<string>();


            _sut.UpdateRiskAssessmentDocumentation(url, name);

            if (riskAssessment == YesNoDontKnowIrrelevant.Yes)
            {
                Assert.Equal(url, _sut.RiskSupervisionDocumentationUrl);
                Assert.Equal(name, _sut.RiskSupervisionDocumentationUrlName);
            }
            else
            {
                Assert.Null(_sut.RiskSupervisionDocumentationUrl);
                Assert.Null(_sut.RiskSupervisionDocumentationUrlName);
            }
        }


        [Theory]
        [InlineData(YesNoDontKnowIrrelevant.No)]
        [InlineData(YesNoDontKnowIrrelevant.DontKnow)]
        [InlineData(YesNoDontKnowIrrelevant.Irrelevant)]
        [InlineData(YesNoDontKnowIrrelevant.Undecided)]
        [InlineData(YesNoDontKnowIrrelevant.Yes)]
        public void UpdateRiskAssessment_Only_Clears_Related_Fields_If_Not_Yes(YesNoDontKnowIrrelevant riskAssessment)
        {
            var level = A<RiskLevel>();
            var riskAssessmentDate = A<DateTime>();
            var url = A<string>();
            var urlName = A<string>();
            var notes = A<string>();

            _sut.UpdateRiskAssessment(YesNoDontKnowIrrelevant.Yes);
            _sut.UpdateRiskAssessmentLevel(level);
            _sut.UpdateRiskAssessmentDate(riskAssessmentDate);
            _sut.UpdateRiskAssessmentDocumentation(url, urlName);
            _sut.UpdateRiskAssessmentNotes(notes);

            _sut.UpdateRiskAssessment(riskAssessment);

            Assert.Equal(riskAssessment, _sut.riskAssessment);
            if (riskAssessment == YesNoDontKnowIrrelevant.Yes)
            {
                Assert.Equal(level, _sut.preriskAssessment);
                Assert.Equal(riskAssessmentDate, _sut.riskAssesmentDate);
                Assert.Equal(url, _sut.RiskSupervisionDocumentationUrl);
                Assert.Equal(urlName, _sut.RiskSupervisionDocumentationUrlName);
                Assert.Equal(notes, _sut.noteRisks);
            }
            else
            {
                Assert.Null(_sut.preriskAssessment);
                Assert.Null(_sut.riskAssesmentDate);
                Assert.Null(_sut.RiskSupervisionDocumentationUrl);
                Assert.Null(_sut.RiskSupervisionDocumentationUrlName);
                Assert.Null(_sut.noteRisks);
            }

        }

        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateTechnicalPrecautionsInPlace_Only_Clears_Related_Fields_If_Not_Yes(DataOptions precautionsInPlace)
        {
            var technicalPrecautions = A<List<TechnicalPrecaution>>();
            var url = A<string>();
            var name = A<string>();
            _sut.UpdateTechnicalPrecautionsInPlace(precautionsInPlace);
            _sut.UpdateTechnicalPrecautions(technicalPrecautions);
            _sut.UpdateTechnicalPrecautionsDocumentation(url, name);

            _sut.UpdateTechnicalPrecautionsInPlace(precautionsInPlace);

            if (precautionsInPlace == DataOptions.YES)
            {
                Assert.Equal(precautionsInPlace, _sut.precautions);
                AssertListsContainSameElements(technicalPrecautions, _sut.GetTechnicalPrecautions().ToList());
                Assert.Equal(url, _sut.TechnicalSupervisionDocumentationUrl);
                Assert.Equal(name, _sut.TechnicalSupervisionDocumentationUrlName);
            }
            else
            {
                Assert.Equal(precautionsInPlace, _sut.precautions);
                Assert.Empty(_sut.GetTechnicalPrecautions());
                Assert.Null(_sut.TechnicalSupervisionDocumentationUrlName);
                Assert.Null(_sut.TechnicalSupervisionDocumentationUrl);
            }
        }

        [Fact]
        public void UpdateTechnicalPrecautionsInPlace_Does_Not_Clear_Related_Fields_If_Yes()
        {
            const DataOptions precautionsInPlace = DataOptions.YES;
            var technicalPrecautions = A<List<TechnicalPrecaution>>();
            var url = A<string>();
            var name = A<string>();
            _sut.UpdateTechnicalPrecautionsInPlace(precautionsInPlace);
            _sut.UpdateTechnicalPrecautions(technicalPrecautions);
            _sut.UpdateTechnicalPrecautionsDocumentation(url, name);

            _sut.UpdateTechnicalPrecautionsInPlace(precautionsInPlace);

            Assert.Equal(precautionsInPlace, _sut.precautions);
            AssertListsContainSameElements(technicalPrecautions, _sut.GetTechnicalPrecautions().ToList());
            Assert.Equal(url, _sut.TechnicalSupervisionDocumentationUrl);
            Assert.Equal(name, _sut.TechnicalSupervisionDocumentationUrlName);
        }
        private static void AssertListsContainSameElements<T>(IList<T> expected, IList<T> actual)
        {
            if (expected.Count != actual.Count) Assert.Fail("Lists have different counts");
            var listsEqual = expected.OrderBy(x => x)
                .SequenceEqual(actual.OrderBy(x => x));
            Assert.True(listsEqual);
        }

        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateTechnicalPrecautions_Updates_If_Precautions_Is_Yes(DataOptions technicalPrecautionsInPlace)
        {
            _sut.UpdateTechnicalPrecautionsInPlace(technicalPrecautionsInPlace);
            var technicalPrecautions = A<List<TechnicalPrecaution>>();

            _sut.UpdateTechnicalPrecautions(technicalPrecautions);

            var actual = _sut.GetTechnicalPrecautions().ToList();
            if (technicalPrecautionsInPlace == DataOptions.YES)
            {
                AssertListsContainSameElements(technicalPrecautions, actual);
            }
            else
            {
                Assert.Empty(actual);
            }
        }


        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateTechnicalPrecautionsDocumentation_Updates_If_Precautions_Is_Yes(DataOptions technicalPrecautions)
        {
            _sut.UpdateTechnicalPrecautionsInPlace(technicalPrecautions);
            var url = A<string>();
            var name = A<string>();

            _sut.UpdateTechnicalPrecautionsDocumentation(url, name);

            if (technicalPrecautions == DataOptions.YES)
            {
                Assert.Equal(url, _sut.TechnicalSupervisionDocumentationUrl);
                Assert.Equal(name, _sut.TechnicalSupervisionDocumentationUrlName);
            }
            else
            {
                Assert.Null(_sut.TechnicalSupervisionDocumentationUrl);
                Assert.Null(_sut.TechnicalSupervisionDocumentationUrlName);
            }
        }

        [Theory]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateUserSupervision_Clears_Related_Fields_If_Not_Yes(DataOptions userSupervision)
        {
            _sut.UpdateUserSupervisionDate(A<DateTime>());
            _sut.UpdateTechnicalPrecautionsDocumentation(A<string>(), A<string>());

            _sut.UpdateUserSupervision(userSupervision);

            Assert.Equal(userSupervision, _sut.UserSupervision);
            Assert.Null(_sut.UserSupervisionDate);
            Assert.Null(_sut.UserSupervisionDocumentationUrl);
            Assert.Null(_sut.UserSupervisionDocumentationUrlName);
        }

        [Fact]
        public void UpdateUserSupervision_Does_Not_Clear_Related_Fields_If_Yes()
        {
            const DataOptions userSupervision = DataOptions.YES;
            var userSupervisionDate = A<DateTime?>();
            var userSupervisionDocumentation = A<NamedLink>();
            _sut.UpdateUserSupervision(userSupervision);
            _sut.UpdateUserSupervisionDate(userSupervisionDate);
            _sut.UpdateUserSupervisionDocumentation(userSupervisionDocumentation.Url, userSupervisionDocumentation.Name);

            _sut.UpdateUserSupervision(userSupervision);

            Assert.Equal(userSupervision, _sut.UserSupervision);
            Assert.Equal(userSupervisionDate, _sut.UserSupervisionDate);
            Assert.Equal(userSupervisionDocumentation.Url, _sut.UserSupervisionDocumentationUrl);
            Assert.Equal(userSupervisionDocumentation.Name, _sut.UserSupervisionDocumentationUrlName);
        }

        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateUserSupervisionDate_Updates_If_User_Supervision_Is_Yes(DataOptions userSupervision)
        {
            _sut.UpdateUserSupervision(userSupervision);
            var date = A<DateTime>();

            _sut.UpdateUserSupervisionDate(date);

            if (userSupervision == DataOptions.YES) Assert.Equal(date, _sut.UserSupervisionDate);
            else Assert.Null(_sut.UserSupervisionDate);
        }

        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateUserSupervisionDocumentation_Updates_If_User_Supervision_Is_Yes(DataOptions userSupervision)
        {
            _sut.UpdateUserSupervision(userSupervision);
            var url = A<string>();
            var name = A<string>();

            _sut.UpdateUserSupervisionDocumentation(url, name);

            if (userSupervision == DataOptions.YES)
            {
                Assert.Equal(url, _sut.UserSupervisionDocumentationUrl);
                Assert.Equal(name, _sut.UserSupervisionDocumentationUrlName);
            }
            else
            {
                Assert.Null(_sut.UserSupervisionDocumentationUrl);
                Assert.Null(_sut.UserSupervisionDocumentationUrlName);
            }
        }

        [Fact]
        public void AddUsageRelationTo_Throws_If_Destination_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.AddUsageRelationTo(null, Maybe<ItInterface>.None,
                A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, Maybe<ItContract>.None));
        }

        [Fact]
        public void Cannot_Add_Local_TaskRef_That_Is_Already_On_System()
        {
            var someKle = new TaskRef() { Uuid = A<Guid>() };
            var kleAdditions = new List<TaskRef>() { someKle };
            _sut.ItSystem = new ItSystem()
            {
                TaskRefs = kleAdditions
            };

            var result = _sut.UpdateKLEDeviations(kleAdditions, new List<TaskRef>());

            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
            Assert.Empty(_sut.TaskRefs);
        }

        [Fact]
        public void AddUsageRelationTo_Returns_Error_If_Destination_Equals_Self()
        {
            //Arrange
            var destination = new ItSystemUsage
            {
                Id = _sut.Id
            };

            //Act
            var result = _sut.AddUsageRelationTo(destination, Maybe<ItInterface>.None, A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, Maybe<ItContract>.None);

            //Assert
            AssertErrorResult(result, "'From' cannot equal 'To'", OperationFailure.BadInput);
        }

        [Fact]
        public void AddUsageRelationTo_Returns_Error_If_Organization_Destination_Is_Different()
        {
            //Arrange
            var destination = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = _sut.OrganizationId + 1
            };

            //Act
            var result = _sut.AddUsageRelationTo(destination, Maybe<ItInterface>.None, A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, Maybe<ItContract>.None);

            //Assert
            AssertErrorResult(result, "Attempt to create relation to it-system in a different organization", OperationFailure.BadInput);
        }

        [Fact]
        public void AddUsageRelationTo_Returns_Error_If_Contract_Organization_Is_Different()
        {
            //Arrange
            var destination = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = _sut.OrganizationId
            };

            var itContract = new ItContract { OrganizationId = _sut.OrganizationId + 1 };

            //Act
            var result = _sut.AddUsageRelationTo(destination, Maybe<ItInterface>.None, A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, itContract);

            //Assert
            AssertErrorResult(result, "Attempt to create relation to it contract in a different organization", OperationFailure.BadInput);
        }

        [Fact]
        public void AddUsageRelationTo_Returns_Error_If_Selected_Interface_Is_Not_Exposed_By_Target_System()
        {
            //Arrange
            var interfaceId = A<int>();

            var destination = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = _sut.OrganizationId,
                ItSystem = new ItSystem
                {
                    ItInterfaceExhibits =
                    {
                        new ItInterfaceExhibit
                        {
                            ItInterface = new ItInterface
                            {
                                Id = interfaceId + 1
                            }
                        }
                    }
                }
            };
            var itContract = new ItContract { OrganizationId = _sut.OrganizationId };


            //Act
            var result = _sut.AddUsageRelationTo(destination, new ItInterface() { Id = interfaceId }, A<string>(), A<string>(), Maybe<RelationFrequencyType>.None, itContract);

            //Assert
            AssertErrorResult(result, "Cannot set interface which is not exposed by the 'to' system", OperationFailure.BadInput);
        }

        [Fact]
        public void AddUsageRelationTo_Returns_Success_And_Adds_New_Relation()
        {
            //Arrange
            var interfaceId = A<int>();
            _sut.UsageRelations.Add(new SystemRelation(new ItSystemUsage()));
            var itInterface = new ItInterface
            {
                Id = interfaceId
            };
            var destination = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = _sut.OrganizationId,
                ItSystem = new ItSystem
                {
                    ItInterfaceExhibits =
                    {
                        new ItInterfaceExhibit
                        {
                            ItInterface = itInterface
                        }
                    }
                }
            };
            var itContract = new ItContract { OrganizationId = _sut.OrganizationId };
            var frequencyType = new RelationFrequencyType();
            var description = A<string>();
            var reference = A<string>();

            //Act
            var result = _sut.AddUsageRelationTo(destination, itInterface, description, reference, frequencyType, itContract);

            //Assert
            Assert.True(result.Ok);
            var newRelation = result.Value;
            Assert.True(_sut.UsageRelations.Contains(newRelation));
            Assert.Equal(2, _sut.UsageRelations.Count); //existing + the new one
            Assert.Equal(itContract, newRelation.AssociatedContract);
            Assert.Equal(frequencyType, newRelation.UsageFrequency);
            Assert.Equal(destination, newRelation.ToSystemUsage);
            Assert.Equal(description, newRelation.Description);
            Assert.NotNull(newRelation.Reference);
            Assert.Equal(reference, newRelation.Reference);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetUsageRelation_Returns(bool some)
        {
            //Arrange
            var id = A<int>();
            var systemRelation = new SystemRelation(new ItSystemUsage()) { Id = some ? id : A<int>() };
            var ignoredRelation = new SystemRelation(new ItSystemUsage()) { Id = A<int>() };
            _sut.UsageRelations.Add(ignoredRelation);
            _sut.UsageRelations.Add(systemRelation);

            //Act
            var relation = _sut.GetUsageRelation(id);

            //Assert
            Assert.Equal(some, relation.HasValue);
            if (some)
            {
                Assert.Same(systemRelation, relation.Value);
            }
        }

        [Fact]
        public void RemoveUsageRelation_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            var ignoredRelation = new SystemRelation(new ItSystemUsage()) { Id = A<int>() };
            _sut.UsageRelations.Add(ignoredRelation);

            //Act
            var result = _sut.RemoveUsageRelation(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void RemoveUsageRelation_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var removedRelation = new SystemRelation(new ItSystemUsage()) { Id = id };
            var ignoredRelation = new SystemRelation(new ItSystemUsage()) { Id = A<int>() };
            _sut.UsageRelations.Add(ignoredRelation);
            _sut.UsageRelations.Add(removedRelation);

            //Act
            var result = _sut.RemoveUsageRelation(id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(removedRelation, result.Value);
        }

        private static void AssertErrorResult(Result<SystemRelation, OperationError> result, string message, OperationFailure error)
        {
            Assert.False(result.Ok);
            var operationError = result.Error;
            Assert.Equal(error, operationError.FailureType);
            Assert.True(operationError.Message.HasValue);
            Assert.Equal(message, operationError.Message.Value);
        }

        [Theory]
        [InlineData(SensitiveDataLevel.NONE)]
        [InlineData(SensitiveDataLevel.PERSONALDATA)]
        [InlineData(SensitiveDataLevel.SENSITIVEDATA)]
        [InlineData(SensitiveDataLevel.LEGALDATA)]
        public void AddSensitiveData_Returns_Ok(SensitiveDataLevel sensitiveDataLevel)
        {
            //Act
            var result = _sut.AddSensitiveDataLevel(sensitiveDataLevel);

            //Assert
            Assert.True(result.Ok);
            var usageSensitiveDataLevel = result.Value;
            Assert.Equal(sensitiveDataLevel, usageSensitiveDataLevel.SensitivityDataLevel);
            Assert.Single(_sut.SensitiveDataLevels);

        }

        [Fact]
        public void AddSensitiveData_Fails_With_Conflict_If_Sensitivity_Level_Already_Exists()
        {
            //Arrange
            var preAddedSensitiveDataLevel = new ItSystemUsageSensitiveDataLevel()
            {
                SensitivityDataLevel = SensitiveDataLevel.NONE
            };
            _sut.SensitiveDataLevels.Add(preAddedSensitiveDataLevel);

            //Act
            var result = _sut.AddSensitiveDataLevel(SensitiveDataLevel.NONE);

            //Assert
            AssertErrorResult(result, "Data sensitivity level already exists", OperationFailure.Conflict);
        }

        [Theory]
        [InlineData(SensitiveDataLevel.NONE)]
        [InlineData(SensitiveDataLevel.PERSONALDATA)]
        [InlineData(SensitiveDataLevel.SENSITIVEDATA)]
        [InlineData(SensitiveDataLevel.LEGALDATA)]
        public void RemoveSensitiveData_Returns_Ok(SensitiveDataLevel sensitiveDataLevel)
        {
            //Arrange
            var preAddedSensitiveDataLevel = new ItSystemUsageSensitiveDataLevel()
            {
                ItSystemUsage = _sut,
                SensitivityDataLevel = sensitiveDataLevel
            };
            _sut.SensitiveDataLevels.Add(preAddedSensitiveDataLevel);

            //Act
            var result = _sut.RemoveSensitiveDataLevel(sensitiveDataLevel);

            //Assert
            Assert.True(result.Ok);
            var usageSensitiveDataLevel = result.Value;
            Assert.Equal(preAddedSensitiveDataLevel, usageSensitiveDataLevel.RemovedRiskLevel);
            Assert.Empty(_sut.SensitiveDataLevels);

        }

        [Fact]
        public void Remove_PersonalData_From_SensitiveData_Clears_PersonalData()
        {
            //Arrange
            var sensitiveDataLevel = SensitiveDataLevel.PERSONALDATA;
            var preAddedSensitiveDataLevel = new ItSystemUsageSensitiveDataLevel()
            {
                ItSystemUsage = _sut,
                SensitivityDataLevel = sensitiveDataLevel,
            };
            _sut.SensitiveDataLevels.Add(preAddedSensitiveDataLevel);

            _sut.PersonalDataOptions.Add(new ItSystemUsagePersonalData { PersonalData = GDPRPersonalDataOption.CprNumber });
            _sut.PersonalDataOptions.Add(new ItSystemUsagePersonalData() { PersonalData = GDPRPersonalDataOption.OtherPrivateMatters });
            _sut.PersonalDataOptions.Add(new ItSystemUsagePersonalData() { PersonalData = GDPRPersonalDataOption.SocialProblems });

            //Act
            var result = _sut.RemoveSensitiveDataLevel(sensitiveDataLevel);

            //Assert
            Assert.True(result.Ok);
            var usageSensitiveDataLevel = result.Value;
            Assert.Equal(preAddedSensitiveDataLevel, usageSensitiveDataLevel.RemovedRiskLevel);
            Assert.Empty(_sut.SensitiveDataLevels);
            Assert.Empty(_sut.PersonalDataOptions);
        }

        [Fact]
        public void RemoveSensitiveData_Fails_With_NotFound_If_Sensitivity_Level_Does_Not_Exist()
        {
            //Act
            var result = _sut.RemoveSensitiveDataLevel(SensitiveDataLevel.NONE);

            //Assert
            AssertErrorResult(result, "Data sensitivity does not exists on system usage", OperationFailure.NotFound);
        }

        [Fact]
        public void Can_Remove_OrganizationalUsage()
        {
            var unitId = A<int>();
            var responsibleUsage = new ItSystemUsageOrgUnitUsage() { OrganizationUnitId = unitId, OrganizationUnit = new OrganizationUnit { Id = unitId } };
            var usage = new ItSystemUsage()
            {
                UsedBy = new List<ItSystemUsageOrgUnitUsage> { responsibleUsage },
                ResponsibleUsage = responsibleUsage,
            };

            var result = usage.RemoveResponsibleOrganizationUnit();

            Assert.False(result.HasValue);
            Assert.Null(usage.ResponsibleUsage);
        }

        [Fact]
        public void Can_Remove_UsedByUnit()
        {
            var unitUuid = A<Guid>();
            var unit = new OrganizationUnit { Uuid = unitUuid };
            var usage = new ItSystemUsage
            {
                UsedBy = new List<ItSystemUsageOrgUnitUsage> { new() { OrganizationUnit = unit } },
                Organization = new Organization
                {
                    OrgUnits = new List<OrganizationUnit> { unit }
                }
            };

            var result = usage.RemoveUsedByUnit(unitUuid);

            Assert.False(result.HasValue);
            Assert.Empty(usage.UsedBy);
        }

        [Fact]
        public void Can_Transfer_OrganizationalUsage()
        {
            var unitUuid = A<Guid>();
            var targetUnitUuid = A<Guid>();
            var unit = new OrganizationUnit { Uuid = unitUuid };
            var targetUnit = new OrganizationUnit { Uuid = targetUnitUuid };
            var responsibleUsage = new ItSystemUsageOrgUnitUsage { OrganizationUnit = unit };
            var usage = new ItSystemUsage
            {
                UsedBy = new List<ItSystemUsageOrgUnitUsage> { responsibleUsage },
                ResponsibleUsage = responsibleUsage,
                Organization = new Organization { OrgUnits = new List<OrganizationUnit> { unit, targetUnit } }
            };

            var result = usage.TransferResponsibleOrganizationalUnit(targetUnitUuid);

            Assert.False(result.HasValue);
            Assert.Equal(targetUnit.Id, usage.ResponsibleUsage.OrganizationUnit.Id);
        }

        [Fact]
        public void Can_Transfer_UsedByUnit()
        {
            var unitUuid = A<Guid>();
            var targetUnitUuid = A<Guid>();
            var unit = new OrganizationUnit { Uuid = unitUuid };
            var targetUnit = new OrganizationUnit { Uuid = targetUnitUuid };
            var usage = new ItSystemUsage
            {
                Organization = new Organization()
                {
                    OrgUnits = new List<OrganizationUnit> { unit, targetUnit }
                },
                UsedBy = new List<ItSystemUsageOrgUnitUsage> { new() { OrganizationUnit = unit } }
            };

            var result = usage.TransferUsedByUnit(unitUuid, targetUnitUuid);

            Assert.False(result.HasValue);
            Assert.DoesNotContain(unitUuid, usage.UsedBy.Select(x => x.OrganizationUnit.Uuid));
            Assert.Contains(targetUnitUuid, usage.UsedBy.Select(x => x.OrganizationUnit.Uuid));
        }

        [Theory]
        [InlineData(null, 9, UserCount.BELOWTEN)]
        [InlineData(0, 9, UserCount.BELOWTEN)]
        [InlineData(10, 49, UserCount.TENTOFORTYNINE)]
        [InlineData(50, 99, UserCount.FIFTYTONINETYNINE)]
        [InlineData(100, 499, UserCount.HUNDREDTOFOURHUNDREDNINETYNINE)]
        [InlineData(500, null, UserCount.FIVEHUNDREDPLUS)]
        [InlineData(500, int.MaxValue, UserCount.FIVEHUNDREDPLUS)]
        public void Can_Change_UserCount(int? lower, int? upper, UserCount expectedResult)
        {
            //Arrange
            var newIntervalValue = (lower, upper);

            //Act
            var error = _sut.SetExpectedUsersInterval(newIntervalValue);

            //Assert
            Assert.True(error.IsNone);
            Assert.Equal(expectedResult, _sut.UserCount);
        }

        [Theory]
        [MemberData(nameof(ValidationInvalidData))]
        public void Invalid_When_LifeCycleStatus_Or_MainContract_Invalid(LifeCycleStatusType? lifeCycleStatus, ItContractItSystemUsage? mainContract, List<ItSystemUsageValidationError> expectedErrors)
        {
            var itSystemUsage = new ItSystemUsage
            {
                LifeCycleStatus = lifeCycleStatus,
                MainContract = mainContract
            };

            var validity = itSystemUsage.CheckSystemValidity();

            Assert.False(validity.Result);
            Assert.Equal(expectedErrors, validity.ValidationErrors);
        }

        [Theory]
        [MemberData(nameof(DateValidationInvalidData))]
        public void Invalid_When_Dates_Invalid(DateTime startDate, DateTime endDate, List<ItSystemUsageValidationError> expectedErrors)
        {
            var itSystemUsage = new ItSystemUsage
            {
                Concluded = startDate,
                ExpirationDate = endDate
            };

            var validity = itSystemUsage.CheckSystemValidity();

            Assert.False(validity.Result);
            Assert.Equal(expectedErrors, validity.ValidationErrors);
        }

        [Theory]
        [MemberData(nameof(ValidationValidData))]
        public void Valid_When_All_Valid(LifeCycleStatusType lifeCycleStatus, DateTime concluded, DateTime expirationDate, ItContractItSystemUsage? mainContract)
        {
            var itSystemUsage = new ItSystemUsage
            {
                LifeCycleStatus = lifeCycleStatus,
                Concluded = concluded,
                ExpirationDate = expirationDate,
                MainContract = mainContract
            };

            var validity = itSystemUsage.CheckSystemValidity();
            Assert.True(validity.Result);
        }

        [Fact]
        public void Valid_When_All_Null()
        {
            var itSystemUsage = new ItSystemUsage();

            var validity = itSystemUsage.CheckSystemValidity();

            Assert.True(validity.Result);
        }

        [Fact]
        public void Invalid_When_StartDate_Not_Passed_And_EndDate_Is_Also_In_Future()
        {
            var itSystemUsage = new ItSystemUsage
            {
                Concluded = DateTime.UtcNow.AddDays(1),
                ExpirationDate = DateTime.UtcNow.AddDays(30)
            };

            var validity = itSystemUsage.CheckSystemValidity();

            Assert.False(validity.Result);
            Assert.Equal(new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.StartDateNotPassed }, validity.ValidationErrors);
        }

        [Fact]
        public void Invalid_When_EndDate_Passed_And_LifeCycle_Is_Invalid()
        {
            var itSystemUsage = new ItSystemUsage
            {
                ExpirationDate = DateTime.UtcNow.AddDays(-1),
                LifeCycleStatus = LifeCycleStatusType.NotInUse
            };

            var validity = itSystemUsage.CheckSystemValidity();

            Assert.False(validity.Result);
            Assert.Equal(
                new List<ItSystemUsageValidationError>
                {
                    ItSystemUsageValidationError.EndDatePassed,
                    ItSystemUsageValidationError.NotOperationalAccordingToLifeCycle
                },
                validity.ValidationErrors);
        }

        [Fact]
        public void Invalid_When_EndDate_Passed_And_MainContract_Is_Inactive()
        {
            var itSystemUsage = new ItSystemUsage
            {
                ExpirationDate = DateTime.UtcNow.AddDays(-1),
                MainContract = new ItContractItSystemUsage { ItContract = new ItContract { Terminated = DateTime.UtcNow.AddDays(-1) } }
            };

            var validity = itSystemUsage.CheckSystemValidity();

            Assert.False(validity.Result);
            Assert.Equal(
                new List<ItSystemUsageValidationError>
                {
                    ItSystemUsageValidationError.EndDatePassed,
                    ItSystemUsageValidationError.MainContractNotActive
                },
                validity.ValidationErrors);
        }

        [Theory]
        [InlineData(LifeCycleStatusType.NotInUse)]
        [InlineData(LifeCycleStatusType.Pilot)]
        public void Invalid_When_All_Validation_Sources_Have_Errors(LifeCycleStatusType status)
        {
            var itSystemUsage = new ItSystemUsage
            {
                ExpirationDate = DateTime.UtcNow.AddDays(-1),
                LifeCycleStatus = status,
                MainContract = new ItContractItSystemUsage { ItContract = new ItContract { Terminated = DateTime.UtcNow.AddDays(-1) } }
            };

            var validity = itSystemUsage.CheckSystemValidity();

            Assert.False(validity.Result);
            Assert.Equal(
                new List<ItSystemUsageValidationError>
                {
                    ItSystemUsageValidationError.EndDatePassed,
                    ItSystemUsageValidationError.NotOperationalAccordingToLifeCycle,
                    ItSystemUsageValidationError.MainContractNotActive
                },
                validity.ValidationErrors);
        }

        [Fact]
        public void Valid_When_MainContract_Is_Set_But_ItContract_Is_Null()
        {
            var itSystemUsage = new ItSystemUsage
            {
                MainContract = new ItContractItSystemUsage { ItContract = null }
            };

            var validity = itSystemUsage.CheckSystemValidity();

            Assert.True(validity.Result);
        }

        [Fact]
        public void AddPersonalData_Adds_PersonalData()
        {
            //Arrange
            var itSystemUsage = CreateItSystemUsageWithSensitiveLevelPersonalData();
            var personalDataOption = A<GDPRPersonalDataOption>();

            //Act
            var result = itSystemUsage.AddPersonalData(personalDataOption);

            //Assert
            Assert.False(result.Failed);
            Assert.Contains(personalDataOption, itSystemUsage.PersonalDataOptions.Select(x => x.PersonalData));
        }

        [Fact]
        public void AddPersonalData_Returns_BadState_If_SensitiveDataPersonalData_Not_Present()
        {
            //Arrange
            var personalDataOption = A<GDPRPersonalDataOption>();

            //Act
            var result = _sut.AddPersonalData(personalDataOption);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
        }

        [Fact]
        public void AddPersonalData_Returns_Conflict_If_Data_Already_Present()
        {
            //Arrange
            var personalDataOption = A<GDPRPersonalDataOption>();
            var itSystemUsage = CreateItSystemUsageWithSensitiveLevelPersonalData(personalDataOption);

            //Act
            var result = itSystemUsage.AddPersonalData(personalDataOption);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
        }

        [Fact]
        public void RemovePersonalData_Removes_PersonalData()
        {
            //Arrange
            var personalDataOption = A<GDPRPersonalDataOption>();
            var itSystemUsage = CreateItSystemUsageWithSensitiveLevelPersonalData(personalDataOption);

            //Act
            var result = itSystemUsage.RemovePersonalData(personalDataOption);

            //Assert
            Assert.False(result.Failed);
            Assert.DoesNotContain(personalDataOption, itSystemUsage.PersonalDataOptions.Select(x => x.PersonalData));
            Assert.Equal(personalDataOption, result.Value.PersonalData);
        }

        [Fact]
        public void RemovePersonalData_Returns_NotFound()
        {
            //Arrange
            var personalDataOption = A<GDPRPersonalDataOption>();
            var itSystemUsage = CreateItSystemUsageWithSensitiveLevelPersonalData();

            //Act
            var result = itSystemUsage.RemovePersonalData(personalDataOption);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        public static readonly object[][] ValidationInvalidData =
        {
            [
                LifeCycleStatusType.NotInUse, null!,
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.NotOperationalAccordingToLifeCycle }
            ],
            [
                LifeCycleStatusType.Pilot, null!,
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.NotOperationalAccordingToLifeCycle }
            ],
            [
                (LifeCycleStatusType?)null!, new ItContractItSystemUsage {ItContract = new ItContract {Terminated = DateTime.UtcNow.AddDays(-1)}},
                new List<ItSystemUsageValidationError> {ItSystemUsageValidationError.MainContractNotActive}
            ],
            [
                LifeCycleStatusType.NotInUse, new ItContractItSystemUsage {ItContract = new ItContract {Terminated = DateTime.UtcNow.AddDays(-1)}},
                new List<ItSystemUsageValidationError> {ItSystemUsageValidationError.NotOperationalAccordingToLifeCycle, ItSystemUsageValidationError.MainContractNotActive}
            ],
        };

        public static readonly object[][] DateValidationInvalidData =
        {
            [
                DateTime.UtcNow.AddDays(-1), null!,
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.EndDatePassed }
            ],
            [
                null!, DateTime.UtcNow.AddDays(-1),
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.EndDatePassed }
            ],
            [
                DateTime.UtcNow.AddDays(1), null!,
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.StartDateNotPassed, ItSystemUsageValidationError.EndDatePassed }
            ]
        };

        public static readonly object[][] ValidationValidData =
        {
            new object[] {LifeCycleStatusType.Undecided, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null!},
            new object[] {LifeCycleStatusType.Operational, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null!},
            new object[] {LifeCycleStatusType.PhasingIn, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null!},
            new object[] {LifeCycleStatusType.PhasingOut, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null!},
            new object[] {null!, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null!},
            new object[] {null!, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), new ItContractItSystemUsage{ ItContract = new ItContract{ Active = true} }},
            new object[] { LifeCycleStatusType.PhasingOut, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), new ItContractItSystemUsage{ ItContract = new ItContract{ Active = true} }},
        };

        private static void AssertErrorResult<T>(Result<T, OperationError> result, string message, OperationFailure error)
        {
            Assert.False(result.Ok);
            var operationError = result.Error;
            Assert.Equal(error, operationError.FailureType);
            Assert.True(operationError.Message.HasValue);
            Assert.Equal(message, operationError.Message.Value);
        }

        private static ItSystemUsage CreateItSystemUsageWithSensitiveLevelPersonalData(GDPRPersonalDataOption? personalDataOption = null)
        {
            var usage = new ItSystemUsage
            {
                SensitiveDataLevels = new List<ItSystemUsageSensitiveDataLevel>
                {
                    new() {SensitivityDataLevel = SensitiveDataLevel.PERSONALDATA}
                }
            };

            if (personalDataOption.HasValue)
            {
                usage.PersonalDataOptions = new List<ItSystemUsagePersonalData> { new() { PersonalData = personalDataOption.Value } };
            }

            return usage;
        }

        [Fact]
        public void MainContractState_Returns_NoContract_When_No_MainContract_Assigned()
        {
            //Arrange
            var usage = new ItSystemUsage
            {
                MainContract = null
            };

            //Act
            var state = usage.MainContractState;

            //Assert
            Assert.Equal(MainContractState.NoContract, state);
        }

        [Fact]
        public void MainContractState_Returns_Active_When_MainContract_Is_Active()
        {
            //Arrange
            var usage = new ItSystemUsage
            {
                MainContract = new ItContractItSystemUsage
                {
                    ItContract = new ItContract
                    {
                        Active = true
                    }
                }
            };

            //Act
            var state = usage.MainContractState;

            //Assert
            Assert.Equal(MainContractState.Active, state);
        }

        [Fact]
        public void MainContractState_Returns_Inactive_When_MainContract_Exists_But_Is_Not_Active()
        {
            //Arrange
            var usage = new ItSystemUsage
            {
                MainContract = new ItContractItSystemUsage
                {
                    ItContract = new ItContract
                    {
                        Active = false,
                        ExpirationDate = DateTime.UtcNow.AddDays(-1) // Expired contract
                    }
                }
            };

            //Act
            var state = usage.MainContractState;

            //Assert
            Assert.Equal(MainContractState.Inactive, state);
        }

        [Fact]
        public void MainContractState_Returns_Active_When_MainContract_Has_No_Validity_Constraints()
        {
            //Arrange
            var usage = new ItSystemUsage
            {
                MainContract = new ItContractItSystemUsage
                {
                    ItContract = new ItContract
                    {
                        // No dates set, so contract is valid by default
                        Concluded = null,
                        ExpirationDate = null
                    }
                }
            };

            //Act
            var state = usage.MainContractState;

            //Assert
            Assert.Equal(MainContractState.Active, state);
        }

        [Fact]
        public void MainContractState_Returns_Inactive_When_MainContract_Is_Expired()
        {
            //Arrange
            var usage = new ItSystemUsage
            {
                MainContract = new ItContractItSystemUsage
                {
                    ItContract = new ItContract
                    {
                        Active = false,
                        ExpirationDate = DateTime.UtcNow.AddDays(-1)
                    }
                }
            };

            //Act
            var state = usage.MainContractState;

            //Assert
            Assert.Equal(MainContractState.Inactive, state);
        }

        [Fact]
        public void ResetTechnicalSystemTypes_ClearsCollection()
        {
            _sut.UpdateTechnicalSystemTypes(new List<TechnicalSystemType> { new TechnicalSystemType { Id = A<int>(), Uuid = Guid.NewGuid() } });
            Assert.NotEmpty(_sut.TechnicalSystemTypes);

            _sut.ResetTechnicalSystemTypes();

            Assert.Empty(_sut.TechnicalSystemTypes);
        }

        [Fact]
        public void UpdateTechnicalSystemTypes_SetsValues_WhenNew()
        {
            var newValues = new List<TechnicalSystemType> { new TechnicalSystemType { Id = A<int>(), Uuid = Guid.NewGuid() } };

            var error = _sut.UpdateTechnicalSystemTypes(newValues);

            Assert.True(error.IsNone);
            Assert.Single(_sut.TechnicalSystemTypes);
        }

        [Fact]
        public void UpdateTechnicalSystemTypes_SetsValues_WhenDifferentIds()
        {
            var initial = new List<TechnicalSystemType> { new TechnicalSystemType { Id = A<int>(), Uuid = Guid.NewGuid() } };
            _sut.UpdateTechnicalSystemTypes(initial);

            var newValues = new List<TechnicalSystemType> { new TechnicalSystemType { Id = A<int>(), Uuid = Guid.NewGuid() } };
            var error = _sut.UpdateTechnicalSystemTypes(newValues);

            Assert.True(error.IsNone);
            Assert.Single(_sut.TechnicalSystemTypes);
            Assert.Equal(newValues[0].Id, _sut.TechnicalSystemTypes.First().Id);
        }

        [Fact]
        public void UpdateTechnicalSystemTypes_DoesNotChangeValues_WhenSameIds()
        {
            var existing = new TechnicalSystemType { Id = A<int>(), Uuid = Guid.NewGuid() };
            _sut.UpdateTechnicalSystemTypes(new List<TechnicalSystemType> { existing });

            var sameIdValues = new List<TechnicalSystemType> { new TechnicalSystemType { Id = existing.Id, Uuid = existing.Uuid } };
            var error = _sut.UpdateTechnicalSystemTypes(sameIdValues);

            Assert.True(error.IsNone);
            Assert.Single(_sut.TechnicalSystemTypes);
        }

        [Fact]
        public void UpdateTechnicalSystemTypes_Throws_WhenNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.UpdateTechnicalSystemTypes(null));
        }

        private void AssertCriticalityFieldsLastChanged(DateTime? beforeChange, bool expectChange = true)
        {
            if (expectChange)
            {
                Assert.True(_sut.CriticalityFieldsLastChanged > beforeChange);
            }
            else
            {
                Assert.Equal(_sut.CriticalityFieldsLastChanged, beforeChange);
            }
        }
    }
}

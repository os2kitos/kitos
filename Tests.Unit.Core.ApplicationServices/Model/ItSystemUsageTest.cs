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
                Assert.Equal(result.Value.FailureType, OperationFailure.BadInput);
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
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateRiskAssessmentDate_Updates_If_RiskAssessment_Is_Yes(DataOptions riskAssessment)
        {
            _sut.UpdateRiskAssessment(riskAssessment);
            var date = A<DateTime>();

            _sut.UpdateRiskAssessmentDate(date);

            if (riskAssessment == DataOptions.YES)
            {
                Assert.Equal(date, _sut.riskAssesmentDate);
            }
            else
            {
                Assert.Null(_sut.riskAssesmentDate);
            }
        }

        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateRiskAssessmentNotes_Updates_If_RiskAssessment_Is_Yes(DataOptions riskAssessment)
        {
            _sut.UpdateRiskAssessment(riskAssessment);
            var notes = A<string>();

            _sut.UpdateRiskAssessmentNotes(notes);

            if (riskAssessment == DataOptions.YES)
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
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateRiskAssessmentLevel_Updates_If_RiskAssessment_Is_Yes(DataOptions riskAssessment)
        {
            _sut.UpdateRiskAssessment(riskAssessment);
            var level = A<RiskLevel>();

            _sut.UpdateRiskAssessmentLevel(level);

            if (riskAssessment == DataOptions.YES)
            {
                Assert.Equal(level, _sut.preriskAssessment);
            }
            else
            {
                Assert.Null(_sut.preriskAssessment);
            }
        }

        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateRiskAssessmentNote_Updates_If_RiskAssessment_Is_Yes(DataOptions riskAssessment)
        {
            _sut.UpdateRiskAssessment(riskAssessment);
            var note = A<string>();

            _sut.UpdateRiskAssessmentNotes(note);

            if (riskAssessment == DataOptions.YES)
            {
                Assert.Equal(note, _sut.noteRisks);
            }
            else
            {
                Assert.Null(_sut.noteRisks);
            }
        }

        [Theory]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateRiskAssessmentDocumentation_Updates_If_RiskAssessment_Is_Yes(DataOptions riskAssessment)
        {
            _sut.UpdateRiskAssessment(riskAssessment);
            var url = A<string>();
            var name = A<string>();


            _sut.UpdateRiskAssessmentDocumentation(url, name);

            if (riskAssessment == DataOptions.YES)
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
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.YES)]
        [InlineData(DataOptions.UNDECIDED)]
        public void UpdateRiskAssessment_Only_Clears_Related_Fields_If_Not_Yes(DataOptions riskAssessment)
        {
            var level = A<RiskLevel>();
            var riskAssessmentDate = A<DateTime>();
            var url = A<string>();
            var urlName = A<string>();
            var notes = A<string>();

            _sut.UpdateRiskAssessment(DataOptions.YES);
            _sut.UpdateRiskAssessmentLevel(level);
            _sut.UpdateRiskAssessmentDate(riskAssessmentDate);
            _sut.UpdateRiskAssessmentDocumentation(url, urlName);
            _sut.UpdateRiskAssessmentNotes(notes);

            _sut.UpdateRiskAssessment(riskAssessment);

            Assert.Equal(riskAssessment, _sut.riskAssessment);
            if (riskAssessment == DataOptions.YES)
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
        [InlineData(DataOptions.DONTKNOW)]
        [InlineData(DataOptions.NO)]
        [InlineData(DataOptions.YES)]
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
        [InlineData(0, 9, UserCount.BELOWTEN)]
        [InlineData(10, 50, UserCount.TENTOFIFTY)]
        [InlineData(50, 100, UserCount.FIFTYTOHUNDRED)]
        [InlineData(100, null, UserCount.HUNDREDPLUS)]
        [InlineData(100, int.MaxValue, UserCount.HUNDREDPLUS)]
        public void Can_Change_UserCount(int lower, int? upper, UserCount expectedResult)
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
        public void Invalid_When_LifeCycleStatus_Or_MainContract_Invalid(LifeCycleStatusType lifeCycleStatus, ItContractItSystemUsage mainContract, List<ItSystemUsageValidationError> expectedErrors)
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
        public void Valid_When_All_Valid(LifeCycleStatusType lifeCycleStatus, DateTime concluded, DateTime expirationDate, ItContractItSystemUsage mainContract)
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
            new object[]
            {
                LifeCycleStatusType.NotInUse, null,
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.NotOperationalAccordingToLifeCycle }
            },
            new object[]
            {
                null, new ItContractItSystemUsage {ItContract = new ItContract {Terminated = DateTime.UtcNow.AddDays(-1)}},
                new List<ItSystemUsageValidationError> {ItSystemUsageValidationError.MainContractNotActive}
            },
            new object[]
            {
                LifeCycleStatusType.NotInUse, new ItContractItSystemUsage {ItContract = new ItContract {Terminated = DateTime.UtcNow.AddDays(-1)}},
                new List<ItSystemUsageValidationError> {ItSystemUsageValidationError.NotOperationalAccordingToLifeCycle, ItSystemUsageValidationError.MainContractNotActive}
            },
        };

        public static readonly object[][] DateValidationInvalidData =
        {
            new object[]
            {
                DateTime.UtcNow.AddDays(-1), null,
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.EndDatePassed }
            },
            new object[]
            {
                null, DateTime.UtcNow.AddDays(-1),
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.EndDatePassed }
            },
            new object[]
            {
                DateTime.UtcNow.AddDays(1), null,
                new List<ItSystemUsageValidationError> { ItSystemUsageValidationError.StartDateNotPassed, ItSystemUsageValidationError.EndDatePassed }
            }
        };

        public static readonly object[][] ValidationValidData =
        {
            new object[] {LifeCycleStatusType.Undecided, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null},
            new object[] {LifeCycleStatusType.Operational, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null},
            new object[] {LifeCycleStatusType.PhasingIn, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null},
            new object[] {LifeCycleStatusType.PhasingOut, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null},
            new object[] {null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), null},
            new object[] {null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), new ItContractItSystemUsage{ ItContract = new ItContract{ Active = true} }},
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
    }
}

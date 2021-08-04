using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.KLE;
using Infrastructure.Services.Types;
using Moq;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.KLE.V2
{
    [Collection(nameof(SequenceExtensions)), Description("We are messing with the global KLE numbers so this one cannot run in parallel with other tests")]
    public class KleOptionV2ApiTest : WithAutoFixture
    {
        private const string TestKeyPrefix = "KV2";

        [Fact]
        public async Task Can_Get_Kle_Numbers()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var date = A<DateTime>().Date;
            SetLatestUpdatedDate(date);

            //Act
            var dto = await KleOptionV2Helper.GetKleNumbersAsync(token.Token);

            //Assert
            Assert.Equal(date, dto.ReferenceVersion);
            Assert.NotEmpty(dto.Payload);
        }

        [Fact]
        public async Task Can_Get_Kle_Numbers_By_Parent_Id()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);
            PurgePreviousTestData();
            var parent = CreateTaskRefInDb(100, 10);
            var child1 = CreateTaskRefInDb(100, 10, 100, parentId: parent.Id);
            var child2 = CreateTaskRefInDb(100, 10, 101, parentId: parent.Id);
            CreateTaskRefInDb(200, 10, 102); //this one should not be matched

            //Act
            var result = await KleOptionV2Helper.GetKleNumbersAsync(token.Token, parentIdQuery: parent.Uuid);

            //Assert
            var payload = result.Payload.ToList();
            Assert.Equal(2, payload.Count);
            Assert.Contains(payload, x => x.Uuid == child1.Uuid);
            Assert.Contains(payload, x => x.Uuid == child2.Uuid);
        }

        [Fact]
        public async Task Cannot_Get_Kle_Numbers_By_Parent_Id_If_Parent_Id_Is_Empty_Guid()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var result = await KleOptionV2Helper.SendGetKleNumbersAsync(token.Token, parentIdQuery: Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Can_Get_Kle_Numbers_By_Parent_Key()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);
            PurgePreviousTestData();
            var parent = CreateTaskRefInDb(100, 10);
            var child1 = CreateTaskRefInDb(100, 10, 100, parentId: parent.Id);
            var child2 = CreateTaskRefInDb(100, 10, 101, parentId: parent.Id);
            CreateTaskRefInDb(200, 10, 102); //this one should not be matched

            //Act
            var result = await KleOptionV2Helper.GetKleNumbersAsync(token.Token, parentKeyQuery: parent.TaskKey);

            //Assert
            var payload = result.Payload.ToList();
            Assert.Equal(2, payload.Count);
            Assert.Contains(payload, x => x.Uuid == child1.Uuid);
            Assert.Contains(payload, x => x.Uuid == child2.Uuid);
        }

        [Fact]
        public async Task Can_Get_Kle_Numbers_By_Key_Prefix()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);
            PurgePreviousTestData();
            var match1 = CreateTaskRefInDb(100, 10, 100);
            var match2 = CreateTaskRefInDb(100, 15, 100);
            CreateTaskRefInDb(100, 20, 101);

            //Act
            var result = await KleOptionV2Helper.GetKleNumbersAsync(token.Token, kleNumberPrefix: $"{TestKeyPrefix}100.1");

            //Assert
            var payload = result.Payload.ToList();
            Assert.Equal(2, payload.Count);
            Assert.Contains(payload, x => x.Uuid == match1.Uuid);
            Assert.Contains(payload, x => x.Uuid == match2.Uuid);
        }

        [Fact]
        public async Task Can_Get_Kle_Numbers_By_Description_Content()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var content = A<string>();
            PurgePreviousTestData();
            var match1 = CreateTaskRefInDb(100, 10, 100, description: $"{content}{A<string>()}");
            var match2 = CreateTaskRefInDb(100, 15, 100, description: $"{A<string>()}{content}{A<string>()}");
            CreateTaskRefInDb(100, 20, 101);

            //Act
            var result = await KleOptionV2Helper.GetKleNumbersAsync(token.Token, kleDescriptionContent: content);

            //Assert
            var payload = result.Payload.ToList();
            Assert.Equal(2, payload.Count);
            Assert.Contains(payload, x => x.Uuid == match1.Uuid);
            Assert.Contains(payload, x => x.Uuid == match2.Uuid);
        }

        [Fact]
        public async Task Can_Get_Specific_Kle()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);
            PurgePreviousTestData();
            var parent = CreateTaskRefInDb(100, 10);
            var child = CreateTaskRefInDb(100, 10, 100, parentId: parent.Id);

            //Act
            var response = await KleOptionV2Helper.GetKleNumberAsync(token.Token, child.Uuid);

            //Assert
            Assert.Equal(child.Uuid, response.Payload.Uuid);
            Assert.Equal(child.TaskKey, response.Payload.KleNumber);
            Assert.Equal(child.Parent.Uuid, response.Payload.ParentKle.Uuid);
            Assert.Equal(child.Parent.TaskKey, response.Payload.ParentKle.Name);
            Assert.Equal(child.Description, response.Payload.Description);
        }

        [Fact]
        public async Task Cannot_Get_Unknown_Kle()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await KleOptionV2Helper.SendGetKleNumberAsync(token.Token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private TaskRef CreateTaskRefInDb(int mainGroup, int? @group = null, int? subject = null, string description = null, int? parentId = null)
        {
            if (subject.HasValue && !group.HasValue)
                throw new ArgumentException($"{nameof(@group)} must be defined if {nameof(subject)} is defined.");

            var key = $"{TestKeyPrefix}{mainGroup:D}";
            if (group.HasValue)
                key = $"{key}.{group.Value:D}";
            if (subject.HasValue)
                key = $"{key}.{subject.Value:D}";

            DatabaseAccess.MutateEntitySet<TaskRef>(refs => refs.Insert(new TaskRef()
            {
                ObjectOwnerId = TestEnvironment.DefaultUserId,
                LastChangedByUserId = TestEnvironment.DefaultUserId,
                OwnedByOrganizationUnitId = 1,
                TaskKey = key,
                Description = description,
                ParentId = parentId,
                Uuid = Guid.NewGuid()
            }));

            return DatabaseAccess.MapFromEntitySet<TaskRef, TaskRef>(refs => refs.AsQueryable().Include(x => x.Parent).AsNoTracking().Single(x => x.TaskKey == key));
        }

        private static void PurgePreviousTestData()
        {
            DatabaseAccess.MutateEntitySet<TaskRef>(taskRefs =>
                taskRefs
                    .AsQueryable()
                    .Transform(new QueryByKeyPrefix(TestKeyPrefix).Apply)
                    .ToList()
                    .ForEach(taskRefs.Delete)
                );
        }

        private static void SetLatestUpdatedDate(DateTime date)
        {
            DatabaseAccess.MutateEntitySet<KLEUpdateHistoryItem>(items =>
            {
                items.AsQueryable().ToList().ForEach(existing => items.DeleteByKey(existing.Id));
                items.Save();
                items.Insert(new KLEUpdateHistoryItem(date));
            });
        }
    }
}

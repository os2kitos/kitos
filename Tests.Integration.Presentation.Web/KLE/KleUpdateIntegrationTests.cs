﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainServices;
using ExpectedObjects;
using Infrastructure.DataAccess;

using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.KLE
{
    //Make sure this test is not affected by others since it is slow and will cause conflicts
    [Collection(nameof(SequentialTestGroup))]
    public class KleUpdateIntegrationTests : WithAutoFixture
    {
        private static readonly ConcurrentStack<int> TestKeys =
            Enumerable.Range(0, 999999).FromNullable().Select(x => new ConcurrentStack<int>(x)).Value;

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Get_Kle_Status_Returns_Forbidden(OrganizationRole role)
        {
            //Arrange
            var url = TestEnvironment.CreateUrl("api/v1/kle/status");
            var login = await HttpApi.GetCookieAsync(role);

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }
        }

        [Fact]
        public async Task GlobalAdmin_Can_Get_Kle_Status()
        {
            //Arrange
            var url = TestEnvironment.CreateUrl("api/v1/kle/status");
            var login = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            ResetKleHistory();

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var statusDto = await response.ReadResponseBodyAsKitosApiResponseAsync<KLEStatusDTO>();
                Assert.True(DateTime.TryParse(statusDto.Version, out var dt), "Failed to parse version as a string");
                Assert.False(statusDto.UpToDate);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Get_Kle_Changes_Returns_Forbidden(OrganizationRole role)
        {
            //Arrange
            var url = TestEnvironment.CreateUrl("api/v1/kle/changes");
            var login = await HttpApi.GetCookieAsync(role);

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }
        }

        [Fact]
        public async Task GlobalAdmin_Can_Get_Kle_Changes()
        {
            //Arrange
            var url = TestEnvironment.CreateUrl("api/v1/kle/changes");
            var login = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            ResetKleHistory();

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Put_Kle_Returns_Forbidden(OrganizationRole role)
        {
            //Arrange
            var url = TestEnvironment.CreateUrl($"api/v1/kle/update");
            var login = await HttpApi.GetCookieAsync(role);

            //Act
            using (var response = await HttpApi.PutWithCookieAsync(url, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }
        }

        [Fact]
        public async Task Put_Removes_Obsoleted_TaskRefs_And_Patches_Uuids_And_Adds_Any_Missing()
        {
            //Arrange
            await PrepareForDetailedTest();
            var expectedTaskRefs = BuildTaskRefIntegritySet().ToList();

            #region root
            //Remove some
            MutateEntitySet<TaskRef>(repository =>
            {
                var root = repository
                    .AsQueryable()
                    .Where(MatchRootTask())
                    .First();

                //Do a depth first removal to get around fk constraints
                var toBeRemoved = FlattenTreeDepthFirst(root).Take(3).ToList();

                // Remove task refs with ItSystem FKs
                var taskRefsWithItSystem = toBeRemoved.Where(x => x.ItSystems.Any()).ToList();
                taskRefsWithItSystem.ForEach(repository.DeleteWithReferencePreload);

                // Remove task refs without FKs
                repository.RemoveRange(toBeRemoved);
                repository.Save();
            });

            MutateEntitySet<TaskRef>(repository =>
            {
                var other = repository.AsQueryable().First();
                var objectOwnerId = other.ObjectOwnerId;
                var organizationUnitId = other.OwnedByOrganizationUnitId;
                repository.Insert(CreateTaskRef(objectOwnerId, organizationUnitId));
                repository.Insert(CreateTaskRef(objectOwnerId, organizationUnitId));
                repository.Save();
            });
            #endregion root

            #region organization
            //Add some task in organization units refs which we expect to be removed
            MutateDatabase(db =>
            {
                using (var taskUsages = new GenericRepository<TaskUsage>(db))
                using (var taskRefs = new GenericRepository<TaskRef>(db))
                {
                    var other = taskRefs.AsQueryable().First();
                    var objectOwnerId = other.ObjectOwnerId;
                    var organizationUnitId = other.OwnedByOrganizationUnitId;
                    var taskRef1 = taskRefs.Insert(CreateTaskRef(objectOwnerId, organizationUnitId));
                    var taskRef2 = taskRefs.Insert(CreateTaskRef(objectOwnerId, organizationUnitId));
                    taskRefs.Save();

                    //Add usages which we expect to be removed
                    taskUsages.Insert(CreateTaskUsage(taskRef1, objectOwnerId, organizationUnitId));
                    taskUsages.Insert(CreateTaskUsage(taskRef2, objectOwnerId, organizationUnitId));
                    taskUsages.Save();
                }
            });
            #endregion organization

            #region projects
            var project = await ItProjectHelper.CreateProject(A<string>(), TestEnvironment.DefaultOrganizationId);

            //Add some task refs to a project and save the expected keys (keys not removed)
            MutateDatabase(context =>
            {
                using (var taskRefs = new GenericRepository<TaskRef>(context))
                using (var projects = new GenericRepository<ItProject>(context))
                {
                    var itProject = projects.GetByKey(project.Id);
                    var toKeep = taskRefs.AsQueryable().Take(2).ToList();
                    toKeep.ForEach(itProject.TaskRefs.Add);
                    projects.Save();
                }
            });

            var expectedProjectTaskRefs = GetProjectTaskRefKeys(project.Id);
            Assert.Equal(2, expectedProjectTaskRefs.Count);

            //Add the task refs subject to removal
            MutateDatabase(db =>
            {
                using (var projects = new GenericRepository<ItProject>(db))
                using (var taskRefs = new GenericRepository<TaskRef>(db))
                {
                    var other = taskRefs.AsQueryable().First();
                    var objectOwnerId = other.ObjectOwnerId;
                    var organizationUnitId = other.OwnedByOrganizationUnitId;
                    var taskRef1 = taskRefs.Insert(CreateTaskRef(objectOwnerId, organizationUnitId));
                    var taskRef2 = taskRefs.Insert(CreateTaskRef(objectOwnerId, organizationUnitId));
                    taskRefs.Save();

                    //Add usages which we expect to be removed
                    var itProject = projects.GetByKey(project.Id);
                    itProject.TaskRefs.Add(taskRef1);
                    itProject.TaskRefs.Add(taskRef2);
                    projects.Save();
                }
            });
            #endregion projects

            #region systems
            var system1Dto = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            //Add some task refs to a system and save the expected keys (keys not removed)
            MutateDatabase(context =>
            {
                using (var taskRefs = new GenericRepository<TaskRef>(context))
                using (var systems = new GenericRepository<Core.DomainModel.ItSystem.ItSystem>(context))
                {
                    var itSystem = systems.GetByKey(system1Dto.Id);
                    var toKeep = taskRefs.AsQueryable().Take(2).ToList();
                    toKeep.ForEach(itSystem.TaskRefs.Add);
                    systems.Save();
                }
            });

            var expectedSystemTaskRefs = GetSystemTaskKeys(system1Dto.Id);
            Assert.Equal(2, expectedSystemTaskRefs.Count);

            //Add the task refs subject to removal
            MutateDatabase(db =>
            {
                using (var systems = new GenericRepository<Core.DomainModel.ItSystem.ItSystem>(db))
                using (var taskRefs = new GenericRepository<TaskRef>(db))
                {
                    var other = taskRefs.AsQueryable().First();
                    var objectOwnerId = other.ObjectOwnerId;
                    var organizationUnitId = other.OwnedByOrganizationUnitId;
                    var taskRef1 = taskRefs.Insert(CreateTaskRef(objectOwnerId, organizationUnitId));
                    var taskRef2 = taskRefs.Insert(CreateTaskRef(objectOwnerId, organizationUnitId));
                    taskRefs.Save();

                    //Add usages which we expect to be removed
                    var itSystem = systems.GetByKey(system1Dto.Id);
                    itSystem.TaskRefs.Add(taskRef1);
                    itSystem.TaskRefs.Add(taskRef2);
                    systems.Save();
                }
            });
            #endregion systems

            #region system usages
            var system2Dto = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            //Add some task refs to a system, and an opt out in the system usage as well as additional task refs
            MutateDatabase(context =>
            {
                using (var taskRefs = new GenericRepository<TaskRef>(context))
                using (var systems = new GenericRepository<Core.DomainModel.ItSystem.ItSystem>(context))
                {
                    var itSystem = systems.GetByKey(system2Dto.Id);
                    var toKeep = taskRefs.AsQueryable().OrderBy(x => x.Id).Take(2).ToList();
                    toKeep.ForEach(itSystem.TaskRefs.Add);
                    systems.Save();
                }
            });

            //Take system into use and add additional task refs as well as an opt out
            var usage = await ItSystemHelper.TakeIntoUseAsync(system2Dto.Id, TestEnvironment.DefaultOrganizationId);
            MutateDatabase(context =>
            {
                using (var taskRefs = new GenericRepository<TaskRef>(context))
                using (var systems = new GenericRepository<ItSystemUsage>(context))
                {
                    var systemUsage = systems.GetByKey(usage.Id);
                    var toOptOut = taskRefs.AsQueryable().OrderBy(x => x.Id).Skip(1).First();
                    var additional = taskRefs.AsQueryable().OrderBy(x => x.Id).Skip(2).First();
                    systemUsage.TaskRefs.Add(additional);
                    systemUsage.TaskRefsOptOut.Add(toOptOut);
                    systems.Save();
                }
            });

            var (expectedTaskRefKeys, expectedInheritedKeys, expectedOptOutKeys) = GetSystemUsageTasks(usage.Id);
            Assert.Equal(1, expectedTaskRefKeys.Count);
            Assert.Equal(2, expectedInheritedKeys.Count);
            Assert.Equal(1, expectedOptOutKeys.Count);

            //Add some additional which will be removed by import .. both to main system, opt out and local
            MutateDatabase(db =>
            {
                using (var systems = new GenericRepository<Core.DomainModel.ItSystem.ItSystem>(db))
                using (var usages = new GenericRepository<ItSystemUsage>(db))
                using (var taskRefs = new GenericRepository<TaskRef>(db))
                {
                    var reference = taskRefs.AsQueryable().First();
                    var objectOwnerId = reference.ObjectOwnerId;
                    var organizationUnitId = reference.OwnedByOrganizationUnitId;
                    var taskRef1 = taskRefs.Insert(CreateTaskRef(objectOwnerId, organizationUnitId));
                    var taskRef2 = taskRefs.Insert(CreateTaskRef(objectOwnerId, organizationUnitId));
                    var taskRef3 = taskRefs.Insert(CreateTaskRef(objectOwnerId, organizationUnitId));
                    taskRefs.Save();

                    //Add inherited key which should be removed
                    var system = systems.GetByKey(system1Dto.Id);
                    system.TaskRefs.Add(taskRef1);
                    systems.Save();

                    //Add additional task ref and opt out which should be removed
                    var systemUsage = usages.GetByKey(usage.Id);
                    systemUsage.TaskRefs.Add(taskRef2);
                    systemUsage.TaskRefsOptOut.Add(taskRef3);
                    usages.Save();
                }
            });

            #endregion system usages

            #region uuid_name_patch
            MutateEntitySet<TaskRef>(repository =>
            {
                var root = repository
                    .AsQueryable()
                    .Where(MatchRootTask())
                    .First();

                //Change the name of a branch of task refs. Change one uuid to empty (UX on that column now)
                var toBeRenamed = FlattenTreeDepthFirst(root).ToList();
                toBeRenamed.ForEach(Rename);
                toBeRenamed.Where((_, index) => index == 0).ToList().ForEach(ClearUUID);
                repository.Save();
            });
            #endregion

            //Act
            await PutKle();

            //Assert - make sure the removed task refs were re-added
            VerifyTaskRefIntegrity(expectedTaskRefs);
            var actualTaskRefs = GetProjectTaskRefKeys(project.Id);
            VerifyTaskRefUsageKeys(expectedProjectTaskRefs, actualTaskRefs);
            var actualSystemTaskRefs = GetSystemTaskKeys(system1Dto.Id);
            VerifyTaskRefUsageKeys(expectedSystemTaskRefs, actualSystemTaskRefs);
            var (actualTaskRefKeys, actualInheritedKeys, actualOptOutKeys) = GetSystemUsageTasks(usage.Id);
            VerifyTaskRefUsageKeys(expectedTaskRefKeys, actualTaskRefKeys);
            VerifyTaskRefUsageKeys(expectedInheritedKeys, actualInheritedKeys);
            VerifyTaskRefUsageKeys(expectedOptOutKeys, actualOptOutKeys);
        }

        private static void ClearUUID(TaskRef taskRef)
        {
            taskRef.Uuid = Guid.Empty;
        }

        private void Rename(TaskRef taskRef)
        {
            taskRef.Description = A<string>();
        }

        private static void VerifyTaskRefIntegrity(IReadOnlyList<TaskRefIntegrityInput> expectedTaskRefs)
        {
            var actualTaskRefs = BuildTaskRefIntegritySet().ToList();
            Assert.Equal(expectedTaskRefs.Count, actualTaskRefs.Count);

            for (var i = 0; i < actualTaskRefs.Count; i++)
            {
                var expected = expectedTaskRefs[i];
                var actual = actualTaskRefs[i];
                expected.ToExpectedObject().ShouldMatch(actual);
            }
        }

        private static void VerifyTaskUsageIntegrity(IReadOnlyList<TaskUsageIntegrityInput> expectedUsages)
        {
            var actualTaskRefs = BuildTaskUsageIntegritySet().ToList();
            Assert.Equal(expectedUsages.Count, actualTaskRefs.Count);

            for (var i = 0; i < actualTaskRefs.Count; i++)
            {
                var expected = expectedUsages[i];
                var actual = actualTaskRefs[i];
                expected.ToExpectedObject().ShouldMatch(actual);
            }
        }

        private static void VerifyTaskRefUsageKeys(IReadOnlyList<string> expectedKeys, IReadOnlyList<string> actualKeys)
        {
            Assert.Equal(expectedKeys.Count, actualKeys.Count);
            for (var i = 0; i < expectedKeys.Count; i++)
            {
                Assert.Equal(expectedKeys[i], actualKeys[i]);
            }
        }

        private static IEnumerable<TaskRefIntegrityInput> BuildTaskRefIntegritySet()
        {
            return MapFromEntitySet<TaskRef, IEnumerable<TaskRefIntegrityInput>>(
                repository =>
                repository
                .AsQueryable()
                .ToList()
                .Select(MapIntegrityInput)
                .OrderBy(x => x.TaskKey)
                .ToList());
        }

        private static IEnumerable<TaskUsageIntegrityInput> BuildTaskUsageIntegritySet()
        {
            return MapFromEntitySet<TaskUsage, IEnumerable<TaskUsageIntegrityInput>>(
                repository =>
                    repository
                        .AsQueryable()
                        .ToList()
                        .Select(MapIntegrityInput)
                        .OrderBy(x => x.TaskRefId)
                        .ToList());
        }

        private static TaskRefIntegrityInput MapIntegrityInput(TaskRef arg)
        {
            return new TaskRefIntegrityInput
            {
                Description = arg.Description,
                ParentTaskKey = arg.Parent?.TaskKey,
                TaskKey = arg.TaskKey,
                UUID = arg.Uuid
            };
        }

        private static TaskUsageIntegrityInput MapIntegrityInput(TaskUsage arg)
        {
            return new TaskUsageIntegrityInput
            {
                Id = arg.Id,
                TaskRefId = arg.TaskRef?.TaskKey ?? throw new InvalidOperationException("Unable to load task key of parent")
            };
        }

        private static IEnumerable<TaskRef> FlattenTreeDepthFirst(TaskRef root)
        {
            var children = new List<TaskRef>();
            var rootChildren = root.Children;
            foreach (var child in rootChildren)
            {
                children.AddRange(FlattenTreeDepthFirst(child));
            }
            children.AddRange(rootChildren);
            return children;
        }

        /// <summary>
        /// Helper method to perform queries on entities in a repository. Makes sure we don't get false cache items 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="map"></param>
        /// <returns></returns>
        private static TOutput MapFromEntitySet<TModel, TOutput>(Func<IGenericRepository<TModel>, TOutput> map) where TModel : class
        {
            return DatabaseAccess.MapFromEntitySet(map);
        }

        /// <summary>
        /// Helper method to perform changes to entities in a repository. Makes sure we don't get false cache items 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="mutate"></param>
        private static void MutateEntitySet<TModel>(Action<IGenericRepository<TModel>> mutate) where TModel : class
        {
            DatabaseAccess.MutateEntitySet(mutate);
        }

        /// <summary>
        /// Helper method to perform changes to entities in a repository. Makes sure we don't get false cache items 
        /// </summary>
        /// <param name="mutate"></param>
        private static void MutateDatabase(Action<KitosContext> mutate)
        {
            DatabaseAccess.MutateDatabase(mutate);
        }

        private static Expression<Func<TaskRef, bool>> MatchRootTask()
        {
            return taskRef => taskRef.ParentId == null;
        }

        private static async Task PutKle()
        {
            var url = TestEnvironment.CreateUrl("api/v1/kle/update");
            var login = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using (var response = await HttpApi.PutWithCookieAsync(url, login))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        private static async Task PrepareForDetailedTest()
        {
            ResetKleHistory();
            await PutKle();
            ResetKleHistory();
        }

        private TaskRef CreateTaskRef(int? objectOwnerId, int organizationUnitId)
        {
            if (TestKeys.TryPop(out var nextKey))
            {
                return new TaskRef
                {
                    TaskKey = nextKey.ToString(),
                    ObjectOwnerId = objectOwnerId,
                    LastChangedByUserId = objectOwnerId,
                    OwnedByOrganizationUnitId = organizationUnitId,
                    Uuid = Guid.NewGuid()
                };
            }
            throw new InvalidOperationException("Unable to get more keys");
        }

        private static TaskUsage CreateTaskUsage(TaskRef taskRef1, int? objectOwnerId, int organizationUnitId)
        {
            return new()
            {
                TaskRefId = taskRef1.Id,
                LastChangedByUserId = objectOwnerId,
                ObjectOwnerId = objectOwnerId,
                OrgUnitId = organizationUnitId
            };
        }

        private static void ResetKleHistory()
        {
            using var kleRepo = new GenericRepository<KLEUpdateHistoryItem>(TestEnvironment.GetDatabase());
            var all = kleRepo.AsQueryable().ToList();
            kleRepo.RemoveRange(all);
            kleRepo.Save();
        }

        private static IReadOnlyList<string> GetProjectTaskRefKeys(int projectId)
        {
            return MapFromEntitySet<ItProject, IReadOnlyList<string>>(projects => projects.GetByKey(projectId).TaskRefs.Select(x => x.TaskKey).OrderBy(x => x).ToList());
        }

        private static IReadOnlyList<string> GetSystemTaskKeys(int id)
        {
            return MapFromEntitySet<Core.DomainModel.ItSystem.ItSystem, IReadOnlyList<string>>(systems => systems.GetByKey(id).TaskRefs.Select(x => x.TaskKey).OrderBy(x => x).ToList());
        }

        private static (IReadOnlyList<string> taskRefKeys, IReadOnlyList<string> inheritedKeys, IReadOnlyList<string> optOutKeys) GetSystemUsageTasks(int id)
        {
            return MapFromEntitySet<ItSystemUsage, (IReadOnlyList<string>, IReadOnlyList<string>, IReadOnlyList<string>)>(usages =>
             {
                 var itSystemUsage = usages.GetByKey(id);

                 var taskRefKeys = itSystemUsage.TaskRefs.Select(x => x.TaskKey).OrderBy(x => x).ToList();
                 var optOutKeys = itSystemUsage.TaskRefsOptOut.Select(x => x.TaskKey).OrderBy(x => x).ToList();
                 var inheritedKeys = GetSystemTaskKeys(itSystemUsage.ItSystemId);

                 return (taskRefKeys, inheritedKeys, optOutKeys);
             });
        }
    }
}

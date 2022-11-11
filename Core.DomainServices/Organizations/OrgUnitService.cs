using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.Commands;
using Core.DomainModel.Extensions;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Infrastructure.Services.DataAccess;


namespace Core.DomainServices.Organizations
{
    public class OrgUnitService : IOrgUnitService
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ICommandBus _commandBus;

        public OrgUnitService(IGenericRepository<OrganizationUnit> orgUnitRepository,
            ITransactionManager transactionManager,
            IGenericRepository<Organization> organizationRepository,
            ICommandBus commandBus)
        {
            _orgUnitRepository = orgUnitRepository;
            _transactionManager = transactionManager;
            _organizationRepository = organizationRepository;
            _commandBus = commandBus;
        }

        public OrganizationUnit GetRoot(OrganizationUnit unit)
        {
            var whereWeStarted = unit;

            while (unit.Parent != null)
            {
                unit = unit.Parent;

                //did we get a loop?
                if (unit.Id == whereWeStarted.Id) throw new Exception("Loop in Organization Units");
            }

            return unit;
        }

        public ICollection<OrganizationUnit> GetSubTree(int orgUnitId)
        {
            var orgUnit = _orgUnitRepository.GetByKey(orgUnitId);

            return orgUnit.FlattenHierarchy().ToList();
        }

        public bool DescendsFrom(int descendantUnitId, int ancestorUnitId)
        {
            var unit = _orgUnitRepository.GetByKey(descendantUnitId);
            if (unit == null)
            {
                throw new ArgumentException($"Invalid org unit id:{descendantUnitId}");
            }

            return unit.SearchAncestry(ancestor => ancestor.Id == ancestorUnitId).HasValue;
        }

        public Maybe<OperationError> Delete(Guid organizationUuid, Guid unitUuid)
        {
            using var transaction = _transactionManager.Begin();

            var organization = _organizationRepository.AsQueryable().FirstOrDefault(x => x.Uuid == organizationUuid);
            if (organization == null)
                return new OperationError($"Organization with uuid: {organizationUuid} not found", OperationFailure.NotFound);
            var unitResult = organization.GetOrganizationUnit(unitUuid);
            if (unitResult.IsNone)
                return new OperationError($"Organization unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);
            var unit = unitResult.Value;

            var deleteRegistrationsError = _commandBus.Execute<RemoveOrganizationUnitRegistrationsCommand, Maybe<OperationError>>(new RemoveOrganizationUnitRegistrationsCommand(organization, unit));
            if (deleteRegistrationsError.HasValue)
            {
                transaction.Rollback();
                return deleteRegistrationsError.Value;
            }

            var error = organization.DeleteOrganizationUnit(unit);
            if (error.HasValue)
            {
                transaction.Rollback();
                return error.Value;
            }

            _orgUnitRepository.DeleteWithReferencePreload(unit);
            _orgUnitRepository.Save();

            transaction.Commit();

            return Maybe<OperationError>.None;
        }

        public IQueryable<OrganizationUnit> GetOrganizationUnits(Organization organization)
        {
            return _orgUnitRepository.AsQueryable().ByOrganizationId(organization.Id);
        }

        public Maybe<OrganizationUnit> GetOrganizationUnit(Guid uuid)
        {
            return _orgUnitRepository.AsQueryable().ByUuid(uuid).FromNullable();
        }
    }
}

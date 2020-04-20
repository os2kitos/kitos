using System;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices.Context;
using Core.DomainServices.Time;
using Infrastructure.Services.Delegates;

namespace Infrastructure.DataAccess.Interceptors
{
    public class EFEntityInterceptor : IEFEntityInterceptor
    {
        private readonly Factory<IOperationClock> _operationClock;
        private readonly Factory<Maybe<ActiveUserIdContext>> _userContext;
        private readonly Factory<KitosContext> _activeDbContext;
        private const string ObjectOwnerIdColumnName = nameof(IEntity.ObjectOwnerId);
        private const string LastChangedByUserIdColumnName = nameof(IEntity.LastChangedByUserId);
        private const string LastChangedColumnName = nameof(IEntity.LastChanged);

        public EFEntityInterceptor(
            Factory<IOperationClock> operationClock,
            Factory<Maybe<ActiveUserIdContext>> userContext,
            Factory<KitosContext> activeDbContext)
        {
            _operationClock = operationClock;
            _userContext = userContext;
            _activeDbContext = activeDbContext;
        }

        public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
        {
            if (ShouldHandle(interceptionContext))
            {
                switch (interceptionContext.OriginalResult)
                {
                    case DbInsertCommandTree insertCommand:
                        interceptionContext.Result = HandleInsertCommand(insertCommand);
                        return;
                    case DbUpdateCommandTree updateCommand:
                        interceptionContext.Result = HandleUpdateCommand(updateCommand);
                        return;
                }
            }
        }

        private static bool ShouldHandle(DbCommandTreeInterceptionContext interceptionContext)
        {
            return interceptionContext.OriginalResult.DataSpace == DataSpace.SSpace;
        }

        private DbCommandTree HandleInsertCommand(DbInsertCommandTree insertCommand)
        {
            var userId = GetActiveUserId();
            var now = _operationClock().Now;

            var setClauses = insertCommand.SetClauses
                .Select(clause => clause.UpdateIfMatch(ObjectOwnerIdColumnName, userId))
                .Select(clause => clause.UpdateIfMatch(LastChangedByUserIdColumnName, userId))
                .Select(clause => clause.UpdateIfMatch(LastChangedColumnName, DbExpression.FromDateTime(now)))
                .ToList();

            return new DbInsertCommandTree(
                insertCommand.MetadataWorkspace,
                insertCommand.DataSpace,
                insertCommand.Target,
                setClauses.AsReadOnly(),
                insertCommand.Returning);
        }

        private DbCommandTree HandleUpdateCommand(DbUpdateCommandTree updateCommand)
        {

            var userId = GetActiveUserId();
            var now = _operationClock().Now;

            var setClauses = updateCommand.SetClauses
                .Select(clause => clause.UpdateIfMatch(LastChangedByUserIdColumnName, userId))
                .Select(clause => clause.UpdateIfMatch(LastChangedColumnName, DbExpression.FromDateTime(now)))
                .ToList();

            return new DbUpdateCommandTree(
                updateCommand.MetadataWorkspace,
                updateCommand.DataSpace,
                updateCommand.Target,
                updateCommand.Predicate,
                setClauses.AsReadOnly(),
                null);
        }

        private int GetActiveUserId()
        {
            var userContext = _userContext();
            if (userContext.HasValue)
            {
                return userContext.Value.ActiveUserId;
            }

            //Fallback to first global admin
            return _activeDbContext().Users.First(x => x.IsGlobalAdmin).Id;
        }
    }

    public static class Extensions
    {
        public static DbModificationClause UpdateIfMatch(this DbModificationClause clause, string property, DbExpression value)
        {
            var propertyExpression = (DbPropertyExpression)((DbSetClause)clause).Property;
            //Check if property exist on model
            if (propertyExpression.Property.Name == property)
            {
                return DbExpressionBuilder.SetClause(propertyExpression, value);
            }
            return clause;
        }
    }
}

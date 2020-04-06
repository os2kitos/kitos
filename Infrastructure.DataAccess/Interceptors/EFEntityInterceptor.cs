using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using Core.DomainModel.Result;
using Core.DomainServices.Context;
using Core.DomainServices.Time;

namespace Infrastructure.DataAccess.Interceptors
{
    public class EFEntityInterceptor : IEFEntityInterceptor
    {
        private const string ObjectOwnerIdColumnName = "ObjectOwnerId";
        private const string LastChangedByUserIdColumnName = "LastChangedByUserId";
        private const string LastChangedColumnName = "LastChanged";
        private readonly IOperationClock _operationClock;
        private readonly Maybe<ActiveUserContext> _userContext;

        public EFEntityInterceptor(IOperationClock operationClock, Maybe<ActiveUserContext> userContext)
        {
            _operationClock = operationClock;
            _userContext = userContext;
            DbInterception.Add(this);
        }

        public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
        {

            if (interceptionContext.OriginalResult.DataSpace != DataSpace.SSpace)
            {
                return;
            }

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

        private DbCommandTree HandleInsertCommand(DbInsertCommandTree insertCommand)
        {
            var userId = GetActiveUserId();
            var now = _operationClock.Now;

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
            var now = _operationClock.Now;

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
            return _userContext.Value.UserEntity.Id;
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

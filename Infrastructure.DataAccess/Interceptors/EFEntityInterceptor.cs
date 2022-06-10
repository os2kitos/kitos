using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices.Context;
using Core.DomainServices.Time;


namespace Infrastructure.DataAccess.Interceptors
{
    public class EFEntityInterceptor : IDbCommandTreeInterceptor
    {
        private readonly Factory<IOperationClock> _operationClock;
        private readonly Factory<Maybe<ActiveUserIdContext>> _userContext;
        private readonly Factory<IFallbackUserResolver> _fallbackUserResolver;
        private const string ObjectOwnerIdColumnName = nameof(IEntity.ObjectOwnerId);
        private const string LastChangedByUserIdColumnName = nameof(IEntity.LastChangedByUserId);
        private const string LastChangedColumnName = nameof(IEntity.LastChanged);

        public EFEntityInterceptor(
            Factory<IOperationClock> operationClock,
            Factory<Maybe<ActiveUserIdContext>> userContext,
            Factory<IFallbackUserResolver> fallbackUserResolver)
        {
            _operationClock = operationClock;
            _userContext = userContext;
            _fallbackUserResolver = fallbackUserResolver;
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

            var updates = new List<(string parameterName, KeyValuePair<Predicate<DbSetClause>, DbExpression> condition)>
            {
                new(ObjectOwnerIdColumnName, new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause => MatchPropertyName(clause, ObjectOwnerIdColumnName), userId)),
                new(LastChangedByUserIdColumnName, new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause => MatchPropertyName(clause, LastChangedByUserIdColumnName), userId)),
                new(LastChangedColumnName, new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause => MatchPropertyName(clause, LastChangedColumnName), DbExpression.FromDateTime(now)))
            };

            var setClauses = insertCommand.SetClauses
                .Select(clause => ApplyUpdates(clause, updates))
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

            var updates = new List<(string propertyName, KeyValuePair<Predicate<DbSetClause>, DbExpression> condition)>
            {
                new(ObjectOwnerIdColumnName, new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause => MatchPropertyName(clause, ObjectOwnerIdColumnName) && MatchNull(clause), userId)), //Some EF updates end up in this e.g. changing an owned child on a parent
                new(LastChangedByUserIdColumnName, new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause => MatchPropertyName(clause, LastChangedByUserIdColumnName), userId)),
                new(LastChangedColumnName, new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause => MatchPropertyName(clause, LastChangedColumnName), DbExpression.FromDateTime(now)))
            };

            var setClauses = updateCommand.SetClauses
                .Select(clause => ApplyUpdates(clause, updates))
                .ToList();
            
            foreach (var updateDescriptor in updates)
            {
                ApplyUnusedUpdates(updateCommand, updateDescriptor, setClauses);
            }


            return new DbUpdateCommandTree(
                updateCommand.MetadataWorkspace,
                updateCommand.DataSpace,
                updateCommand.Target,
                updateCommand.Predicate,
                setClauses.AsReadOnly(),
                updateCommand.Returning);
        }

        private static bool MatchNull(DbSetClause clause)
        {
            return clause.Value is DbNullExpression;
        }

        private int GetActiveUserId()
        {
            var userContext = _userContext();
            if (userContext.HasValue)
            {
                return userContext.Value.ActiveUserId;
            }

            //Fallback to first global admin
            return _fallbackUserResolver().Resolve().Id;
        }

        private static bool MatchPropertyName(DbSetClause clause, string propertyName)
        {
            var propertyExpression = (DbPropertyExpression)(clause).Property;
            return propertyExpression.Property.Name == propertyName;
        }

        public static DbModificationClause ApplyUpdates(DbModificationClause clause, List<(string parameterName, KeyValuePair<Predicate<DbSetClause>, DbExpression> condition)> pendingUpdates)
        {
            //Only check for updates until pending updates has been depleted
            if (pendingUpdates.Any())
            {
                foreach (var pendingUpdate in pendingUpdates)
                {
                    if (pendingUpdate.condition.Key((DbSetClause)clause))
                    {
                        var propertyExpression = (DbPropertyExpression)((DbSetClause)clause).Property;

                        //Pending update matched - apply the update and break off
                        pendingUpdates.Remove(pendingUpdate);
                        return DbExpressionBuilder.SetClause(propertyExpression, pendingUpdate.condition.Value);
                    }
                }
            }

            //Return original
            return clause;
        }

        private static DbSetClause GetUpdateSetClause(string column, DbExpression newValueToSetToDb, DbUpdateCommandTree updateCommand)
        {
            // Create the variable reference in order to create the property
            var variableReference = updateCommand.Target.VariableType.Variable(updateCommand.Target.VariableName);
            
            // Create the property to which will assign the correct value
            var tenantProperty = variableReference.Property(column);

            // Create the set clause, object representation of sql insert command
            var newSetClause = DbExpressionBuilder.SetClause(tenantProperty, newValueToSetToDb);
            return newSetClause;
        }

        private static void ApplyUnusedUpdates(DbUpdateCommandTree updateCommand, (string propertyName, KeyValuePair<Predicate<DbSetClause>, DbExpression> condition) updateDescriptor, ICollection<DbModificationClause> setClauses)
        {
            var edmType = updateCommand.Target.VariableType.EdmType;
            if (edmType is not System.Data.Entity.Core.Metadata.Edm.EntityType entityType) 
                return;

            var propertyName = updateDescriptor.propertyName;
            if (entityType.Properties.Contains(propertyName))
                setClauses.Add(GetUpdateSetClause(propertyName, updateDescriptor.condition.Value, updateCommand));
        }
    }
}

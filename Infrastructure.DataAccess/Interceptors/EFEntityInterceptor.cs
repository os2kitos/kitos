using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices.Context;
using Core.DomainServices.Time;
using Infrastructure.Services.Delegates;
using Infrastructure.Services.Types;

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

            var updates = new List<KeyValuePair<Predicate<DbSetClause>, DbExpression>>
            {
                new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause=>MatchPropertyName(clause,ObjectOwnerIdColumnName), userId),
                new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause=>MatchPropertyName(clause,LastChangedByUserIdColumnName), userId),
                new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause=>MatchPropertyName(clause,LastChangedColumnName), DbExpression.FromDateTime(now))
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

            var updates = new List<KeyValuePair<Predicate<DbSetClause>, DbExpression>>
            {
                new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause=>MatchPropertyName(clause,ObjectOwnerIdColumnName) && MatchNull(clause), userId), //Some EF updates end up in this e.g. changing an owned child on a parent
                new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause=>MatchPropertyName(clause,LastChangedByUserIdColumnName), userId),
                new KeyValuePair<Predicate<DbSetClause>, DbExpression>(clause=>MatchPropertyName(clause,LastChangedColumnName), DbExpression.FromDateTime(now))
            };

            var setClauses = updateCommand.SetClauses
                .Select(clause => ApplyUpdates(clause, updates))
                .ToList();

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

        public static DbModificationClause ApplyUpdates(DbModificationClause clause, List<KeyValuePair<Predicate<DbSetClause>, DbExpression>> pendingUpdates)
        {
            //Only check for updates until pending updates has been depleted
            if (pendingUpdates.Any())
            {
                foreach (var pendingUpdate in pendingUpdates.ToList())
                {
                    if (pendingUpdate.Key((DbSetClause)clause))
                    {
                        var propertyExpression = (DbPropertyExpression)((DbSetClause)clause).Property;

                        //Pending update matched - apply the update and break off
                        pendingUpdates.Remove(pendingUpdate);
                        return DbExpressionBuilder.SetClause(propertyExpression, pendingUpdate.Value);
                    }
                }
            }

            //Return original
            return clause;
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel.PublicMessage;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;

namespace Core.BackgroundJobs.Model.PublicMessages;

public class CreateInitialPublicMessages : IAsyncBackgroundJob
{
    private readonly ITransactionManager _transactionManager;
    private readonly IGenericRepository<PublicMessage> _repository;
    public CreateInitialPublicMessages(ITransactionManager transactionManager, IGenericRepository<PublicMessage> repository)
    {
        _transactionManager = transactionManager;
        _repository = repository;
    }

    public string Id => StandardJobIds.CreateInitialPublicMessages;
    public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
    {
        var transaction = _transactionManager.Begin();
        try
        {
            for (int i = 0; i < 6; i++)
            {
                var msg = new PublicMessage
                {
                    Title = $"Title {i + 1}",
                    ShortDescription = $"Short description {i + 1}"

                };
                _repository.Insert(msg);
            }
            _repository.Save();
            transaction.Commit();
            return Task.FromResult(Result<string, OperationError>.Success("Successfully created public messages"));
        }
        catch (Exception e)
        {
            transaction.Rollback();
            return Task.FromResult(Result<string, OperationError>.Failure(new OperationError($"Error message: {e.ToString()}", OperationFailure.UnknownError)));
        }
    }
}
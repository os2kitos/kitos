using Core.Abstractions.Types;
using Core.DomainModel.PublicMessage;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace Core.BackgroundJobs.Model.PublicMessages
{
    public class CreateMainPublicMessage : IAsyncBackgroundJob
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IGenericRepository<PublicMessage> _repository;

        public CreateMainPublicMessage(ITransactionManager transactionManager, IGenericRepository<PublicMessage> repository)
        {
            _transactionManager = transactionManager;
            _repository = repository;
        }

        public string Id => StandardJobIds.CreateMainPublicMessage;
        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var doesMainExist = _repository.AsQueryable().Any(x => x.IsMain);
            if (doesMainExist)
            {
                return Task.FromResult(Result<string, OperationError>.Success("Main public message already exists"));
            }

            var transaction = _transactionManager.Begin();
            try
            {
                var msg = new PublicMessage
                {
                    Title = "Kitos er Kommunernes IT Overblikssystem",
                    ShortDescription = "Kitos er en open-source web-baseret løsning, der anvendes af 76 kommuner. Kitos skaber overblik over den samlede kommunale IT-portefølje.",
                    Link = "https://www.os2.eu/os2kitos",
                    IsMain = true,
                };
                _repository.Insert(msg);
                _repository.Save();
                transaction.Commit();
                return Task.FromResult(Result<string, OperationError>.Success("Successfully created the main public messages"));
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return Task.FromResult(Result<string, OperationError>.Failure(new OperationError($"Error message: {e}", OperationFailure.UnknownError)));
            }
        }
    }
}

namespace Infrastructure.Services.DataAccess
{
    public interface ITransactionManager
    {
        IDatabaseTransaction Begin();
    }
}

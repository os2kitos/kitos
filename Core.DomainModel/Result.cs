namespace Core.DomainModel
{
    public enum ResultStatus
    {
        Ok,

        NotFound,

        Forbidden,

        Error
    }

    public class Result<TStatus, TResult>
    {
        private readonly TStatus _status;
        private readonly TResult _resultValue;

        private Result(TStatus status, TResult result)
        {
            _status = status;
            _resultValue = result;
        }

        public static Result<ResultStatus, TResult> Ok(TResult value)
        {
            return new Result<ResultStatus, TResult>(ResultStatus.Ok, value);
        }

        public static Result<ResultStatus, TResult> Fail(ResultStatus code)
        {
            return new Result<ResultStatus, TResult>(code, default(TResult));
        }

        public TStatus GetStatus()
        {
            return _status;
        }

        public TResult GetResultValue()
        {
            return _resultValue;
        }
    }
    
}

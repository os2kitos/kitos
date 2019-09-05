using System;

namespace Core.ApplicationServices.Model
{
    public class Result<TStatus, TResult>
    {
        private readonly TStatus _status;
        private readonly TResult _resultValue;

        private Result(TStatus status, TResult result)
        {
            _status = status;
            _resultValue = result;
        }

        public static Result<OperationResult, TResult> Ok(TResult value)
        {
            return new Result<OperationResult, TResult>(OperationResult.Ok, value);
        }

        public static Result<OperationResult, TResult> Fail(OperationResult code)
        {
            if (code == OperationResult.Ok)
            {
                throw new ArgumentException($"{nameof(code)} cannot be {code:G}");
            }
            return new Result<OperationResult, TResult>(code, default(TResult));
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

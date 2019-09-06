using System;

namespace Core.ApplicationServices.Model.Result
{
    public class Result<TStatus, TResult>
    {
        public TStatus Status { get; }
        public TResult ResultValue { get; }

        private Result(TStatus status, TResult result)
        {
            Status = status;
            ResultValue = result;
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
    }
    
}

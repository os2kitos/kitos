namespace Core.DomainServices.Model.Result
{
    public class TwoTrackResult<TSuccess, TFailure>
    {
        private readonly Maybe<TFailure> _failure;
        private readonly Maybe<TSuccess> _value;


        private TwoTrackResult(bool success, Maybe<TSuccess> successResult, Maybe<TFailure> failureResult)
        {
            Ok = success;
            _value = successResult;
            _failure = failureResult;
        }

        public bool Ok { get; }

        public TSuccess Value => _value.Value;

        public TFailure Error => _failure.Value;

        public static TwoTrackResult<TSuccess, TFailure> Success(TSuccess value)
        {
            return new TwoTrackResult<TSuccess, TFailure>(true, Maybe<TSuccess>.Some(value), Maybe<TFailure>.None);
        }

        public static TwoTrackResult<TSuccess, TFailure> Failure(TFailure value)
        {
            return new TwoTrackResult<TSuccess, TFailure>(true, Maybe<TSuccess>.None, Maybe<TFailure>.Some(value));
        }
    }
}

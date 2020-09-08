namespace Core.ApplicationServices.Model.Shared
{
    public class ChangedValue<TValue>
    {
        public TValue Value { get; }

        public static implicit operator ChangedValue<TValue>(TValue source)
        {
            return new ChangedValue<TValue>(source);
        }

        public ChangedValue(TValue value)
        {
            Value = value;
        }
    }
}

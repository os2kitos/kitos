namespace Core.DomainServices.Options
{
    //TODO: Why is it still here?
    public class OptionDescriptor<TOption>
    {
        public TOption Option { get; }
        public string Description { get; }

        public OptionDescriptor(TOption option, string description)
        {
            Option = option;
            Description = description;
        }
    }
}

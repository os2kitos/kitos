namespace Core.DomainServices.Options
{
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

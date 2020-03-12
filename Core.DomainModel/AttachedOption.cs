namespace Core.DomainModel
{
    public class AttachedOption : Entity { 

        public int ObjectId { get; set; }
        public EntityType ObjectType { get; set; }
        public int OptionId { get; set; }
        public OptionType OptionType { get; set; }
    }

    public enum OptionType
    {
        SENSITIVEPERSONALDATA = 1,
        REGISTERTYPEDATA = 2
    }
    public enum EntityType
    {
        ITSYSTEMUSAGE = 1
    }
}

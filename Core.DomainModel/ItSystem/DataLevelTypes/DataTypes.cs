namespace Core.DomainModel.ItSystem.DataTypes
{
    public enum DataOptions
    {
        NO = 0,
        YES = 1,
        DONTKNOW = 2,
        UNDECIDED = 3
    }

    public enum RiskLevel
    {
        LOW = 0,
        MIDDLE = 1,
        HIGH = 2,
        UNDECIDED = 3
    }

    public enum UserCount
    {
        BELOWTEN = 0,
        TENTOFIFTY = 1,
        FIFTYTOHUNDRED = 2,
        HUNDREDPLUS = 3,
        UNDECIDED = 4
    }

    public enum HostedAt
    {
        UNDECIDED = 0,
        ONPREMISE = 1,
        EXTERNAL = 2,
        HYBRID = 3,
    }
}

namespace Core.DomainModel.ItSystem.DataTypes
{
    public enum DataSensitivityLevel
    {
        NONE = 0,
        PERSONALDATA = 1,
        PERSONALDATANDSENSITIVEDATA = 2
    }
    public enum DataOptions
    {
        NO = 0,
        YES = 1,
        DONTKNOW = 2
    }

    public enum UserCount
    {
        BELOWTEN = 0,
        TENTOFIFTY = 1,
        FIFTYTOHUNDRED = 2,
        HUNDREDPLUS = 3
    }
}

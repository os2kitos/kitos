using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystem.DataTypes
{
    public enum DataSensitivityLevel
    {
        NONE,
        PERSONALDATA,
        PERSONALDATANDSENSITIVEDATA
    }
    public enum DataOptions
    {
        NO,
        YES,
        DONTKNOW
    }

    public enum UserCount
    {
        BELOWTEN,
        TENTOFIFTY,
        FIFTYTOHUNDRED,
        HUNDREDPLUS
    }
}

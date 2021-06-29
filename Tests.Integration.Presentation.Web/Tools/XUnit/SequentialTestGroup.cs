using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.XUnit
{
    /// <summary>
    /// NOTE: Simple class used to put all sequential tests into the same group
    ///
    ///*****************************************************************************************************************************************************************
    /// ONLY USE THIS FOR TESTS WHICH ARE NOT ISOLATED E.G. CHANGES/DEPENDS ON GLOBAL STATE. IF POSSIBLE, FIX THAT IN STEAD OF PUTTING IT INTO THE SEQUENTIAL TEST GROUP
    ///*****************************************************************************************************************************************************************
    ///
    /// See: https://github.com/xunit/xunit/issues/1999
    /// </summary>
    [CollectionDefinition(nameof(SequentialTestGroup), DisableParallelization = true)]
    public class SequentialTestGroup
    {
    }
}

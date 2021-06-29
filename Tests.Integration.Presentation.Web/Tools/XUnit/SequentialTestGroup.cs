using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.XUnit
{
    /// <summary>
    /// NOTE: Simple class used to put all sequential tests into the same group
    /// See: https://github.com/xunit/xunit/issues/1999
    /// </summary>
    [CollectionDefinition(nameof(SequentialTestGroup), DisableParallelization = true)]
    public class SequentialTestGroup
    {
    }
}

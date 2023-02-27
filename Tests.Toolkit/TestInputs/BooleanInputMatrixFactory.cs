using System.Collections.Generic;
using System.Linq;

namespace Tests.Toolkit.TestInputs
{
    public static class BooleanInputMatrixFactory
    {
        public static IEnumerable<object[]> Create(int numberOfInputParameters)
        {
            var referenceValues = Enumerable.Repeat(false, numberOfInputParameters).ToList();
            yield return referenceValues.Cast<object>().ToArray();
            for (var i = 0; i < referenceValues.Count; i++)
            {
                var inputs = referenceValues.ToList();
                inputs[i] = true;
                yield return inputs.Cast<object>().ToArray();
            }

            yield return referenceValues.Select(_ => true).Cast<object>().ToArray();
        }
    }
}

using Presentation.Web.Infrastructure.OData;
using Xunit;

namespace Tests.Unit.Presentation.Web.Infrastructure
{
    public class ODataQueryStringNormalizerTest
    {
        [Fact]
        public void NormalizeNestedQueryOptions_ConvertsNestedAmpersandsToSemicolons()
        {
            const string input = "?$expand=Usages($select=OrganizationId&$expand=Organization($select=Uuid,Name)&$filter=OrganizationId eq 42)&$skip=0&$top=100&$count=true";
            const string expected = "?$expand=Usages($select=OrganizationId;$expand=Organization($select=Uuid,Name);$filter=OrganizationId eq 42)&$skip=0&$top=100&$count=true";

            var result = ODataQueryStringNormalizer.NormalizeNestedQueryOptions(input);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void NormalizeNestedQueryOptions_LeavesTopLevelQuerySeparatorsUntouched()
        {
            const string input = "?$expand=BusinessType($select=Name),Organization($select=Id,Name)&$skip=0&$top=100&$count=true";

            var result = ODataQueryStringNormalizer.NormalizeNestedQueryOptions(input);

            Assert.Equal(input, result);
        }

        [Fact]
        public void NormalizeNestedQueryOptions_DoesNotReplaceTopLevelSeparatorAfterParenthesesInStringLiteral()
        {
            // The '(' and ')' are inside a string literal — they must not affect depth.
            const string input = "?$expand=Usages($filter=Name eq 'foo(bar)')&$skip=0&$top=100";
            const string expected = "?$expand=Usages($filter=Name eq 'foo(bar)')&$skip=0&$top=100";

            var result = ODataQueryStringNormalizer.NormalizeNestedQueryOptions(input);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void NormalizeNestedQueryOptions_ConvertsNestedAmpersandWhileFilterHasStringLiteralWithParentheses()
        {
            // Nested option separator before a string-literal-containing filter must still be converted.
            const string input = "?$expand=Usages($select=Id&$filter=Name eq 'foo(bar)')&$skip=0&$top=100";
            const string expected = "?$expand=Usages($select=Id;$filter=Name eq 'foo(bar)')&$skip=0&$top=100";

            var result = ODataQueryStringNormalizer.NormalizeNestedQueryOptions(input);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void NormalizeNestedQueryOptions_HandlesEscapedSingleQuoteInsideStringLiteral()
        {
            // '' inside a string literal is an escaped quote and must not close the literal early.
            const string input = "?$expand=Usages($filter=Name eq 'O''Brien')&$skip=0&$top=100";
            const string expected = "?$expand=Usages($filter=Name eq 'O''Brien')&$skip=0&$top=100";

            var result = ODataQueryStringNormalizer.NormalizeNestedQueryOptions(input);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void NormalizeNestedQueryOptions_DoesNotRewriteTopLevelSeparatorWhenOpenParenIsInsideStringLiteral()
        {
            // Regression: an unmatched '(' inside a string literal must not increment depth,
            // which would cause the following top-level "&$top" to be rewritten to ";$top".
            const string input = "?$filter=Name eq 'A(B'&$top=10";

            var result = ODataQueryStringNormalizer.NormalizeNestedQueryOptions(input);

            Assert.Equal(input, result);
        }
    }
}

using RandomVariableGenerating.Utils;
using Shouldly;
using Xunit;

namespace RandomVariableGenerating.Tests
{
    public class ProbabilityMatrixValidatorTest
    {
        [Fact]
        public void Test()
        {
            var probabilities = new[,]
            {
                {0.1, 0.4, 0.1},
                {0.1, 0.1, 0.2}
            };
            ProbabilityMatrixValidator.Validate(probabilities).ShouldBeTrue();
        }
    }
}
using System;
using Xunit;

namespace RandomVariableGenerating.Tests
{
    public class RandomVariableGeneratorTest
    {
        [Fact]
        public void Some()
        {
            var random = new Random(1);
            var x = new[] {2, 3};
            var y = new[] {1, 3, 4};
            var probabilities = new double[2, 3]
            {
                {0.1, 0.4, 0.1},
                {0.1, 0.1, 0.2}
            };

            var a = RandomVariableGenerator.Generate(random, x, y, probabilities);
        }
    }
}
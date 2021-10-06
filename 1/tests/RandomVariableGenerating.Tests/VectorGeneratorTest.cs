using System;
using RandomVariableGenerating.Utils;
using Shouldly;
using Xunit;

namespace RandomVariableGenerating.Tests
{
    public class VectorGeneratorTest
    {
        [Fact]
        public void GenerateDeterministicVector_StaticMethod()
        {
            var random = new Random(1);
            var expectedVector = new[] {1, 3, 5, 6, 7};
            var actualVector = VectorGenerator.BuildRandomVector(random, 5, 1, 10);
            actualVector.ShouldBe(expectedVector);
        }
        
        [Fact]
        public void GenerateDeterministicVector_InstanceMethod()
        {
            var random = new Random(1);
            var expectedVector = new[] {1, 3, 5, 6, 7};
            var vectorGenerator = new VectorGenerator(random);
            var actualVector = vectorGenerator.BuildRandomVector(5, 1, 10);
            actualVector.ShouldBe(expectedVector);
        }
    }
}

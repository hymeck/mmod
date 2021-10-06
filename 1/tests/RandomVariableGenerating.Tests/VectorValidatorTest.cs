using System;
using RandomVariableGenerating.Utils;
using Shouldly;
using Xunit;

namespace RandomVariableGenerating.Tests
{
    public class VectorValidatorTest
    {
        [Fact]
        public void CheckPositive_ValidVector()
        {
            var vector = new[] {1, 4, 5};
            VectorValidator.Validate(vector).ShouldBeTrue();
        }
        
        [Fact]
        public void CheckNegative_InvalidVector_Null()
        {
            int[] vector = null;
            VectorValidator.Validate(vector).ShouldBeFalse();
        }
        
        [Fact]
        public void CheckNegative_InvalidVector_Empty()
        {
            var vector = Array.Empty<int>();
            VectorValidator.Validate(vector).ShouldBeFalse();
        }
        
        [Fact]
        public void CheckNegative_InvalidVector_WrongOrder()
        {
            var vector = new[] {1, 5, 3};
            VectorValidator.Validate(vector).ShouldBeFalse();
        }
        
        
    }
}
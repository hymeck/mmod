using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;

namespace RandomVariableGenerating.Utils
{
    public class VectorGenerator
    {
        private readonly Random _random;

        public VectorGenerator(Random random)
        {
            _random = Guard.Against.Null(random, nameof(random));
        }

        public IEnumerable<int> YieldUnorderedRandomVector(int size, int leftBound, int rightBound)
        {
            Guard.Against.NegativeOrZero(size, nameof(size));
            Guard.Against.InvalidInput(leftBound, nameof(leftBound), v => v < rightBound);

            var numberSet = new HashSet<int>(size);
            for (var i = 0; i < size; i++)
            {
                generateNumber:
                var number = _random.Next(leftBound, rightBound);
                if (numberSet.Contains(number))
                    goto generateNumber;
                
                numberSet.Add(number);
                yield return number;
            }
        }

        public IEnumerable<int> YieldRandomVector(int size, int leftBound, int rightBound) =>
            YieldUnorderedRandomVector(size, leftBound, rightBound).OrderBy(item => item);

        public int[] BuildRandomVector(int size, int leftBound, int rightBound) =>
            YieldRandomVector(size, leftBound, rightBound).ToArray();

        public static int[] BuildRandomVector(Random random, int size, int leftBound, int rightBound) =>
            new VectorGenerator(random).BuildRandomVector(size, leftBound, rightBound);
    }
}

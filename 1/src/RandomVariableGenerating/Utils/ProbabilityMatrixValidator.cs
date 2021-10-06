using System;

namespace RandomVariableGenerating.Utils
{
    public class ProbabilityMatrixValidator
    {
        private readonly double[,] _probabilities;
        private readonly double _precision;

        public ProbabilityMatrixValidator(double[,] probabilities, double precision = 1e-6)
        {
            _probabilities = probabilities;
            _precision = precision;
        }

        public bool Validate()
        {
            if (_probabilities == null || _precision < 0)
                return false;

            var accumulator = 0d;

            var rows = _probabilities.GetLength(0);
            var cols = _probabilities.GetLength(1);
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    accumulator += _probabilities[i, j];
                    if (accumulator > 1 && !IsAcceptable(accumulator, _precision))
                        return false;
                }
            }

            return IsAcceptable(accumulator, _precision);
        }

        private static bool IsAcceptable(double accumulator, double precision) =>
            Math.Abs(1d - accumulator) < precision;

        public static bool Validate(double[,] probabilities, double precision = 1e-6) =>
            new ProbabilityMatrixValidator(probabilities, precision).Validate();
    }
}
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Ardalis.GuardClauses;

namespace RandomVariableGenerating
{
    public class ProbabilityMatrix
    {
        private readonly double[,] _probabilities;
        private double[] _xDensity;
        private double[] _yDensity;

        public ProbabilityMatrix(double[,] probabilities)
        {
            _probabilities = Guard.Against.Null(probabilities, nameof(probabilities));
        }

        public IReadOnlyList<double> XDensity => _xDensity ??= GetXDensity();
        public IReadOnlyList<double> YDensity => _yDensity ??= GetYDensity();
        public int TotalCount => XDensity.Count * YDensity.Count;

        private double[] GetXDensity()
        {
            var rows = _probabilities.GetLength(0);
            var columns = _probabilities.GetLength(1);
            var xProbabilities = new double[rows];
            for (var i = 0; i< rows ; i++)
            {
                var xProbability = 0d;
                for (var j = 0; j < columns; j++) 
                    xProbability += _probabilities[i, j];
                xProbabilities[i] = xProbability;
            }

            return xProbabilities;
        }
        
        private double[] GetYDensity()
        {
            var rows = _probabilities.GetLength(0);
            var columns = _probabilities.GetLength(1);
            var yProbabilities = new double[columns];
            for (var j = 0; j < columns ; j++)
            {
                var yProbability = 0d;
                for (var i = 0; i < rows; i++) 
                    yProbability += _probabilities[i, j];
                yProbabilities[j] = yProbability;
            }

            return yProbabilities;
        }

        public override string ToString()
        {
            var rows = _probabilities.GetLength(0);
            var columns = _probabilities.GetLength(1);
            var sb = new StringBuilder();
            for (var i = 0;  i< rows ; i++)
            {
                for (var j = 0; j < columns; j++)
                    sb.Append($"{_probabilities[i, j].ToString(CultureInfo.InvariantCulture),6} ");
                sb.Remove(sb.Length - 1, 1); // remove last space
                sb.AppendLine();
            }

            return sb.ToString();
        }
        
        public static implicit operator ProbabilityMatrix(double[,] source) => new (source);
        public static implicit operator double[,](ProbabilityMatrix matrix) => matrix._probabilities;
    }
}

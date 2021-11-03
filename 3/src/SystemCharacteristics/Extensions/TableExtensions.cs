using Spectre.Console;

namespace SystemCharacteristics.Extensions
{
    internal static class TableExtensions
    {
        public static Table AddRow(this Table table, string text, double v1, double v2) =>
            table.AddRow(text, v1.ToInvariantString(), v2.ToInvariantString(), v1.Deviation(v2));
    }
}
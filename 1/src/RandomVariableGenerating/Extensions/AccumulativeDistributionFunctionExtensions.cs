using System;
using System.Globalization;
using Ardalis.GuardClauses;
using Aspose.Pdf;

namespace RandomVariableGenerating.Extensions
{
    public static class AccumulativeDistributionFunctionExtensions
    {
        public static Document ToPdfDocument(this AccumulativeDistributionFunction function)
        {
            Guard.Against.Null(function, nameof(function));
            return GetDocument(function);
        }

        private static Document GetDocument(AccumulativeDistributionFunction function)
        {
            var document = new Document();
            var table = GetPredefinedTable().SetData(function);
            document.Pages.Add().Paragraphs.Add(table);
            return document;
        }

        private static Table SetData(this Table table, AccumulativeDistributionFunction function)
        {
            var headerRow = table.Rows.Add();
            headerRow.Cells.Add("Y_i");
            headerRow.Cells.Add("n_i");
            headerRow.Cells.Add("w_i");
            headerRow.Cells.Add("w_a");

            var i = 0;
            foreach (var (point, count) in function.PointOccurrences)
            {
                var row = table.Rows.Add();
                row.Cells.Add(point.ToString(CultureInfo.InvariantCulture));
                row.Cells.Add(count.ToString(CultureInfo.InvariantCulture));
                row.Cells.Add(function.RelativeFrequencies[i].ToString());
                row.Cells.Add(function.AccumulatedFrequencies[i].ToString());
                i++;
            }

            var lastRow = table.Rows.Add();
            lastRow.Cells.Add("SUM:");
            lastRow.Cells.Add(function.TotalCount.ToString());
            lastRow.Cells.Add(Math.Round(function.TotalFrequency).ToString());

            return table;
        }

        private static Table GetPredefinedTable()
        {
            var tab1 = new Table
            {
                ColumnWidths = "50 50 50",
                ColumnAdjustment = ColumnAdjustment.AutoFitToWindow,
                DefaultCellBorder = new BorderInfo(BorderSide.All, 0.1F),
                DefaultCellPadding = new MarginInfo {Top = 5f, Left = 5f, Right = 5f, Bottom = 5f},
                Border = new BorderInfo(BorderSide.All, 1F)
            };
            return tab1;
        }
    }
}

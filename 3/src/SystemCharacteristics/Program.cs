using SystemCharacteristics.Extensions;
using Spectre.Console;
using static System.Math;

namespace SystemCharacteristics
{
    class Program
    {
        static void Main(string[] args)
        {
            // l - arrival rate
            // mu - service rate
            // rho - traffic intensity
            // p0 - zero probability
            // rp - rejection probability
            // Q - relative bandwidth
            // A - absolute bandwidth
            // ql - queue length
            // sl - system length
            // qt - queue time
            // st - system time

            double TrafficIntensity(double l, double mu) => l / mu;
            double ZeroProbability(double rho, int m) => (1 - rho) / (1 - Pow(rho, m + 2));
            double RejectionProbability(double rho, double p0, int m) => Pow(rho, m + 1) * p0;
            double RelativeBandwidth(double p0) => 1 - p0;
            double AbsoluteBandwidth(double l, double Q) => l * Q;

            double QueueLength(double rho, double p0, int m) =>
                Pow(rho, 2) * p0 * (1 - Pow(rho, m) * (m * (1 - rho) + 1)) / Pow(1 - rho, 2);

            double SystemLength(double rho, double p0, int m) =>
                rho * p0 * (1 + rho * (1 - Pow(rho, m) * (m * (1 - rho) + 1) / Pow(1 - rho, 2)));

            double QueueTime(double ql, double l) => ql / l;
            double SystemTime(double sl, double l) => sl / l;
            double Downtime(double rp, double serviceTime) => rp * serviceTime;

            var table = new Table
            {
                Border = TableBorder.MinimalHeavyHead,
                Caption = new TableTitle("System characteristics depending on queue capacity")
            };
            TableColumn FromMarkup(string markup) => new(new Markup(markup));
            string WrapText(string text, string color) => $"[{color}]{text}[/]";
            const string yellow = "#ffcc00";

            TableColumn HeaderColumn()
            {
                var t = new Table
                {
                    Border = TableBorder.None
                };
                t.AddColumns(
                    FromMarkup(WrapText("Characteristics", yellow)), 
                    FromMarkup("\\"), 
                    FromMarkup(WrapText("Queue capacity", yellow)));
                return new TableColumn(t);
            }

            table.AddColumns(
                HeaderColumn(), 
                FromMarkup(WrapText("3", yellow)), 
                FromMarkup(WrapText("4", yellow)),
                FromMarkup(WrapText("Deviation", yellow)));

            const int l = 4;
            const double mu = 2d;
            const int m1 = 3;
            const int m2 = 4;

            var rho = TrafficIntensity(l, mu);
            var rhoStr = rho.ToInvariantString();

            var p01 = ZeroProbability(rho, m1);
            var p02 = ZeroProbability(rho, m2);
            
            var rp1 = RejectionProbability(rho, p01, m1);
            var rp2 = RejectionProbability(rho, p02, m2);
            
            var Q1 = RelativeBandwidth(p01);
            var Q2 = RelativeBandwidth(p02);
            
            var A1 = AbsoluteBandwidth(l, Q1);
            var A2 = AbsoluteBandwidth(l, Q2);
            
            var ql1 = QueueLength(rho, p01, m1);
            var ql2 = QueueLength(rho, p02, m2);
            
            var sl1 = SystemLength(rho, p01, m1);
            var sl2 = SystemLength(rho, p02, m2);

            var qt1 = QueueTime(ql1, l);
            var qt2 = QueueTime(ql2, l);
            
            var st1 = SystemTime(sl1, l);
            var st2 = SystemTime(sl2, l);

            var d1 = Downtime(rp1, 1d / mu);
            var d2 = Downtime(rp2, 1d / mu);

            string Deviation(double v1, double v2) =>
                v1.CompareTo(v2) switch
                {
                    -1 => @"/\",
                    0 => "=",
                    1 => @"\/",
                    _ => "undefined"
                };

            table
                .AddRow("Traffic intensity", rhoStr, rhoStr, Deviation(rho, rho))
                .AddRow("Zero probability", p01.ToInvariantString(), p02.ToInvariantString(), Deviation(p01, p02))
                .AddRow("Rejection probability", rp1.ToInvariantString(), rp2.ToInvariantString(), Deviation(rp1, rp2))
                .AddRow("Relative bandwidth", Q1.ToInvariantString(), Q2.ToInvariantString(), Deviation(Q1, Q2))
                .AddRow("Absolute bandwidth", A1.ToInvariantString(), A2.ToInvariantString(), Deviation(A1, A2))
                .AddRow("Queue length", ql1.ToInvariantString(), ql2.ToInvariantString(), Deviation(ql1, ql2))
                .AddRow("System length", sl1.ToInvariantString(), sl2.ToInvariantString(), Deviation(ql1, ql2))
                .AddRow("Time in queue", qt1.ToInvariantString(), qt2.ToInvariantString(), Deviation(qt1, qt2))
                .AddRow("Time in system", st1.ToInvariantString(), st2.ToInvariantString(), Deviation(st1, st2))
                .AddRow("Downtime", d1.ToInvariantString(), d2.ToInvariantString(), Deviation(d1, d2));

            AnsiConsole.Write(new FigletText("Lab #3: QS Investigation").Color(Color.Yellow));
            AnsiConsole.Write(new Rule("Characteristics comparison"));
            AnsiConsole.Write(table);
        }
    }
}

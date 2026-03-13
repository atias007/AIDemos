using AnomalyDetection;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

// ProductSale.Run();

// PrincipalComponentAnalysis.Run();

// AnomalyDetector.Demo();

// AnomalyDetectorUtil.DemoSpike();
AnomalyDetectorUtil.DemoChangePoint();
return;
ChartDemo();

static void ChartDemo()
{
    // Sine wave
    var sineWave = Enumerable.Range(0, 50)
        .Select(i => Math.Sin(i * 0.2) * 10 + 10)
        .ToList();

    Console.WriteLine("Sine Wave:");
    Console.WriteLine(AsciiChart.Plot(sineWave, height: 12));

    Console.WriteLine();

    // Random walk
    var random = new Random(42);
    double value = 50;
    var randomWalk = Enumerable.Range(0, 40)
        .Select(_ => value += random.NextDouble() * 6 - 3)
        .ToList();

    Console.WriteLine("Random Walk:");
    Console.WriteLine(AsciiChart.Plot(randomWalk, height: 10));
}
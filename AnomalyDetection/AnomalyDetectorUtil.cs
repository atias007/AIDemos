using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;

namespace AnomalyDetection;

// Input data class
public class TimeSeriesData
{
    public float Value { get; set; }
}

// Prediction result class
public class SpikePrediction
{
    [VectorType(3)]
    public double[] Prediction { get; set; } = null!;
}

public class ChangePointPrediction
{
    [VectorType(4)]
    public double[] Prediction { get; set; } = null!;
}

public class ChangePointResult
{
    public int Index { get; set; }
    public float Value { get; set; }
    public bool IsChangePoint { get; set; }
    public double RawScore { get; set; }
    public double PValue { get; set; }
    public double MartingaleScore { get; set; }
}

public class AnomalyDetectorUtil(int? seed = null)
{
    private readonly MLContext _mlContext = seed.HasValue ? new MLContext(seed.Value) : new MLContext();

    public IEnumerable<(int Index, float Value, bool IsSpike, double Confidence)> DetectSpikes(
        IEnumerable<float> values,
        int pvalueHistoryLength = 10,
        double confidence = 98,
        AnomalySide side = AnomalySide.TwoSided)
    {
        var data = values.Select(v => new TimeSeriesData { Value = v }).ToList();
        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms.DetectIidSpike(
            outputColumnName: nameof(SpikePrediction.Prediction),
            inputColumnName: nameof(TimeSeriesData.Value),
            confidence: confidence,
            pvalueHistoryLength: pvalueHistoryLength,
            side: side);

        var transformedData = pipeline.Fit(dataView).Transform(dataView);
        var predictions = _mlContext.Data.CreateEnumerable<SpikePrediction>(transformedData, reuseRowObject: false);

        var results = data.Zip(predictions, (input, prediction) => (input.Value, prediction.Prediction))
            .Select((item, index) => (
                Index: index,
                item.Value,
                IsSpike: item.Prediction[0] == 1,
                Confidence: item.Prediction[2]
            ));

        return results;
    }

    public IEnumerable<ChangePointResult> DetectChangePoints(
           IEnumerable<float> values,
           int changeHistoryLength = 10,
           double confidence = 95,
           MartingaleType martingale = MartingaleType.Power,
           double eps = 0.1)
    {
        var data = values.Select(v => new TimeSeriesData { Value = v }).ToList();
        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms.DetectIidChangePoint(
            outputColumnName: nameof(ChangePointPrediction.Prediction),
            inputColumnName: nameof(TimeSeriesData.Value),
            confidence: confidence,
            changeHistoryLength: changeHistoryLength,
            martingale: martingale,
            eps: eps);

        var transformedData = pipeline.Fit(dataView).Transform(dataView);
        var predictions = _mlContext.Data.CreateEnumerable<ChangePointPrediction>(transformedData, reuseRowObject: false);

        return data.Zip(predictions, (input, prediction) => new ChangePointResult
        {
            Value = input.Value,
            IsChangePoint = prediction.Prediction[0] == 1,
            RawScore = prediction.Prediction[1],
            PValue = prediction.Prediction[2],
            MartingaleScore = prediction.Prediction[3]
        })
        .Select((result, index) =>
        {
            result.Index = index;
            return result;
        });
    }

    public static void DemoChangePoint()
    {
        var detector = new AnomalyDetectorUtil(seed: 42);

        // Data with two distinct regime changes
        float[] sensorData =
        {
            // Regime 1: baseline around 10
            10, 11, 10, 12, 11, 10, 11, 12, 10, 11,

            // Regime 2: shift up to around 25
            25, 26, 24, 25, 27, 25, 26, 24, 25, 26,

            // Regime 3: shift down to around 5
            5, 6, 4, 5, 6, 5, 4, 6, 5, 5
        };

        var results = detector.DetectChangePoints(
            sensorData,
            changeHistoryLength: 5,
            confidence: 95,
            martingale: MartingaleType.Power,
            eps: 0.1);

        Console.WriteLine("Index\tValue\tChange?\tRawScore\tP-Value\t\tMartingale");
        Console.WriteLine(new string('-', 70));

        foreach (var result in results)
        {
            if (result.IsChangePoint)
                Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"{result.Index}\t{result.Value}\t{(result.IsChangePoint ? "YES" : "no")}\t" +
                              $"{result.RawScore:F4}\t\t{result.PValue:F6}\t{result.MartingaleScore:F4}");

            Console.ResetColor();
        }
    }

    public static void DemoSpike2()
    {
        float[] data =
        {
            266,295,331,333,312,384,267,317,367,353,242,366,344,310,327,338,352,313,224,169,252,202,236,273,258,297,381,195,211,242,
            334,388,353,279,286,343,296,264,181,284,256,210,244,242,439,303,334,259,341,335,336,225,229,245,313,322,326,271,297,293,
            256,272,302,283,262,299,290,297,266,311,359,350,253,283,333,313,319,226,278,251,226,239,236,204,311,247,230,226,237,236,
            278,381,346,404,363,276,210,455,405,206,212,135,173,169,171,332,263,175,233,297,228,215,211,150,147,163,217,309,310,256,
            267,314,208,201,221,240,252,242,268,262,253,210,140,162,263,211,251,257,355,247,231,262,252,308,314,223,245,199,174,131,
            179,184,240,238,258,279,272,230,231,190,186,260,178,232,268,261,277,348,188,257,143,262,231,216,246,210,170,171,177,225,
            218,209,203,252,360,270,182,248,317,329,297,178,161,176,151,171,184,210,260,245,204,192,152,121,155,253,330,251,160,187,
            161,165,155,192,190,212,221,213,190,175,168,172,137,184,142,192,220,459,205,187,189,199,167,189,235,222,191,245,195,133,
            243,143,210,242,302,373,441,375,310,367,318,353,281,229,227,188,158,157,171,148,196,216,177,272,257,204,154,182,246,294,
            193,221,165,199,201,164,198,210,209,194,141,327,264,232,199,208,260,254,204,168,264,310,219,142,188,199,177,202,179,181,
            154,188,156,168,180,205,172,175,159,227,189,256,191,292,268,145,140,159,167,222,214,238,236,220,148,162,126,413,141,128,
            178,189,194,172,154,159,177,202,159,163,182,172,280,146,204,133,141,146,116,118,127,188,135, 98,191,179,151,142,179,152,
            205,178,196,151,178,149,167,151,125,170,198,141,226,272,138,186,272,167,199,322,166,180,180,149,121,179,259,155,113,101,
            127,156,99,126,163,192,111,160,120,243,158,75,88,127,158,105,116,73,96,130,115,113,101,131,112,142,100,106,83,78,83,105,
            88,93,101,77,76,84,97,66,105,87,128,95,79,97,77,76,83,93,108,313,310,214,216,279,209,149,161,86,180,185,177,142,188,181,
            170,229,183,164,163,140,131,138,151,148,135,193,206,191,157,195,189,199,174,167,192,236,219,219,202,240,191,227,213,214,
            204,153,129,156,173,155,150,139,155,235,195,227,238,206,202,194,252,188,227,258,224,222,238,234,253,274,313,221,141,200,
            217,219,185,194,290,269,186,201,211,207,183,163,134,171,171,127,140,199,158,161,155,231,194,201,160,129,112,113,164,142,
            147,129,124,155,83,91,78,92,153,114,106,149,150,95,125,76,98,178,185,151,128,141,105,93,91,88,77,118,109,152,49,38,50,64,
            72,55,54,59,84,54,62,68,93,81,48,41,85,93,45,22,38,69,64,41,37,67,80,59,65,64,75,68,36,42,127,66,60,80,88,90,64,40,45,79,
            74,78,74,59,62,42,31,54,58,80,74,87,111,109,86,92,82,82,97,68,110,94,76,80,73,85,85,95,117,69,86,79,82,56,94,108,61,110,
            113,91,83,65,87,162,297,308,121,128,98,62,58,67,92,71,74,159,76,157,529,85,82,77,52,61,87,96,117,57,115,90,91,64,75,69,
            121,65,46,88,88,125,85,113,111,63,42,64,75,65,64,40,48,81,90,69,127,101,152,67,124,91,78,47,59,100,83,87,43,72,69,50,26,
            30,73,94,74,57,57,79,18,31,84,54,104,38,35,51,64,56,75,41,53,77,50,24,67,46,55,53,37,61,72,57,84,142,111,160,169,173,194,
            148,96,63,148,82,122,156,156,153,165,129,159,196,194,82,147,110,116,94,116,94,118,124,124,113,106,131,138,122,101,157,117,
            81,75,138,99,92,58,116,159,156,121,120,116,76,112,149,131,119,85,84,128,80,80,91,81,118,136,126,108,103,55,46,58,67,82,42,
            46,94,79,53,50,115,58,87,56,49,1051,1244,1044,1153,1171,1147,601,60,38,83,24,55,41,74,72,476,1143,1133,130,40,39,35,
            67,67,199,177,70,125,72,96,111,93,119,95,94,97,419,76,59,80,112,103,93,121,126,61,407,473,474,432,420,429,436,502,40,54,
            115,91,119,111,120,125,102,82,93,119,77,95,91,110,92,116,96,46,73,86,125,89,83,54,66,68,53,49,35,57,35,72,81,98,75,73,54,31,
            48,58,36,47,38,59,34,41,19,23,36,76,113,165,97,237,108,106,76,54,52,53,51,178,126,44,36,62,126,133,132,96,84,83,69,61,26,47,
            51,44,46,60,80,50,57,53,59,54,60,68,30,31,52,49,46,42,38,53,57,32,372,106,14,172,28,21,47,57,22,24,36,33,45,63,30,44,45,19,
            22,29,36,41,37,18,34,21,40,60,64,66,55,30,50,30,37,108,106,74,19,51,88,51,95,44,152,118,81,97,120,110,49,111,71,126,81,114,
            90,55,105,114,110,117,121,54,131,101,111,118,164,111,83,233,198,157,98,163,177,173,188,151,224,151,163,123,159,134,188,154,
            117,220,291,294,194,290,204,170,200,209,173,258,214,218,156,144,168,195,168,123,164,139,149,78,97,160,145,172,172,174,132,
            165,190,194,163,233,226,192,178,238,218,220,232,151,131,143,326,305,266,261,210,176,179,197,215,364,362,386,418,436,388,
            351,376,444,391,491,432,463,301,317,306,371,382,455,428,344,238,304,347,411,430,430,321,316,294,327,299,311,300,374,428,
            277,249,205,203,210,239,261,313,320,173,218,205,231,185,186,250,288,293,243,277,216,198,225,228,208,194,156,188,202,193,
            260,396,318,333,332,290,244,246,261,188,214,163,174,202,212,281,225,216,292,250,222,195,207,234,470,267,181,261,276,334,
            297,306,307,230,327,287,357,285,308,275,254,215,225,227,220,268,294,272,260,230,308,270,
        };

        var detector = new AnomalyDetectorUtil();

        var results = detector.DetectSpikes(
            data,
            pvalueHistoryLength: 10,
            confidence: 98);

        Console.WriteLine("Index\tValue\tSpike?\tConfidence");
        Console.WriteLine(new string('-', 40));

        foreach (var (index, value, isSpike, confidence) in results)
        {
            if (isSpike)
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"{index}\t{value}\t{(isSpike ? "YES" : "no")}\t{confidence:F4}");
            Console.ResetColor();
        }
    }

    public static void DemoSpike()
    {
        var detector = new AnomalyDetectorUtil(seed: 42);

        // Sample data with obvious spikes at indices 5 and 12
        float[] sensorReadings =
        {
            10, 11, 10, 12, 11,   // normal
            45,                   // spike!
            10, 11, 12, 10, 11,   // normal
            9,
            -20,                  // negative spike!
            10, 11, 12
        };

        var results = detector.DetectSpikes(
            sensorReadings,
            pvalueHistoryLength: 5,
            confidence: 95);

        Console.WriteLine("Index\tValue\tSpike?\tConfidence");
        Console.WriteLine(new string('-', 40));

        foreach (var (index, value, isSpike, confidence) in results)
        {
            if (isSpike)
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"{index}\t{value}\t{(isSpike ? "YES" : "no")}\t{confidence:F4}");
            Console.ResetColor();
        }
    }
}
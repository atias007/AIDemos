using Microsoft.ML;
using Microsoft.ML.Data;

namespace AnomalyDetection;

internal class PrincipalComponentAnalysis
{
    public static void Run()
    {
        var mlContext = new MLContext(seed: 1);

        // Sample input data
        var data = new List<InputData>
            {
                new() { Features = [0,2,1] },
                new() { Features = [0,2,1] },
                new() { Features = [0,2,1] },
                new() { Features = [0,1,2] },
                new() { Features = [0,2,1] },
                new() { Features = [0,0,0.1f] },
            };

        var dataView = mlContext.Data.LoadFromEnumerable(data);

        // Configure the PCA trainer
        var pipeline = mlContext.AnomalyDetection.Trainers.RandomizedPca(
            featureColumnName: nameof(InputData.Features),
            rank: 1,
            ensureZeroMean: false,
            oversampling: 20); // simulate standard PCA

        var model = pipeline.Fit(dataView);

        var transformed = model.Transform(dataView);
        var predictions = mlContext.Data.CreateEnumerable<AnomalyPrediction1>(
            transformed,
            reuseRowObject: false)
            .ToList();

        Console.WriteLine("Anomaly Detection Results:");
        //foreach (var p in predictions)
        //{
        //    Console.WriteLine($"Score: {p.Score:F4}, Is Anomaly: {(p.PredictedLabel ? "Yes" : "No")}");
        //}

        // Output the predictions with their anomaly scores
        for (int i = 0; i < data.Count; i++)
        {
            var featuresText = string.Join(",", data[i].Features);
            var prediction = predictions[i];
            if (prediction.PredictedLabel)
            {
                Console.WriteLine($"Sample {i} [{featuresText}] is an outlier with score {prediction.Score}");
            }
            else
            {
                Console.WriteLine($"Sample {i} [{featuresText}] is an inlier with score {prediction.Score}");
            }
        }
    }
}

public class InputData
{
    [VectorType(3)]
    public float[] Features { get; set; }
}

public class AnomalyPrediction1
{
    //[ColumnName("PredictedLabel")]
    public bool PredictedLabel { get; set; }

    public float Score { get; set; }
}
using Microsoft.ML;
using Microsoft.ML.Data;

namespace AnomalyDetection;

public class SupplierData
{
    public required string SupplierName { get; set; }
    public float DayOfWeek { get; set; }
    public float HourOfDay { get; set; }
    public float TotalRequests { get; set; }
}

public class AnomalyPrediction
{
    [VectorType(4)]
    public float[] Features { get; set; } = [];

    [VectorType(4)]
    public float[] Reconstruction { get; set; } = [];

    public float Score { get; set; }
}

public class AnomalyDetector
{
    public static void Demo()
    {
        var mlContext = new MLContext();

        // Load your dataset
        var data = mlContext.Data.LoadFromTextFile<SupplierData>(
            path: "supplier_data.csv", hasHeader: true, separatorChar: ',');

        // Data processing
        var pipeline = mlContext.Transforms.Categorical.OneHotEncoding("SupplierName")
            .Append(mlContext.Transforms.Concatenate("Features",
                "SupplierName", "DayOfWeek", "HourOfDay", "TotalRequests"))
            .Append(mlContext.Transforms.ApplyOnnxModel(
                modelFile: "autoencoder.onnx",
                inputColumnNames: ["Features"],
                outputColumnNames: ["Reconstruction"]))
            .Append(mlContext.Transforms.CustomMapping(
                (AnomalyPrediction input, AnomalyPrediction output) =>
                {
                    // Calculate reconstruction error (MSE)
                    float error = 0;
                    for (int i = 0; i < input.Features.Length; i++)
                        error += (input.Features[i] - input.Reconstruction[i]) *
                                 (input.Features[i] - input.Reconstruction[i]);
                    output.Score = error / input.Features.Length;
                },
                contractName: "AnomalyScoring"));

        // Fit and transform
        var model = pipeline.Fit(data);
        var transformed = model.Transform(data);

        var predictions = mlContext.Data.CreateEnumerable<AnomalyPrediction>(
            transformed, reuseRowObject: false);

        foreach (var p in predictions)
        {
            Console.WriteLine($"Anomaly Score: {p.Score}");
        }
    }
}
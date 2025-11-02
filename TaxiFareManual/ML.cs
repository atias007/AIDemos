using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiFareManual;

#region Model

public class TaxiTrip
{
    [LoadColumn(0)]
    public string? VendorId;

    [LoadColumn(1)]
    public string? RateCode;

    [LoadColumn(2)]
    public float PassengerCount;

    [LoadColumn(3)]
    public float TripTime;

    [LoadColumn(4)]
    public float TripDistance;

    [LoadColumn(5)]
    public string? PaymentType;

    [LoadColumn(6)]
    public float FareAmount;
}

public class TaxiTripFarePrediction
{
    [ColumnName("Score")]
    public float FareAmount;
}

#endregion Model

internal class ML
{
    private readonly string _trainDataPath = @"C:\Temp\taxi-fare-train.csv";
    private readonly string _testDataPath = @"C:\Temp\taxi-fare-test.csv";
    private readonly string _modelPath = @"C:\Temp\Model.zip";

    public ITransformer Train(MLContext context)
    {
        var model = Train(context, _trainDataPath);
        return model;
    }

    private static ITransformer Train(MLContext mlContext, string dataPath)
    {
        /*
         * Loads the data.
         * Extracts and transforms the data.
         * Trains the model.
         * Returns the model.
         */

        var dataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(
            path: dataPath,
            hasHeader: true,
            separatorChar: ',');

        var pipeline = mlContext.Transforms
            .CopyColumns(outputColumnName: "Label", inputColumnName: "FareAmount")
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "VendorIdEncoded", inputColumnName: "VendorId"))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "RateCodeEncoded", inputColumnName: "RateCode"))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "PaymentTypeEncoded", inputColumnName: "PaymentType"))
            .Append(mlContext.Transforms.Concatenate(
                outputColumnName: "Features",
                inputColumnNames:
                [
                    "VendorIdEncoded",
                    "RateCodeEncoded",
                    "PassengerCount",
                    "TripDistance",
                    "PaymentTypeEncoded"
                ]))
            .Append(mlContext.Regression.Trainers.FastTree());

        var model = pipeline.Fit(dataView);
        return model;
    }

    public void Evaluate(MLContext mlContext, ITransformer model)
    {
        /*
         * Loads the test dataset.
         * Creates the regression evaluator.
         * Evaluates the model and creates metrics.
         * Displays the metrics.
         */

        var dataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(
            path: _testDataPath,
            hasHeader: true,
            separatorChar: ',');

        var predictions = model.Transform(dataView);
        var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");

        Console.WriteLine();
        Console.WriteLine($"*************************************************");
        Console.WriteLine($"*       Model quality metrics evaluation         ");
        Console.WriteLine($"*------------------------------------------------");
        Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
        Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:0.##}");
    }

    public static void TestSinglePrediction(MLContext mlContext, ITransformer model)
    {
        var predictionFunction = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(model);
        var taxiTripSample = new TaxiTrip()
        {
            VendorId = "VTS",
            RateCode = "1",
            PassengerCount = 1,
            TripTime = 1140,
            TripDistance = 3.75f,
            PaymentType = "CRD",
            FareAmount = 0 // To predict. Actual/Observed = 15.5
        };

        var prediction = predictionFunction.Predict(taxiTripSample);

        Console.WriteLine($"**********************************************************************");
        Console.WriteLine($"Predicted fare: {prediction.FareAmount:0.####}, actual fare: 15.5");
        Console.WriteLine($"**********************************************************************");
    }
}
using Microsoft.ML;
using Microsoft.ML.Data;

namespace AnomalyDetection;

internal class ProductSale
{
    public static void Run()
    {
        // Create MLContext to be shared across the model creation workflow objects

        //assign the Number of records in dataset file to cosntant variable
        const int size = 36;
        var path = @"c:\temp\product-sales.csv";

        //Load the data into IDataView.
        var mlContext = new MLContext();
        var dataView = mlContext.Data.LoadFromTextFile<ProductSalesData>(path: path, hasHeader: true, separatorChar: ',');

        //To detech temporay changes in the pattern
        DetectSpike(mlContext, size, dataView);

        //To detect persistent change in the pattern
        DetectChangepoint(mlContext, size, dataView);

        Console.WriteLine("=============== End of process, hit any key to finish ===============");

        Console.ReadLine();

        static void DetectSpike(MLContext mlContext, int size, IDataView dataView)
        {
            Console.WriteLine("===============Detect temporary changes in pattern===============");

            //STEP 1: Create Esimtator
            var estimator = mlContext.Transforms.DetectIidSpike(
                outputColumnName: nameof(ProductSalesPrediction.Prediction),
                inputColumnName: nameof(ProductSalesData.numSales),
                confidence: 95.0, // Use double for confidence
                pvalueHistoryLength: size / 4);

            //STEP 2:The Transformed Model.
            //In IID Spike detection, we don't need to do training, we just need to do transformation.
            //As you are not training the model, there is no need to load IDataView with real data, you just need schema of data.
            //So create empty data view and pass to Fit() method.
            var tansformedModel = estimator.Fit(CreateEmptyDataView(mlContext));

            //STEP 3: Use/test model
            //Apply data transformation to create predictions.
            var transformedData = tansformedModel.Transform(dataView);
            var predictions = mlContext.Data.CreateEnumerable<ProductSalesPrediction>(transformedData, reuseRowObject: false);

            Console.WriteLine("Alert\tScore\tP-Value");
            foreach (var p in predictions)
            {
                if (p.Prediction[0] == 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }
                Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}", p.Prediction[0], p.Prediction[1], p.Prediction[2]);
                Console.ResetColor();
            }
            Console.WriteLine("");
        }
    }

    private static void DetectChangepoint(MLContext mlContext, int size, IDataView dataView)
    {
        Console.WriteLine("===============Detect Persistent changes in pattern===============");

        //STEP 1: Setup transformations using DetectIidChangePoint
        var estimator = mlContext.Transforms.DetectIidChangePoint(
            outputColumnName: nameof(ProductSalesPrediction.Prediction),
            inputColumnName: nameof(ProductSalesData.numSales),
            confidence: 95.0, // Use double for confidence
            changeHistoryLength: size / 4);

        //STEP 2:The Transformed Model.
        //In IID Change point detection, we don't need need to do training, we just need to do transformation.
        //As you are not training the model, there is no need to load IDataView with real data, you just need schema of data.
        //So create empty data view and pass to Fit() method.
        var tansformedModel = estimator.Fit(CreateEmptyDataView(mlContext));

        //STEP 3: Use/test model
        //Apply data transformation to create predictions.
        var transformedData = tansformedModel.Transform(dataView);
        var predictions = mlContext.Data.CreateEnumerable<ProductSalesPrediction>(transformedData, reuseRowObject: false);

        Console.WriteLine($"{nameof(ProductSalesPrediction.Prediction)} column obtained post-transformation.");
        Console.WriteLine("Alert\tScore\tP-Value\tMartingale value");

        foreach (var p in predictions)
        {
            if (p.Prediction[0] == 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}\t{3:0.00}  <-- alert is on, predicted changepoint", p.Prediction[0], p.Prediction[1], p.Prediction[2], p.Prediction[3]);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}\t{3:0.00}", p.Prediction[0], p.Prediction[1], p.Prediction[2], p.Prediction[3]);
            }
        }
        Console.WriteLine("");
    }

    private static IDataView CreateEmptyDataView(MLContext mlContext)
    {
        //Create empty DataView. We just need the schema to call fit()
        IEnumerable<ProductSalesData> enumerableData = [];
        var dv = mlContext.Data.LoadFromEnumerable(enumerableData);
        return dv;
    }
}

public class ProductSalesData
{
    [LoadColumn(0)]
    public string Month;

    [LoadColumn(1)]
    public float numSales;
}

public class ProductSalesPrediction
{
    //vector to hold alert,score,p-value values
    [VectorType(3)]
    public double[] Prediction { get; set; }
}
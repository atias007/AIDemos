using CsvHelper;
using PricePredict;
using System.Globalization;

// TaxiFare.Train(@"R:\CodeQualityDemo\PricePredict\TaxiFare.mlnet");

// https://github.com/dotnet/machinelearning/tree/main/test/data
// https://github.com/dotnet/machinelearning/blob/main/test/data/taxi-fare-test.csv
// https://github.com/dotnet/machinelearning/blob/main/test/data/taxi-fare-train.csv

using var reader = new StreamReader(@"c:\temp\taxi-fare-test.csv");
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var records = csv.GetRecords<TestModel>();

foreach (var item in records)
{
    var sampleData = new TaxiFare.ModelInput()
    {
        Vendor_id = item.vendor_id,
        Rate_code = item.rate_code,
        Passenger_count = item.passenger_count,
        Trip_distance = item.trip_distance,
        Payment_type = item.payment_type,
    };

    var result = TaxiFare.Predict(sampleData);
    Console.WriteLine($"Predict Price: {result.Score:N2}$ --> {item.fare_amount:N2}$ --> {Perc(result.Score, item.fare_amount)}");
}
static int Perc(float predicted, float actual)
{
    if (actual == 0) return predicted == 0 ? 100 : 0;
    return (int)((1 - Math.Abs(predicted - actual) / actual) * 100);
}
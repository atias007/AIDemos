//Load sample data
using MLDemo1;

var sampleData = new Yelp.ModelInput()
{
    Col0 = @"Crust is not good.",
};

//Load model and predict output
var result = Yelp.Predict(sampleData);

Console.WriteLine(result.PredictedLabel);
Console.WriteLine(result.Score[0]);
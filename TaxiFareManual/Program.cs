using Microsoft.ML;
using TaxiFareManual;

var mlContext = new MLContext(seed: 0);
var ml = new ML();
var transormer = ml.Train(mlContext);
ml.Evaluate(mlContext, transormer);
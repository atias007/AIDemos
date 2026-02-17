namespace AnomalyDetection;

public static class AsciiChart
{
    public static string Plot(IEnumerable<double> values, int height = 10, int width = 60)
    {
        var data = values.ToList();
        if (data.Count == 0) { return string.Empty; }

        double min = data.Min();
        double max = data.Max();
        double range = max - min;
        if (range == 0) { range = 1; }

        // Scale data to fit width if needed
        var scaled = data;
        if (data.Count > width)
        {
            scaled = [.. Enumerable.Range(0, width).Select(i => data[(int)((double)i * data.Count / width)])];
        }

        var lines = new List<string>();

        // Build chart row by row from top to bottom
        for (int row = height - 1; row >= 0; row--)
        {
            double threshold = min + (range * row / (height - 1));
            var line = new char[scaled.Count];

            for (int col = 0; col < scaled.Count; col++)
            {
                double val = scaled[col];
                double valAbove = row < height - 1 ? min + (range * (row + 1) / (height - 1)) : double.MaxValue;

                if (val >= threshold && val < valAbove)
                    line[col] = '•';  // Point at this level
                else if (val >= threshold)
                    line[col] = ' '; // '│';  // Line passing through
                else
                    line[col] = ' ';
            }

            // Add axis label
            string label = threshold.ToString("F1").PadLeft(8);
            lines.Add($"{label} ┤{new string(line)}");
        }

        // Add bottom axis
        lines.Add($"{string.Empty,8} └{string.Empty.PadLeft(scaled.Count, '─')}");

        return string.Join(Environment.NewLine, lines);
    }
}
namespace PricePredict;

public record TestModel(
    string vendor_id,
    float rate_code,
    float passenger_count,
    float trip_time_in_secs,
    float trip_distance,
    string payment_type,
    float fare_amount);
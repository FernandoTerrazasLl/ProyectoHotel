using System;

public interface ICancellationFeeStrategy
{
    decimal CalculateFee(DateTime checkInDate, DateTime cancellationDate, decimal pricePerNight);
}

public class FlexibleCancellationStrategy : ICancellationFeeStrategy
{
    private const int FlexibleCancellationHoursThreshold = 24;

    public decimal CalculateFee(DateTime checkInDate, DateTime cancellationDate, decimal pricePerNight)
    {
        var hoursBeforeCheckIn = (checkInDate - cancellationDate).TotalHours;
        return hoursBeforeCheckIn >= FlexibleCancellationHoursThreshold ? 0m : pricePerNight;
    }
}

public class StrictCancellationStrategy : ICancellationFeeStrategy
{
    private const int StrictCancellationDaysThreshold = 7;
    private const decimal StrictCancellationFeeRatio = 0.5m;

    public decimal CalculateFee(DateTime checkInDate, DateTime cancellationDate, decimal pricePerNight)
    {
        var daysBeforeCheckIn = (checkInDate - cancellationDate).TotalDays;
        return daysBeforeCheckIn >= StrictCancellationDaysThreshold 
            ? Math.Round(pricePerNight * StrictCancellationFeeRatio, 2, MidpointRounding.AwayFromZero)
            : pricePerNight;
    }
}

public class NonRefundableCancellationStrategy : ICancellationFeeStrategy
{
    public decimal CalculateFee(DateTime checkInDate, DateTime cancellationDate, decimal pricePerNight)
    {
        return pricePerNight;
    }
}

public static class CancellationFeeStrategyFactory
{
    public static ICancellationFeeStrategy GetStrategy(string policyCode)
    {
        return policyCode?.Trim().ToUpperInvariant() switch
        {
            "FLEXIBLE" => new FlexibleCancellationStrategy(),
            "STRICT" => new StrictCancellationStrategy(),
            "NON_REFUNDABLE" => new NonRefundableCancellationStrategy(),
            _ => new FlexibleCancellationStrategy() // Flexible por defecto
        };
    }
}

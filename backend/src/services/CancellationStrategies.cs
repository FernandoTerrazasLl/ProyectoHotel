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
    private const int DecimalDigits = 2; // Smell 2: Magic number 2 replaced with named constant

    public decimal CalculateFee(DateTime checkInDate, DateTime cancellationDate, decimal pricePerNight)
    {
        var daysBeforeCheckIn = (checkInDate - cancellationDate).TotalDays;
        return daysBeforeCheckIn >= StrictCancellationDaysThreshold 
            ? Math.Round(pricePerNight * StrictCancellationFeeRatio, DecimalDigits, MidpointRounding.AwayFromZero)
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
    // Smell 1: Magic strings defined as constants
    public const string FlexiblePolicyCode = "FLEXIBLE";
    public const string StrictPolicyCode = "STRICT";
    public const string NonRefundablePolicyCode = "NON_REFUNDABLE";

    public static ICancellationFeeStrategy GetStrategy(string policyCode)
    {
        return policyCode?.Trim().ToUpperInvariant() switch
        {
            FlexiblePolicyCode => new FlexibleCancellationStrategy(),
            StrictPolicyCode => new StrictCancellationStrategy(),
            NonRefundablePolicyCode => new NonRefundableCancellationStrategy(),
            _ => new FlexibleCancellationStrategy() // Flexible por defecto
        };
    }
}

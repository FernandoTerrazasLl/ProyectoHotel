using NpgsqlTypes;

public enum BookingStatus
{
    [PgName("confirmed")]
    Confirmed,
    [PgName("checked_in")]
    CheckedIn,
    [PgName("checked_out")]
    CheckedOut,
    [PgName("cancelled")]
    Cancelled
}

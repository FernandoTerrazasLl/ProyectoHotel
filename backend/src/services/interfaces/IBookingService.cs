public interface IBookingService
{
    Task<OperationResult<BookingSummaryDto>> CreateBookingAsync(CreateBookingRequest request);
    Task<OperationResult<BookingSummaryDto>> GetBookingByIdAsync(int bookingId);
    Task<OperationResult<List<BookingSummaryDto>>> GetActiveAndFutureBookingsAsync();
    Task<OperationResult<BookingSummaryDto>> CheckInAsync(int bookingId);
    Task<OperationResult<BookingSummaryDto>> CheckOutAsync(int bookingId);
    Task<OperationResult<BookingSummaryDto>> CancelBookingAsync(int bookingId, CancelBookingRequest request);
}

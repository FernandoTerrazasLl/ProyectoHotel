public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);
    Task<Booking?> GetByIdWithDetailsAsync(int id);
    Task<Booking> AddWithGuestsAsync(Booking booking, IEnumerable<GuestBooking> guestBookings);
    Task UpdateAsync(Booking booking);
    Task<List<Booking>> GetActiveAndFutureAsync(DateTime referenceDate);
    Task<bool> HasOverlappingBookingAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, int? excludeBookingId = null);
}

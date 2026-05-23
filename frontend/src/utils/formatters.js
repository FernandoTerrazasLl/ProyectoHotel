/**
 * Utility functions for formatting values in the Frontend.
 */

/**
 * Rounds a numeric value to 2 decimal places.
 * @param {number|string} value - The value to round.
 * @returns {number} The rounded number.
 */
export function roundMoney(value) {
  return Math.round((Number(value) + Number.EPSILON) * 100) / 100;
}

/**
 * Formats a numeric value as a currency string.
 * @param {number|string} value - The value to format.
 * @returns {string} The formatted currency string.
 */
export function formatCurrency(value) {
  return `$${roundMoney(value).toFixed(2)}`;
}

/**
 * Formats a booking room information string consistently.
 * @param {object} booking - The booking object containing room and guest count details.
 * @returns {string} The formatted room info string.
 */
export function formatRoomInfo(booking) {
  const roomPrice = booking.roomTypePricePerNight ?? booking.roomType?.pricePerNight ?? 0;
  const roomType = booking.roomTypeName ?? booking.roomType?.name ?? "";
  return `Habitacion ${booking.roomNumber} - ${roomType} - ${formatCurrency(roomPrice)} - Numero huespedes: ${booking.numberGuests}`;
}

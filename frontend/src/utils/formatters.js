export function roundMoney(value) {
  return Math.round((Number(value) + Number.EPSILON) * 100) / 100;
}
export function formatCurrency(value) {
  return `$${roundMoney(value).toFixed(2)}`;
}

export function formatRoomInfo(booking) {
  const roomPrice = booking.roomTypePricePerNight ?? booking.roomType?.pricePerNight ?? 0;
  const roomType = booking.roomTypeName ?? booking.roomType?.name ?? "";
  return `Habitacion ${booking.roomNumber} - ${roomType} - ${formatCurrency(roomPrice)} - Numero huespedes: ${booking.numberGuests}`;
}

namespace Portfolio.Models;

public sealed class AvailableSlot
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public bool IsBooked { get; set; }
    public BookingRequest? BookingRequest { get; set; }
}

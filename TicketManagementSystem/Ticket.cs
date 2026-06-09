using System;
namespace TicketManagementSystem
{
    public class Ticket
    {
        public int Id { get; set; } = 0;

        public string Title { get; set; } = String.Empty;

        public Priority Priority { get; set; } = Priority.Low;

        public string Description { get; set; } = String.Empty;

        public User AssignedUser { get; set; } = null;

        public User AccountManager { get; set; } = null;

        public DateTime Created { get; set; } = DateTime.MinValue;

        public double PriceDollars { get; set; } = 0.0;
    }
}

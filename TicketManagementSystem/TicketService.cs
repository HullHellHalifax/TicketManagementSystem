using System;
using System.Configuration;
using System.IO;
using System.Text.Json;
using EmailService;

namespace TicketManagementSystem
{
    public class TicketService
    {
        public int CreateTicket(string t, Priority p, string assignedTo, string desc, DateTime d, bool isPayingCustomer)
        {
            // Check if t or desc are null or if they are invalid and throw exception
            if (String.IsNullOrEmpty(t) || String.IsNullOrEmpty(desc)) throw new InvalidTicketException("Title or description were null");

            User user = GetUser(assignedTo);

            if (user is null) throw new UnknownUserException("User " + assignedTo + " not found");

            bool priorityRaised = false;
            if (d < DateTime.UtcNow - TimeSpan.FromHours(1)) (p, priorityRaised) = UpdatePriority(p);

            if ((t.Contains("Crash") || t.Contains("Important") || t.Contains("Failure")) && !priorityRaised) (p, _) = UpdatePriority(p);

            if (p == Priority.High)
            {
                EmailServiceProxy emailService = new();
                emailService.SendEmailToAdministrator(t, assignedTo);
            }

            double price = 0;
            User accountManager = null;
            if (isPayingCustomer)
            {
                // Only paid customers have an account manager.
                accountManager = new UserRepository().GetAccountManager();
                price = (p == Priority.High ? 100 : 50);
            }

            Ticket ticket = new()
            {
                Title = t,
                AssignedUser = user,
                Priority = p,
                Description = desc,
                Created = d,
                PriceDollars = price,
                AccountManager = accountManager
            };

            // Return the id
            return TicketRepository.CreateTicket(ticket);
        }

        // Updates the Priority & Sets a flag indicating the priority was raised
        private (Priority, bool) UpdatePriority(Priority oldPriority)
        {
            return oldPriority switch {
                Priority.Low => (Priority.Medium, true),
                Priority.Medium => (Priority.High, true),
                Priority.High => (Priority.High, false),
                _ => (oldPriority, false)
            };
        }

        // Gets the User object for the supplied username
        private User GetUser(string userName)
        {
            User user = null;
            using (UserRepository ur = new())
            {
                if (String.IsNullOrEmpty(userName) is false)
                {
                    user = ur.GetUser(userName);
                }
            }
            return user;
        }

        public void AssignTicket(int id, string username)
        {
            User user = GetUser(username);

            if (user is null) throw new UnknownUserException("User not found");

            Ticket ticket = TicketRepository.GetTicket(id);

            if (ticket is null) throw new ApplicationException("No ticket found for id " + id);

            ticket.AssignedUser = user;

            TicketRepository.UpdateTicket(ticket);
        }

        private void WriteTicketToFile(Ticket ticket)
        {
            string ticketJson = JsonSerializer.Serialize(ticket);
            File.WriteAllText(Path.Combine(Path.GetTempPath(), $"ticket_{ticket.Id}.json"), ticketJson);
        }
    }

    public enum Priority
    {
        High,
        Medium,
        Low
    }
}

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PollStation.Models
{
    public class PollOption
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Votes { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; } = new Poll();
    }
}
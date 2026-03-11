using System.ComponentModel.DataAnnotations;

namespace PollStation.Models
{
    public class Poll
    {
        public int Id { get; set; }

        [Required]
        public string Question { get; set; }

        public string UserId { get; set; }

        public string QrCode { get; set; }

        public List<PollOption> Options { get; set; } = new();
    }
}
namespace PollStation.Models
{
    public enum PollStatus
    {
        Open,
        Closed
    }

    public class Poll
    {
        public int Id { get; set; }

        public string Question { get; set; } = string.Empty;

        public List<PollOption> Options { get; set; } = new List<PollOption>();

        public string? QrCode { get; set; }

        public string UserId { get; set; } = string.Empty;

        public PollStatus Status { get; set; }
    }
}
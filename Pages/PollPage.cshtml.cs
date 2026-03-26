using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using PollStation.Data;
using PollStation.Models;

namespace PollStation.Pages
{
    public class PollPageModel : PageModel
    {
        private readonly PollStationContext _context;
        private readonly IHubContext<PollHub> _hub;

        public Poll Poll { get; set; }

        public PollPageModel(PollStationContext context, IHubContext<PollHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task OnGetAsync(int id)
        {
            Poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // ✅ Vote + live update
        public async Task<JsonResult> OnPostVoteAsync([FromBody] VoteDto vote)
        {
            if (vote == null) return new JsonResult(new { success = false });

            var option = await _context.PollOptions.FindAsync(vote.OptionId);
            if (option != null)
            {
                option.Votes++;
                await _context.SaveChangesAsync();
            }

            var poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == vote.PollId);

            var optionsDto = poll.Options.Select(o => new { id = o.Id, text = o.Text, votes = o.Votes }).ToList();

            await _hub.Clients.All.SendAsync("PollUpdated", optionsDto);

            Console.WriteLine("Vote handler called for option " + vote.OptionId);

            return new JsonResult(new { success = true, Options = optionsDto });
        }

        public class VoteDto
        {
            public int OptionId { get; set; }
            public int PollId { get; set; }
        }

        public int TotalVotes => Poll?.Options?.Sum(o => o.Votes) ?? 0;

        public int Percentage(int votes)
        {
            if (TotalVotes == 0) return 0;
            return (int)Math.Round((double)votes / TotalVotes * 100);
        }
    }
}
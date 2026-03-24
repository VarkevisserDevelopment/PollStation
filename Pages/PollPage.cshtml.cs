using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PollStation.Data;
using PollStation.Hubs;
using PollStation.Models;

namespace PollStation.Pages
{
    public class PollPageModel : PageModel
    {
        private readonly PollStationContext _context;
        private readonly IHubContext<PollHub> _hub;

        public PollPageModel(PollStationContext context, IHubContext<PollHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public Poll Poll { get; set; }

        // ✅ GET poll
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Poll == null) return NotFound();
            return Page();
        }

        // ✅ POST vote met cookie-lock en live update
        public async Task<IActionResult> OnPostVoteAsync([FromBody] VoteDto vote)
        {
            var cookieName = $"voted_poll_{vote.PollId}";
            if (Request.Cookies[cookieName] != null)
            {
                // Stemmen mag opnieuw wijzigen, dus verwijderen oude stem
                int oldOptionId = int.Parse(Request.Cookies[cookieName]);
                var oldOption = await _context.PollOptions.FindAsync(oldOptionId);
                if (oldOption != null && oldOption.Votes > 0)
                {
                    oldOption.Votes--;
                }
            }

            // Stem toevoegen
            var option = await _context.PollOptions.FindAsync(vote.OptionId);
            if (option != null)
            {
                option.Votes++;
                await _context.SaveChangesAsync();
            }

            // Cookie zetten voor 30 dagen
            Response.Cookies.Append(cookieName, vote.OptionId.ToString(),
                new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30)
                });

            // Poll volledig laden voor live update
            var pollData = await _context.Polls
                .Where(p => p.Id == vote.PollId)
                .Include(p => p.Options)
                .Select(p => new
                {
                    p.Id,
                    Options = p.Options.Select(o => new { id = o.Id, text = o.Text, votes = o.Votes }).ToList()
                })
                .FirstOrDefaultAsync();

            // Verzenden naar alle clients
            await _hub.Clients.All.SendAsync("PollUpdated", pollData.Options);

            // JSON teruggeven (optioneel)
            return new JsonResult(new { success = true, poll = pollData });
        }

        public int TotalVotes => Poll?.Options?.Sum(o => o.Votes) ?? 0;

        public int Percentage(int votes)
        {
            if (TotalVotes == 0) return 0;
            return (int)Math.Round((double)votes / TotalVotes * 100);
        }
    }

    // ✅ DTO voor vote POST
    public class VoteDto
    {
        public int OptionId { get; set; }
        public int PollId { get; set; }
    }
}
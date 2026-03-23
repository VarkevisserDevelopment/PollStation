using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PollStation.Data;
using PollStation.Models;
using Microsoft.AspNetCore.SignalR;
using PollStation.Hubs;

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

        // ✅ Load poll
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Poll == null)
                return NotFound();

            return Page();
        }

        // ✅ Vote handler + cookie lock + live update
        public async Task<IActionResult> OnPostVoteAsync(int optionId, int pollId)
        {
            var cookieName = $"voted_poll_{pollId}";

            // 🍪 Check of al gestemd is
            if (Request.Cookies[cookieName] != null)
            {
                return RedirectToPage(new { id = pollId });
            }

            var option = await _context.PollOptions.FindAsync(optionId);

            if (option != null)
            {
                option.Votes++;
                await _context.SaveChangesAsync();
            }

            // 🍪 Cookie zetten (30 dagen)
            Response.Cookies.Append(cookieName, "true",
                new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30)
                });

            // 🔄 Live update via SignalR
            await _hub.Clients.All.SendAsync("PollUpdated");

            return RedirectToPage(new { id = pollId });
        }

        // ✅ Totaal aantal stemmen (veilig tegen null)
        public int TotalVotes => Poll?.Options?.Sum(o => o.Votes) ?? 0;

        // ✅ Percentage berekening
        public int Percentage(int votes)
        {
            if (TotalVotes == 0)
                return 0;

            return (int)Math.Round((double)votes / TotalVotes * 100);
        }
    }
}
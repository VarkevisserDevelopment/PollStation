using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PollStation.Data;
using PollStation.Models;

namespace PollStation.Pages
{
    public class PollPageModel : PageModel
    {
        private readonly PollStationContext _context;

        public PollPageModel(PollStationContext context)
        {
            _context = context;
        }

        public Poll Poll { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Poll == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostVoteAsync(int optionId, int pollId)
        {
            var cookieName = $"voted_poll_{pollId}";

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

            Response.Cookies.Append(cookieName, "true", new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30)
            });

            return RedirectToPage(new { id = pollId });
        }

        public int TotalVotes()
        {
            return Poll.Options.Sum(o => o.Votes);
        }

        public int Percentage(int votes)
        {
            var total = TotalVotes();
            if (total == 0) return 0;

            return (int)Math.Round((double)votes / total * 100);
        }
    }
}
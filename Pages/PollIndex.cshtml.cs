using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PollStation.Data;
using PollStation.Models;

namespace PollStation.Pages
{
    [Authorize]
    public class PollIndexModel : PageModel
    {
        private readonly PollStationContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty(SupportsGet = true)]
        public int? ShowResultsFor { get; set; }

        public PollIndexModel(PollStationContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Poll> Polls { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                Polls = new List<Poll>(); // voorkomt crash
                return;
            }

            Polls = await _context.Polls
                .Include(p => p.Options)
                .Where(p => p.UserId == user.Id)
                .ToListAsync();
        }

        public int TotalVotes(Poll poll)
        {
            return poll.Options.Sum(o => o.Votes);
        }

        public int Percentage(int votes, Poll poll)
        {
            var total = TotalVotes(poll);
            if (total == 0) return 0;

            return (int)Math.Round((double)votes / total * 100);
        }

        // ✅ DELETE FIXED
        public async Task<IActionResult> OnPostDeleteAsync(int pollId)
        {
            var poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == pollId);

            if (poll != null)
            {
                _context.PollOptions.RemoveRange(poll.Options);
                _context.Polls.Remove(poll);

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // ✅ STATUS CHANGE
        public async Task<IActionResult> OnPostChangeStatusAsync(int pollId, PollStatus status)
        {
            var poll = await _context.Polls.FindAsync(pollId);

            if (poll != null)
            {
                poll.Status = status;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
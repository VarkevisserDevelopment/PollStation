using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PollStation.Data;
using PollStation.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace PollStation.Pages
{
    [Authorize]
    public class PollIndexModel : PageModel
    {
        private readonly PollStationContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PollIndexModel(PollStationContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Poll> Polls { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? ShowResultsFor { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                Polls = new List<Poll>();
                return;
            }

            Polls = await _context.Polls
                .Include(p => p.Options)
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.Id)
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

        public class StatusUpdateModel
        {
            public int PollId { get; set; }
            public PollStatus Status { get; set; }
        }

        public async Task<IActionResult> OnPostChangeStatusAsync([FromBody] StatusUpdateModel model)
        {
            var poll = await _context.Polls.FindAsync(model.PollId);

            if (poll == null)
                return NotFound();

            poll.Status = model.Status;

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostDownloadQrAsync(int pollId)
        {
            var poll = await _context.Polls.FindAsync(pollId);

            if (poll == null || string.IsNullOrEmpty(poll.QrCode))
                return NotFound();

            byte[] imageBytes = Convert.FromBase64String(poll.QrCode);

            return File(imageBytes, "image/png", $"Poll_{pollId}_QR.png");
        }
    }
}
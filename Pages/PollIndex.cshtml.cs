using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        public List<Poll> Polls { get; set; }

        public PollIndexModel(PollStationContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            Polls = await _context.Polls
                .Where(p => p.UserId == user.Id)
                .ToListAsync();
        }
    }
}
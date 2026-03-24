using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PollStation.Data;
using PollStation.Models;
using QRCoder;


namespace PollStation.Pages
{
    [Authorize]
    public class CreatePollModel(PollStationContext context, UserManager<IdentityUser> userManager) : PageModel
    {
        private readonly PollStationContext _context = context;
        private readonly UserManager<IdentityUser> _userManager = userManager;
        [BindProperty]
        public Poll Poll { get; set; } = new Poll();

        [BindProperty]
        public List<string> Options { get; set; } = new List<string> { "", "" };

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            if (string.IsNullOrWhiteSpace(Poll.Question))
            {
                ModelState.AddModelError("", "Question is required");
                return Page();
            }

            var poll = new Poll
            {
                Question = Poll.Question,
                UserId = user.Id,
                Status = PollStatus.Open,
                Options = Options
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => new PollOption
                    {
                        Text = o,
                        Votes = 0
                    })
                    .ToList(),

                QrCode = "temp"
            };

            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            // QR code genereren
            var url = $"{Request.Scheme}://{Request.Host}/PollPage?id={poll.Id}";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);

            poll.QrCode = Convert.ToBase64String(qrCodeImage);

            _context.Polls.Update(poll);
            await _context.SaveChangesAsync();

            return RedirectToPage("/PollIndex");
        }
    }
}
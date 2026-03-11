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
    public class CreatePollModel : PageModel
    {
        private readonly PollStationContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CreatePollModel(PollStationContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Poll Poll { get; set; }

        [BindProperty]
        public List<string> Options { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Poll.UserId = user.Id;

            foreach (var option in Options)
            {
                Poll.Options.Add(new PollOption
                {
                    Text = option,
                    Votes = 0
                });
            }

            // QR CODE GENEREREN
            var url = $"{Request.Scheme}://{Request.Host}/PollPage?id={Poll.Id}";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);

            Poll.QrCode = Convert.ToBase64String(qrCodeImage);

            _context.Polls.Add(Poll);
            //await _context.SaveChangesAsync();
            await _context.SaveChangesAsync();

            return RedirectToPage("/PollIndex");


        }
    }
}
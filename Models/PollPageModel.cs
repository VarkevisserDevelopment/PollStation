using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PollStation.Models
{
    public class PollPageModel : PageModel
    {
        public int PerfectVotes { get; set; }
        public int NiceVotes { get; set; }
        public int BetterVotes { get; set; }
        public int BadVotes { get; set; }

        public int PerfectPercent { get; set; }
        public int NicePercent { get; set; }
        public int BetterPercent { get; set; }
        public int BadPercent { get; set; }

        private static int _perfectVotes = 0;
        private static int _niceVotes = 0;
        private static int _betterVotes = 0;
        private static int _badVotes = 0;

        public void OnGet()
        {
            LoadResults();
        }

        public IActionResult OnPost(string vote)
        {
            switch (vote)
            {
                case "perfect":
                    _perfectVotes++;
                    break;

                case "nice":
                    _niceVotes++;
                    break;

                case "better":
                    _betterVotes++;
                    break;

                case "bad":
                    _badVotes++;
                    break;
            }

            LoadResults();

            return Page();
        }

        private void LoadResults()
        {
            PerfectVotes = _perfectVotes;
            NiceVotes = _niceVotes;
            BetterVotes = _betterVotes;
            BadVotes = _badVotes;

            int total = PerfectVotes + NiceVotes + BetterVotes + BadVotes;

            if (total == 0)
            {
                PerfectPercent = NicePercent = BetterPercent = BadPercent = 0;
                return;
            }

            PerfectPercent = (int)Math.Round(PerfectVotes * 100.0 / total);
            NicePercent = (int)Math.Round(NiceVotes * 100.0 / total);
            BetterPercent = (int)Math.Round(BetterVotes * 100.0 / total);

            BadPercent = 100 - (PerfectPercent + NicePercent + BetterPercent);
        }
    }
}
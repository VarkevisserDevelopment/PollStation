using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PollStation.Data;

public class PollHub : Hub
{
    private static int _onlineUsers = 0;
    private readonly PollStationContext _context;

    public PollHub(PollStationContext context)
    {
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        _onlineUsers++;

        await Clients.All.SendAsync("UpdateUserCount", _onlineUsers);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _onlineUsers--;

        await Clients.All.SendAsync("UpdateUserCount", _onlineUsers);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task Vote(int pollId, int optionId)
    {
        var option = await _context.PollOptions.FindAsync(optionId);
        if (option == null) return;

        option.Votes++;
        await _context.SaveChangesAsync();

        var poll = await _context.Polls
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Id == pollId);

        var optionsDto = poll.Options.Select(o => new
        {
            id = o.Id,
            text = o.Text,
            votes = o.Votes
        }).ToList();

        await Clients.All.SendAsync("PollUpdated", optionsDto);
    }
}
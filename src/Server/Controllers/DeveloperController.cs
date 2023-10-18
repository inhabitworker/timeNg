using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeveloperController : ControllerBase
{
    private TimeNetDbContext _context;
    private IHubContext<EventsService, IEventsServiceClientHub> _hub;

    public DeveloperController(TimeNetDbContext context, IHubContext<EventsService, IEventsServiceClientHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    /// <summary>
    /// Seed the database with new mock data for given number of months.
    /// </summary>
    /// <returns></returns>
    [HttpPut("seed")]
    public async Task<ActionResult> Seed([FromQuery] int months)
    {
        var data = Mock.Generate(months);

        try
        {
            await _context.Seed(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok();
    }

    [HttpPut("reset")]
    public async Task<ActionResult> Reset()
    {
        await _context.Reset();
        return Ok();
    }

    [HttpPost("notification")]
    public async Task<ActionResult> SendNotification(string message, bool error = false, int seconds = 3)
    {
        await _hub.Clients.All.ReceiveNotification(message, error, seconds);
        return Ok();
    }

    
}

using Microsoft.AspNetCore.Mvc;
using Shared.Entity;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private StatsService _statistics;

    public StatisticsController(StatsService statistics)
    {
        _statistics = statistics;
    }

    /// <summary>
    /// Get a dictionary of dates with accompanying activity double.
    /// </summary>
    /// <returns></returns>
    [HttpGet("heatmap")]
    public async Task<ActionResult<Dictionary<DateTime, double>>> GetHeatmap()
    {
        var heatmap = await Task.Run(_statistics.Heatmap);
        return Ok(heatmap);
    }

    /// <summary>
    /// Get a list of the top tags.
    /// </summary>
    /// <returns></returns>
    [HttpGet("toptags")]
    public async Task<ActionResult<IEnumerable<TagDTO>>> GetTopTags()
    {
        var top = await _statistics.TopTags();
        return Ok(top);
    }

    /// <summary>
    /// Get miscellaneous stats computed from whole data.
    /// </summary>
    /// <returns></returns>
    [HttpGet("misc")]
    public async Task<ActionResult<MiscStats>> GetMisc()
    {
        var misc = await _statistics.Misc();
        return Ok(misc);
    }
}

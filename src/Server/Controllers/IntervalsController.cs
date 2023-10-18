using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Entity;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IntervalsController : ControllerBase
{
    private readonly TimeNetDbContext _context;
    private readonly ICommandService _commands;
    private readonly IQueryService _query;

    public IntervalsController(TimeNetDbContext context, ICommandService commands, IQueryService query)
    {
        _context = context;
        _commands = commands;
        _query = query;
    }

    // Queries

    /// <summary>
    /// Get a specific interval via unix timestamp.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>An interval object, if one exists for given timestamp.</returns>
    [HttpGet("start")]
    public async Task<ActionResult<IEnumerable<IntervalDTO>>> GetIntervalsByStartTimes([FromQuery] IEnumerable<DateTime> starts)
    {
        var intervals = await _context.Intervals.Where(i => starts.Contains(i.Start)).Select(i => i.ToDTO()).ToListAsync();
        return Ok(intervals);
    }
    /// <summary>
    /// Get intervals for given ids. If an interval is not returned for the provided id, it no
    /// longer or never did exist at all.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpGet("id")]
    public async Task<ActionResult<IEnumerable<IntervalDTO>>> GetIntervalsByIds([FromQuery] IEnumerable<int> ids)
    {
        var intervals = await _context.Intervals.Where(i => ids.Contains(i.Id)).Select(i => i.ToDTO()).ToListAsync();
        return Ok(intervals);
    }
    /// <summary>
    /// Get a intervals, using queries to filter.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<QueryResponse<IntervalDTO>>> GetIntervalsByQuery([FromQuery] IntervalQuery query)
    {
        // check if tags exist?
        var intervals = await _query.Intervals(query);
        return Ok(intervals);
    }

    /// <summary>
    /// Get the latest interval, useful for status/active.
    /// </summary>
    /// <returns></returns>
    [HttpGet("latest")]
    public async Task<ActionResult<IntervalDTO?>> GetLatest()
    {
        var latest = await _query.Latest();
        if (latest == null) return Ok();
        return Ok(latest);
    }
    /// <summary>
    /// Get the interval immediately proceeding the provided time. Could be an interval start time.
    /// </summary>
    /// <param name="time"></param>
    /// <returns>List of IntervalDTO objects.</returns>
    [HttpGet("next")]
    public async Task<ActionResult<IntervalDTO?>> GetNext(DateTime time)
    {
        return await _query.Next(time);
    }
    /// <summary>
    /// Get the interval immediately preceding the provided time. Could be an interval start time.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    [HttpGet("prev")]
    public async Task<ActionResult<IntervalDTO?>> GetPrevious(DateTime time)
    {
        return await _query.Previous(time);
    }

    [HttpPost("import")]
    public async Task<ActionResult> ImportIntervals(IEnumerable<IntervalDTO> data)
    {
        await _commands.ImportIntervals(data);
        return Ok();
    }

    // Commands

    /// <summary>
    /// Update/create an interval, by providing a mutated IntervalDTO object, bearing same Id.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>Updated IntervalDTO, same as input.</returns>
    [HttpPut]
    public async Task<ActionResult> ApplyInterval(IntervalDTO input)
    {
        try
        {
            await _commands.ApplyInterval(input);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        return Ok();
    }
    /// <summary>
    /// Delete intervals by providing start times.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns>Nothing.</returns>
    [HttpDelete()]
    public async Task<ActionResult> DeleteIntervals(IEnumerable<int> ids)
    {
        await _commands.DeleteIntervals(ids);
        return Ok();
    }

}

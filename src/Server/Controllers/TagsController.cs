using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Entity;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagsController : ControllerBase
{
    private readonly TimeNetDbContext _context;
    private readonly ICommandService _commands;
    private readonly IQueryService _query;

    public TagsController(TimeNetDbContext context, ICommandService commands, IQueryService query)
    {
        _context = context;
        _commands = commands;
        _query = query; 
    }

    // Queries

    /// <summary>
    /// Get a specific tag as object with associated intervals, exact name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>A list of TagDTO object.</returns>
    [HttpGet("name")]
    public async Task<ActionResult<IEnumerable<TagDTO>>> GetTagsByNames([FromQuery] List<string> names)
    {
        var tags = await _context.Tags
            .AsNoTracking()
            .Include(t => t.Intervals)
            .Where(t => names.Contains(t.Name))
            .Select(t => t.ToDTO())
            .ToListAsync();

        return Ok(tags);
    }

    /// <summary>
    /// Get a specific tag as object with associated intervals, by id.
    /// </summary>
    /// <param id="id"></param>
    /// <returns>A single TagDTO object.</returns>
    [HttpGet("id")]
    public async Task<ActionResult<IEnumerable<TagDTO>>> GetTagsByIds([FromQuery] List<int> ids)
    {
        var tags = await _context.Tags
            .AsNoTracking()
            .Include(t => t.Intervals)
            .Where(t => ids.Contains(t.Id))
            .Select(t => t.ToDTO())
            .ToListAsync();

        return Ok(tags);
    }

    /// <summary>
    /// Get tags, match provided filters.
    /// </summary>
    /// <returns>An IEnumerable of TagDTO objects.</returns>
    [HttpGet]
    public async Task<ActionResult<QueryResponse<TagDTO>>> GetTagsByQuery([FromQuery] TagQuery tagFilter)
    {
        var result = await _query.Tags(tagFilter);
        return Ok(result);
    }

    /// <summary>
    /// Rename tag by provided the old/current name and desired replcaement.
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    /// <returns>The full updated tag object.</returns>
    [HttpPut]
    public async Task<ActionResult> ApplyTag(TagDTO tag)
    {
        await _commands.ApplyTag(tag);
        return Ok();
    }

    /// <summary>
    /// Delete a tag by providing the exact name.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Status.</returns>
    [HttpDelete]
    public async Task<ActionResult> DeleteTags(List<int> ids)
    {
        try
        {
            await _commands.DeleteTags(ids);
        }
        catch
        {
            return BadRequest($"Error deleting {ids}.");
        }
        return Ok();
    }
}

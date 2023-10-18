using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Shared.Data;

/// <summary>
/// Persistent store of interval data to SQLITE.
/// </summary>
public class TimeNetDbContext : DbContext
{
    /// <summary>
    /// Interval store.
    /// </summary>
    public DbSet<IntervalEntity> Intervals { get; set; }

    /// <summary>
    /// Tags many-to-many.
    /// </summary>
    public DbSet<TagEntity> Tags { get; set; }

    /// <summary>
    /// Stores config serialized to json string.
    /// </summary>
    public DbSet<ConfigEntity> Config { get; set; }

    /// <summary>
    /// Colour regex matches.
    /// </summary>
    public DbSet<ColourMatch> Colours { get; set; }


    private IHubContext<EventsService, IEventsServiceClientHub> _hub;

    public TimeNetDbContext(IHubContext<EventsService, IEventsServiceClientHub> hub, DbContextOptions<TimeNetDbContext> options)
        : base(options)
    {
        _hub = hub;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite(@"Data Source=timeweb.db");
        options.EnableSensitiveDataLogging();
        // shut up?
		options.UseLoggerFactory(LoggerFactory.Create(builder => 
					{
						builder.AddFilter(_ => false);
					})); 
    }

    /// <summary>
    /// Save methods, overriden to do some "middlewares".
    /// </summary>
    /// <returns></returns>
    public override int SaveChanges()
    {
        HandleTracked();

        var data = DataChange();
        var conf = ConfigChange();

        var saved = base.SaveChanges();

        if (data) _hub.Clients.All.DataChanged();
        if (conf) _hub.Clients.All.ConfigChanged();

        CleanTags();

        return saved;
    }

    /// <summary>
    /// Save methods, overriden to do some "middlewares".
    /// </summary>
    /// <returns></returns>
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default(CancellationToken))
    {
        HandleTracked();
        var data = DataChange();
        var conf = ConfigChange();

        var saved = await base.SaveChangesAsync(cancellationToken);

        if (data) await _hub.Clients.All.DataChanged();
        if (conf) await _hub.Clients.All.ConfigChanged();

        CleanTags();
        return saved;
    }

    /// <summary>
    /// Applies persisted update time properties to relevant entities.
    /// </summary>
    private void HandleTracked()
    {
        foreach (var entity in ChangeTracker
            .Entries<TrackedEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            if (entity.Entity != null) entity.Entity.Updated = DateTime.Now;
    }

    /// <summary>
    /// Check "data" entities (intervald and tags) for changes.
    /// </summary>
    /// <returns></returns>
    private bool DataChange()
    {
        var intervalChange = ChangeTracker
            .Entries<IntervalEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .Any();

        var tagChange = ChangeTracker
            .Entries<TagEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .Any();

        return intervalChange || tagChange;
    }


    /// <summary>
    /// Catch if the config entity has been changed.
    /// </summary>
    /// <returns></returns>
    public bool ConfigChange() => ChangeTracker
            .Entries<ConfigEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .Any();

    /// <summary>
    /// Remove orphaned tags from data.
    /// </summary>
    private void CleanTags() => Tags.RemoveRange(Tags.Where(t => t.Intervals.Count == 0));

    public async Task Reset()
    {
        await Database.EnsureDeletedAsync();
        await Database.EnsureCreatedAsync();
        await SaveChangesAsync();
    }

    /// <summary>
    /// Recreates the database and applies fresh set of data. Does not validate.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task Seed(IEnumerable<IntervalDTO> data)
    {
        await Reset();

        // insert intervals with no tag references
        Intervals.AttachRange(data.Select(i => new IntervalEntity
        {
            Start = i.Start,
            Tags = new List<TagEntity>(),
            End = i.End,
            Annotation = i.Annotation
        }));

        // inversion of intervals list to map to tag entity list
        var tags = data
            .Where(i => i.Tags != null)
            .SelectMany(i => i.Tags.Select(t => new { i.Start, t }))
            .GroupBy(it => it.t)
            .Select(it => new TagEntity
            {
                Name = it.Key,
                Intervals = Intervals
                    .Local
                    .Select(i => i)
                    .Where(i => it.Select(it => it.Start).ToList().Contains(i.Start))
                    .ToList()
            });

        Tags.AttachRange(tags);
        await SaveChangesAsync();
    }

}


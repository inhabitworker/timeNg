namespace Shared.Validation;

/// <summary>
/// Service layer level validation against data for interval placement. Strictly not parallel.
/// </summary>
public static class Validators 
{
    /// <summary>
    /// Ensure a string is an exact existing tag.
    /// </summary>
    /// <param name="queryService"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task ExistingTags(IQueryService queryService, List<string> tags)
    {
        TagQuery query = new TagQuery() { PageSize = 0 };
        var tagEntities = (await queryService.Tags(query)).Data;

        List<string> existingTags = tagEntities.Select(t => t.Name).ToList();

        foreach(var tag in tags)
        {
            if (!existingTags.Contains(tag)) throw new Exception($"Tag '{tag}' does note exist.");
        }
    }

    public static IEnumerable<IntervalDTO> Overlaps(IEnumerable<IntervalDTO> testData, IntervalDTO interval)
    {
        var intervals = testData.OrderByDescending(i => i.Start).ToList();
        var latest = intervals.First();
        var results = new List<IntervalDTO>();

        // get index where existing start time is first earlier/equal to interval being testedes start
        var index = intervals.FindIndex(i => DateTime.Compare(i.Start, interval.Start) < 1);
        // startposition may or may not be a conflict -- jump start do

        do
        {
            var test = intervals.ElementAt(index);
            results.Add(test);

            if (index == 0) break;
            index--;

        } while (Overlap(intervals.ElementAt(index), interval));

        return results;
    }

    public static bool Overlap(IntervalDTO existing, IntervalDTO integrating)
    {
        if (existing.End == null && integrating.End == null) return true;

        //-----[  existing   ]------------------------
        //-----------------------[  integ  ]----------

        //----------------------[  existing   ]-------
        //--[  integ  ]-------------------------------

        if (integrating.Start > (existing.End ?? DateTime.Now) 
            || (integrating.End ?? DateTime.Now) < existing.Start) return false;

        return true;

        //-----[  existing   ]------
        //-------[ integ ]----------

        //-----[  existing  ]-------
        //---[ integ ]--------------

        //-----[  existing   ]------
        //---[     integ       ]----

        //-----[  existing  ]-------
        //-------------[ integ ]----
    }

    public static async Task DisjointInterval(IQueryService query, IntervalDTO interval)
    {
        // see validation attribute for checking object against self, not later than now , etc..

        // querying all intervals, excluding prior start/id if shifting
            // latest can be assumed to be active if end is null

        // Excluding id 0 is cool
        IntervalQuery intervalQuery = new() { PageSize = 0, Exclude = new List<int>() { interval.Id } };
        var intervals = (await query.Intervals(intervalQuery)).Data;

        if (intervals.Count() == 0) return; // no intervals, do whatever you like.

        // active/latest case 
        var latest = intervals.First();
        if(interval.End == null)
        {
            if (latest.End == null)
            {
                throw new Exception("There is already an active interval.");
            }
            else if (!(DateTime.Compare(latest.End.Value, interval.Start) < 0))
            {
                throw new Exception("Active interval must be latest time on record.");
            }

            return;
        }
        else
        {
            // if there is an active interval, provided complete interval must fall before
            // if the end is the start assuredly will
            if (latest.End == null && interval.End > latest.Start) 
                throw new Exception("New interval cannot overlap active interval.");
        }


        // needless? there should be no exact match for provided times in remaining list 
        var exactStart = intervals.FirstOrDefault(i => i.Start == interval.Start || i.Start == interval.End);
        if (exactStart != null) throw new Exception($"Time {exactStart.Start} is already anchoring an interval.");

        var exactEnd = intervals.FirstOrDefault(i => i.End == interval.Start || i.End == interval.End);
        if (exactEnd != null) throw new Exception($"Time {exactEnd.End} is already anchoring an interval.");


        // get position where interval ought to lie (currently occupied by preceding interval)
        var position = intervals
            .ToList()
            .FindIndex(i => DateTime.Compare(i.Start, interval.Start) < 1);


        // test against current index occupant (to be preceding)
        var preceding = intervals.ElementAt(position);
        if (preceding != null)
        {
            if (DateTime.Compare(interval.Start, preceding.End!.Value) < 0)
                throw new Exception($"Interval overlaps with preceding interval {preceding.Start}, which ends at {preceding.End}.");
        }


        // test against following index occupant (to be proceeding)
        var proceeding = intervals.ElementAt(position-1);
        if (interval.End != null && proceeding != null) 
        {
            if (DateTime.Compare(interval.End.Value, proceeding.Start) > 0) 
                throw new Exception($"Interval overlaps with proceeding interval starting at {proceeding.Start}.");
        }
    }

 }


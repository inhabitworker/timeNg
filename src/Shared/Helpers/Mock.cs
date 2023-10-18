using System.Text.RegularExpressions;
using Shared.Entity;

namespace Shared.Helpers;

/// <summary>
/// Static methods for generating random "mock" data.
/// </summary>
public static class Mock
{
    /// <summary>
    /// Generate and store data to field in class.
    /// </summary>
    /// <param name="months"></param>
    public static List<IntervalDTO> Generate(int months = 3)
    {
        List<IntervalDTO> generated = new();
        if (months <= 0) return generated;

        DateTime date = DateTime.Now.AddMonths(-months);
        Random random = new();

        // tail recurse unreliable for dotnet
        while (!(date >= DateTime.Now))
        {
            // choose day-relevant data set to pull from
            MockData dataPool = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Saturday ? Weekend : Work;

            // length of interval and other data varies with time of day and so on
            int length;
            List<string> tags;
            string annotation = "";

            if (date.Hour >= 22)
            {
                length = (int)(7 * 60 * 60 + random.NextDouble() * 60 * 60);
                tags = new() { "Sleeping" };
            }
            else if (date.Hour >= 17)
            {
                dataPool = Freetime;
                length = (int)(60 * 60 * (0.5 + random.NextDouble()));
                tags = GetTags(random, dataPool, random.Next(1, 3));
                annotation = random.Next(14) == 7 ? dataPool.Annotations[random.Next(0, dataPool.Annotations.Count)] : "";
            }
            else
            {
                // datapool set for day
                length = (int)(2 * 60 * 60 * (0.5 + random.NextDouble()));
                tags = GetTags(random, dataPool, random.Next(1, 3));
                annotation = random.Next(14) == 7 ? dataPool.Annotations[random.Next(0, dataPool.Annotations.Count)] : "";
            }

            // halt if interval extends into future
            if (date.AddSeconds(length) > DateTime.Now) break;

            // entropy lol
            if (random.Next(10) > 2)
            {
                // alright then add it
                generated.Add(new IntervalDTO()
                {
                    Start = date,
                    End = date.AddSeconds(length),
                    Tags = tags,
                    Annotation = annotation
                });
            }

            // add some time to buffer for next record gen, then call again
            int buffer = random.Next(300, 3000);
            date = date.AddSeconds(length).AddSeconds(buffer);
        }

        return generated;
    }

    private static List<string> GetTags(Random random, MockData dataPool, int n)
    {
        List<string> tags = new List<string>();

        while (tags.Count <= n)
        {
            var tag = dataPool.Tags[random.Next(0, dataPool.Tags.Count - 1)];
            if (!tags.Contains(tag))
            {
                tags.Add(tag);
            }
        };

        return tags;
    }


    // work week engagements
    private static readonly MockData Work = new MockData()
    {
        Tags = {
            "Project Red",
            "Project Green",
            "Project Blue",
            "Maintenence",
            "Communication",
            "Review",
            "Collecting Research",
            "Word Processor",
            "Goofing Off",
            "Spreadsheets",
            "Video Conference",
            "Making Coffee",
            "Reporting Issues",
            "Human Resources",
            "Onboarding Intern",
            "Abusing Intern",
            "Programming",
            "Design",
            "Meeting",
            "Billable"
        },
        Annotations = {
            "I dont get paid enough for this.",
            "We need to follow up on this, immediately.",
            "Remember the e-mail?",
            "I am a working professional, but this task has overwhelmed me. I will take some time off very soon. I must remember to clear my calendar, after this.",
            "Soon I will leave my role for an better one.",
            "Im about to go postal.",
            "Our industry is experiencing a downturn, and the results of this activity prove it.",
            "Our industry is experiencing a downturn, and the product of this activity has not helped the matter."
        }
    };

    // weekend engagements
    private static readonly MockData Weekend = new MockData()
    {
        Tags = {
            "Socializing",
            "Exercise",
            "Gaming",
            "Shopping",
            "Eating",
            "Social Media"
        },
        Annotations = {
            "This is such a waste of my weekend.",
            "I will soon be back to work, and I can forget all about this.",
            "Not doing this again.",
            "Planned for this all week and it was great, I should do it again.",
            "We cannot afford to keep doing this, as inflation is rising.",
            "Instead of doing this, I must find a new side hustle.",
            "BAD!"
        }
    };

    // atomic evening things
    private static MockData Freetime = new MockData()
    {
        Tags = {
            "Reading",
            "Nap",
            "Walking Dog",
            "Phone Call",
            "Watching Documentary",
            "Snack",
            "Watching TV",
            "News",
            "E-mail",
            "Gaming"
        },
        Annotations = {
            "I have a headache.",
            "I am hungry.",
            "Just remembered about something important.",
            "I need to sleep.",
            "That was not fun, at all.",
            "What am I doing?"

        }
    };

    private record MockData
    {
        public List<string> Tags { get; set; } = new();
        public List<string> Annotations { get; set; } = new();
    }

    public static List<ColourMatch> Colours(IEnumerable<string> nameList)
    {
        var _random = new Random();

        int floor = 2;
        int ceil = 7;
        int i = 0;
        List<int> used = new();
        List<ColourMatch> tagColours = new();

        while (i <= ceil)
        {
            if (_random.Next(14) == 7 && i >= floor) break;

            string col = "#";
            for (var x = 0; x < 3; x++)
            {
                col = $"{col}{_random.Next(255).ToString("X2")}";
            }

            int index = -1;


            while (used.Contains(index) || index == -1)
            {
                index = _random.Next(0, nameList.Count() - 1);
            }

            var tag = nameList.ElementAt(index);
            tagColours.Add(new() { Regex = $@"{Regex.Escape(tag)}", Colour = col });
            used.Add(index);

            i++;
        }

        return tagColours;

        /*
        var config = Get();
        config.Colours.Concat(tagColours);
        Set(config);
        */
    }

}



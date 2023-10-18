#!/usr/bin/env lua

-- carefule with the apostrophe'd strings

local Data = {

	-- work week engagements
	work = {
		tags = {
			"Project Red",
			"Project Green",
			"Project Blue",
			"Maintenence",
			"Communication",
			"Review",
			"Collecting reasearch",
			"Word Processor",
			"Programming",
			"Design",
			"Meeting",
			"Billable"
		},
		annotations = {
			"I dont get paid enough for this.",
			"We need to follow up on this, immediately.",
			"Remember the e-mail?",
			"I am a working professional, but this task has overwhelmed me. I will take some time off very soon. I must remember to clear my calendar, after this.",
			"Soon I will leave my role for an better one.",
			"Im about to go postal.",
			"Our industry is experiencing a downturn, and the results of this activity prove it.",
			"Our industry is experiencing a downturn, and the product of this activity has not helped the matter."
		}
	},

	-- weekend engagements
	weekend = {
		tags = {
			"Socializing",
			"Exercise",
			"Gaming",
			"Shopping",
			"Eating",
			"Social Media"
		},
		annotations = {
			"This is such a waste of my weekend.",
			"I will soon be back to work, and I can forget all about this.",
			"Not doing this again.",
			"Planned for this all week and it was great, I should do it again.",
			"We cannot afford to keep doing this, as inflation is rising.",
			"Instead of doing this, I must find a new side hustle.",
			"BAD!"
		}
	},

	-- atomic evening things
	freetime = {
		tags = {
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
		annotations = {
			"I have a headache.",
			"I am hungry.",
			"Just remembered about something important.",
			"I need to sleep.",
			"That was not fun, at all.",
			"What am I doing?"
		}
	}
}

return Data

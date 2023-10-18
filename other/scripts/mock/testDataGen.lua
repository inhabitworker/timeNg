#!/usr/bin/env lua

-- some globas, require data and util luas,
--
--Phony = require('./data')
Util = require 'util'

TWPath = os.getenv("HOME").."/.timewarrior"
BackUpDir = "/.timewarrior_backup_0"

-- ugly but im tired
Start = nil
End = nil
TagCounts =	{}


-- interactive configuration of globals, ignoring args
function Configure ()
	local function setSpan()
		print("How many days worth of timewarrior data should we generate?")
		print("		Note: Any existing data will be backed up automatically.")
		print("		Note: Enter a numerical value below 2001.")

		local input = io.read("*l")
		local tempSpan = tonumber(input)

		if tempSpan ~= nil then
			if tempSpan < 2001 then
				Span = tempSpan
			else
				print("Enter a lower number (max: 2000.")
				setSpan()
			end
		else
			print("Enter a numerical value (max: 2000).")
			setSpan()
		end
	end
	setSpan()
	Init()
end


-- function handles tag counting as interval are accrued, for later tags.data generate
function TagCount(tags)
	for _,tag in ipairs(tags) do
		if TagCounts[tag] ~= nil then
			TagCounts[tag] = TagCounts[tag] + 1
		else
			TagCounts[tag] = 1
		end
	end
end


-- tags.data generator
function Tags()
	print("["..os.clock().."] Writing tag data files...\n")

	local tagsFile = TWPath.."/data/".."tags.data"
	local writer = io.open(tagsFile, "w")
	assert(writer)

	writer:write("{\n")

	for k,v in pairs(TagCounts) do
		local tagString = '	"'..k..'":{"count":'..v..'},\n'
		writer:write(tagString)
	end
	writer:write("}")


	print("Finished data generation.")
	print("Backup can be located in home dir as: "..BackUpDir)
end


-- main generator loop
function Generate(initEpoch)
	-- normalize date for recursive actions
	local wake = initEpoch - math.fmod(initEpoch, 24*60*60) + 7*60*60 + math.random(1,30)*60

	-- new file for year-month
	local month = os.date("%m", wake)
	local year = os.date("%Y", wake)

	print("["..os.clock().."] Writing data for "..year.."-"..month)
	local dataFile = TWPath.."/data/"..year.."-"..month..".data"
	local writer = io.open(dataFile, "w")
	local undoWriter = io.open(TWPath.."/data/undo.data", "w")
	assert(writer)
	assert(undoWriter)

	-- we will control by increment this throughout day
	local varEpoch = wake

	-- begin looping days of month, tick over "day" 01 check at end, allowing entry
	repeat
		-- each day:
		-- our guy finishes up day at 5pm, sleep 10pm
		local evening = varEpoch - math.fmod(varEpoch, 24*60*60) + 17*60*60
		local night = varEpoch - math.fmod(varEpoch, 24*60*60) + 22*60*60

		-- workday or weekend activity?
		local day = os.date("%a", varEpoch)
		local dataPool = "work"
		if day == "Sat" or day == "Sun" then
			dataPool = "weekend"
		end

		while varEpoch < evening do

			-- activity length for dayhours ~2hrs (+/- 1hr)
			local intervalSpan = math.floor(60*120*(0.5 + math.random()))

			-- utility function makes interval/tags
			local intTag = Util.IntervalBuild(varEpoch, intervalSpan, dataPool)

			TagCount(intTag["tags"])
			--print(intTag["interval"])
			print(intTag["undo"])
			writer:write(intTag["interval"].."\n")
			undoWriter:write(intTag["undo"])

			-- set new operating time with random-ish downtime
			varEpoch = varEpoch + intervalSpan + math.random(600,3600)

			-- eventual exit is during this period
			if varEpoch > End then
				Tags()
				os.exit()
			end
		end

		while varEpoch < night do
			-- activity length during evening, span ~1hr+-0.5
			local intervalSpan = math.floor(60*60*(0.5 + math.random()))
			local intTag = Util.IntervalBuild(varEpoch, intervalSpan, "freetime")
			TagCount(intTag["tags"])
			--print(intTag["interval"])
			print(intTag["undo"])
			writer:write(intTag["interval"].."\n")
			undoWriter:write(intTag["undo"])
			varEpoch = varEpoch + intervalSpan + math.random(300,3000)
		end

		-- activity sleep is 7-8hr
		local intervalSpan = math.floor((7 + math.random()) * 60 * 60)
		local startString = Util.FormatTime(varEpoch)
		local endString = Util.FormatTime(varEpoch + intervalSpan)
		local interval = "inc "..startString.." - "..endString.." # Sleeping "
		TagCount({"Sleeping"})
		local undoPrefix = "txn:\n  before:\n  after: "
		local undo = undoPrefix..'{"id":0,"start":"'..startString..'","end":"'..endString..'","tags:["Sleeping"]'


		if math.random(1,20) == 7 then
			interval = interval..'# "I feel refreshed"'
			undo = undo..',"annotation":"I feel refreshed"'
		end
		undo = undo..'}\n  type: interval\n'
		--print(interval)
		print(undo)
		writer:write(interval.."\n")
		undoWriter:write(undo)

		varEpoch = varEpoch + intervalSpan + math.random(300,3000)

	until os.date("%d", varEpoch) == "01"

	Generate(varEpoch)
end


-- handle backup and fresh instantiation of timew data before proceeding
function Init ()
	print("["..os.clock().."] Initializing...\n")

	local function backUp()
		local backups = io.popen('find "'..os.getenv("HOME")..'" -maxdepth 1 -name ".timewarrior_backup_*" -type d')
		local suffix = 0
		if backups then
			for _ in backups:lines() do
				suffix = suffix +1
			end
			BackUpDir = "/.timewarrior_backup_"..suffix
		end

		os.rename(TWPath, os.getenv("HOME")..BackUpDir)
	end

	-- check for extant data, possibility timew not installed
	if io.open(TWPath.."/timewarrior.cfg") == nil then
		if os.execute('command -v timew') == nil then
			error("Timewarrior not detected.")
		else
			print("Timewarrior installed but no data found.")
		end
	else
		backUp()
	end

	local timew = io.popen('timew', "w")
	assert(timew, "Couldn't call TimeWarrior.")
	timew:write("yes")
	timew:close()


	-- timezone giving me serious pain
	-- working with epoch seconds, avoid tablemaking?
	-- normalize anchor dates to middays, safe
	local nowNormal = nil
	if math.fmod(os.time(), 24*60*60) > 43200 then
		nowNormal =  os.time(os.date("!*t")) - math.fmod(os.time(), 24*60*60) + 12*60*60
	else
		-- midday prev day for night owl
		nowNormal = os.time(os.date("!*t")) - math.fmod(os.time(), 24*60*60) - 12*60*60
	end

	-- our guy is up and at them for 7 am.
	Start = nowNormal - Span*24*60*60 - 5*60*60
	End = nowNormal

	Generate(Start)
end


---- entry -----

-- handle args, ostensibly automate-able
--
assert(type((arg[1]) == "number") or (arg[1] == nil), "If provided, argument must be a number, representing days worth of data.")

if arg[1] then
	Span = arg[1] < 2001 and arg[1] or 2000 -- cap value
	Init()
else
	Configure()
end


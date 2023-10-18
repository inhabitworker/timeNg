#!/usr/bin/env lua

local Util = {}
local Phony = require 'data'

-- date format for timew
function Util.FormatTime(date)
	local dateString = os.date("%Y%m%d", date)
	local timeString = os.date("%H%M%S", date)
	return dateString.."T"..timeString.."Z"
end

-- uniq tag picker function
function Util.Pick(dataPool, tOrA, used)
	local select = math.random(1, #Phony[dataPool][tOrA])

	if used[1] ~= nil then
		for k,v in pairs(used) do
			if select == v then
				return Util.Pick(dataPool, tOrA, used)
			end
		end
	end

	return select
end

-- interval builder
function Util.IntervalBuild(varEpoch, intervalSpan, dataPool)
	local tags = {}
	local tagCount = math.random(1,3)

	local used = {}
	for _=1,tagCount do
		local select = Util.Pick(dataPool, "tags", used)
		table.insert(used, select)
		table.insert(tags, Phony[dataPool]["tags"][select])
	end

	-- start building string with formatted date
	local startString = Util.FormatTime(varEpoch)
	local endString = Util.FormatTime(varEpoch+intervalSpan)
	local interval = "inc "..startString.." - "..endString.." # "

	local undoPrefix = "txn:\n  type: interval\n  before:\n  after: "
	local undo = undoPrefix..'{"id":0,"start":"'..startString..'","end":"'..endString..'","tags:['

	-- append tags
	for _,tag in ipairs(tags) do
		if string.find(tag, "%s") ~= nil then
			interval = interval..'"'..tag..'"'.." "
		else
			interval = interval..tag.." "
		end

		undo = undo..'"'..tag..'",'
	end

	undo = string.sub(undo,1,string.len(undo)-1)..']'

	-- chance of append annotation
	if math.random(1,20) == 7 then
		local select = Util.Pick(dataPool, "annotations", {})
		local annotation = Phony[dataPool]["annotations"][select]
		interval = interval..'# "'..annotation..'"'
		undo = undo..',"annotation":"'..annotation..'"'
	end

	undo = undo..'}\n'

	local data = {interval=interval, undo=undo, tags=tags}

	return data
end

return Util

--[[ Notes...

			-- interval rules:
				-- if time during dayhours, time span ~2hrs+-1
				-- if time during evening, span ~1hr+-0.5
				-- if time reaches post-dayend, make 7-8hr sleep
					-- timew convention = interval anchore to date of start

DATE:
	YYYYMMDD\THHMMSS\Z

INTERVAL:
	{"id":int++,"start":"DATE","end":"DATE","tags":["tag","another tag string"],"annotation":"annotation string"}
	where annotation and tags are nullable, and end too but only for id:1

Files:

	[month].data:
		list of: inc DATE - DATE # tag "multi word tag" anothertag # "annotation string"

	tags.data:
		"{ list of "tag string":{"count":n}, }"

	undo.data:
		list of
			"txn:
				type: interval
				before: [ NOTHING | INTERVAL ]
				after: [ NOTHING | INTERVAL ]"

		txn often appends second set of these three fields, and type for everybody is interval

		start token creates (before: NOTHING) id:0 "virtual" interval record

		stop token has two sequential operations
			deleting (after: NOTHING) the open interval, but from target id:1
			re-inserting it with an end date as id:1

		modifications (tag, untag etc) perform similar as before but with target id

		delete token single operation
]]--

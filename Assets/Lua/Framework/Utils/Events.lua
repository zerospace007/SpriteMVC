--[[
Author:Chiuan
like Unity Brocast Event System in lua.
]]

local EventLib = require("Framework/Utils/Eventlib")

local Event = {}
local Events = {}

function Event.AddListener(event, handler)
	if not event or type(event) ~= "string" then
		Error("event parameter in addlistener function has to be string, " .. type(event) .. " not right.")
	end
	if not handler or type(handler) ~= "function" then
		Error("handler parameter in addlistener function has to be function, " .. type(handler) .. " not right")
	end

	if not Events[event] then
		--create the Event with name
		Events[event] = EventLib:new(event)
	end

	--conn this handler
	Events[event]:connect(handler)
end

function Event.Brocast(event,...)
	if not Events[event] then
		Error("brocast " .. event .. " has no event.")
	else
		Events[event]:fire(...)
	end
end

function Event.RemoveListener(event,handler)
	if not Events[event] then
		Error("remove " .. event .. " has no event.")
	else
		Events[event]:disconnect(handler)
	end
end

return Event
local eventDebug = false

local _bind_mt = {}
_bind_mt.__eq = function(a, b)
    if a._obj == b._obj and a._func == b._func then
        return true
    else
        return false
    end
    return true
end
    
_bind_mt.__call = function(tb, ...)
    if tb._func == nil then
        return
    end

    if tb._obj ~= nil then
        tb._func(tb._obj, ...)
    else
        tb._func(...)
    end
end
 
--bind function
local function BindFunction(obj, func)
    local set = {_obj = obj, _func = func}
    setmetatable(set, _bind_mt)
    return set
end

local function IsOriginBindFunction(set, obj, func)
    if set._obj == obj and set._func == func then
        return true
    else
        return false
    end
end

local EventSystem = {}
EventSystem.mEventCaches = {}

function EventSystem.AddEvent(eventType, obj, func)
    assert(type(eventType) == "string")
    if EventSystem.mEventCaches[eventType] == nil then
        EventSystem.mEventCaches[eventType] = {}
    end
    -- 查重
    local isRepeat = false
    local eventList = EventSystem.mEventCaches[eventType]
    for i, bindTable in ipairs(eventList) do
        if IsOriginBindFunction(bindTable, obj, func) then
            isRepeat = true
            break
        end
    end
    if not isRepeat then
        table.insert(EventSystem.mEventCaches[eventType], BindFunction(obj, func))
    end
end

function EventSystem.RemoveEvent(eventType, obj, func)
    assert(type(eventType) == "string")
    local eventList = EventSystem.mEventCaches[eventType]
    if eventList == nil then
        return
    end
    
    for key, value in ipairs(eventList) do
        if IsOriginBindFunction(value, obj, func) then
            table.remove(eventList, key)
            break
        end
    end
end

function EventSystem.SendEvent(eventType, ...)
    local eventList = EventSystem.mEventCaches[eventType]
    
    if eventList == nil then
        return
    end

    if eventDebug then
        print("trigger event : " .. eventType)
        dump(eventList)
    end
    
    for key, value in ipairs(eventList) do
        if value ~= nil then
            value(...)
        end
    end
end

function EventSystem.AddPersistentEvent(eventType, obj, func)
    local eventList = EventSystem.mEventCaches[eventType]

    if EventSystem.mEventCaches[eventType] == nil then
        EventSystem.mEventCaches[eventType] = {}
    end

    if eventDebug then
        print("add global event : " .. eventType)
        dump(eventList)
    end

    if eventList then
        return
    else
        table.insert(EventSystem.mEventCaches[eventType], BindFunction(obj, func))
    end
end

function EventSystem.HasEventCallBack(eventType)
    local eventList = EventSystem.mEventCaches[eventType]

    if eventList == nil or #eventList == 0 then
        return false
    end
    return true
end

return EventSystem

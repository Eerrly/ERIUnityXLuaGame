local behaviour = class()

function behaviour:ctor()
    self.actions = {}
    local _actions = self:GetActionList()
    for _, v in pairs(_actions) do
        self.actions[v] = action.new()
    end
    self:_RegisterEvent()
end

function behaviour:GetActionList()
    return {}
end

function behaviour:RegHandler(actionName, func)
    self.actions[actionName]:AddHandler(func)
end

function behaviour:Trigger(actionName, ...)
    self.actions[actionName]:Trigger(...)
end

function behaviour:GetEventList()
    return {}
end

function behaviour:AddListener(unityEvent, func)
    unityEvent:AddListener(function(...)
        func(...)
    end)
    if not self.unityEventListCache then
        self.unityEventListCache = {}
    end
    table.insert(self.unityEventListCache, unityEvent)
end

function behaviour:_RegisterEvent()
    local _events = self:GetEventList()
    for eventName, func in pairs(_events) do
        self["_EVENT_FUNCTION_"..eventName] = function(...)
            if self.gameObject.activeInHierarchy and self.___cs.enabled then
                func(...)
            end
        end
        require("EventSystem").AddEvent(eventName, self, self["_EVENT_FUNCTION_"..eventName])
    end
end

function behaviour:_UnregisterEvent()
    local _events = self:GetEventList()
    for eventName, func in pairs(_events) do
        if self["_EVENT_FUNCTION_"..eventName] then
            require("EventSystem").RemoveEvent(eventName, self, self["_EVENT_FUNCTION_"..eventName])
            self["_EVENT_FUNCTION_"..eventName] = nil
        end
    end
end

function behaviour:coroutine(func)
    return self:BehavCoroutine({
        co = coroutine.create(func);
        Resume = function(self, ...)
            local succ, ret = coroutine.resume(self.co, ...)
            if succ then
                return ret
            end
            error(ret)
        end
    })
end

function behaviour.getmt(base)
    return function(table, key)
        local cs = rawget(table, '___cs')
        if cs then
            local v = cs[key]
            if v then
                if type(v) == 'function' then
                    return function(obj, ...)
                        return v(cs, ...)
                    end
                end
                return v
            end
        end
        return base[key]
    end
end

function behaviour:_onDestroy()
    if self.unityEventListCache then
        for _, unityEvent in pairs(self.unityEventListCache) do
            unityEvent:RemoveAllListeners()
        end
        self.unityEventListCache = nil
    end
    self:_UnregisterEvent()
    self.___ex = nil
    self.___cs = nil
end

exports.behaviour = behaviour
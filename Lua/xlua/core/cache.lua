local cache = {}
local cacheData = {}
cacheData.data = {}
cacheData.listeners = {}

function cache.getPersistentDataPath()
    return CS.UnityEngine.Application.persistentDataPath
end

function cache.isPersistent(key)
    local value, persistent = cache.getLocalData(key)
    return persistent
end

-- save a key-value pair to the disk to make it persistent
function cache.save(key, persistentflag)
    if key == nil then return false end
    local info = cacheData.data[key]
    if not info then
        info = { }
        cacheData.data[key] = info
    end

    if persistentflag == 'snapshot' then
        info.snapshot = true
    else
        info.persistent = true
        local value = info.data
        cache.callListener(key, 'onSave')
        if value == nil or value == "" or persistentflag == false then
            -- value is nil to remove the data binded to the key
            CS.UnityEngine.PlayerPrefs.DeleteKey(key);
        else
            value = json.encode(value)
            if persistentflag == 'file' then
                CS.UnityEngine.PlayerPrefs.SetString(key, 'file')
                local f = io.open(cache.getPersistentDataPath() .. '/' .. key .. '.json', 'w')
                if f then
                    f:write(value)
                    f:flush()
                    f:close()
                end
            else
                CS.UnityEngine.PlayerPrefs.SetString(key, value)
            end
        end
        CS.UnityEngine.PlayerPrefs.Save()
    end
    return true
end

-- to remove a key-value pair from the disk to make it not persistent. But the cached data is still available
function cache.unsave(key)
    if key == nil then return end
    if not cache.isPersistent(key) then return end
    cacheData.data[key].persistent = false
    cache.callListener(key, 'onUnsave')
    CS.UnityEngine.PlayerPrefs.DeleteKey(key);
end

-- may need encrypt
function cache.getLocalData(key)
    if key == nil then
        return nil, false, true
    end
    local info = cacheData.data[key]
    if info then
        return info.data, info.persistent, info.readonly
    end
    local data = CS.UnityEngine.PlayerPrefs.GetString(key)
    if data == nil or data == "" then
        info = { persistent = false }
        cacheData.data[key] = info
        return nil, false, false
    else
        info = { }
        cacheData.data[key] = info
        if data == 'file' then
            data = nil
            local f = io.open(cache.getPersistentDataPath() .. '/' .. key .. '.json')
            if f then
                data = f:read("*a")
                f:close()
            end
        end
        data = json.decode(data)
        info.persistent = true
        info.data = data
        return data, true, false
    end
end

-- on writing persistent data, we must set the third param to true or it will fail!
function cache.setLocalData(key, value, persistent)
    if key == nil then return false end
    local oldValue, oldPersistent, isReadonly = cache.getLocalData(key)
    if isReadonly then
        return false
    end
    local isEqual = (oldValue == value)
    if isEqual then
        if persistent and type(oldValue) == "table" then
            isEqual = false
        end
    end

    if isEqual and persistent ~= oldPersistent then
        -- value not changed, only saving.
        cache.save(key, persistent)
        return true
    end
    if not isEqual then
        -- value changed
        local info = cacheData.data[key]
        if not info then
            info = { }
            cacheData.data[key] = info
        end
        info.data = value
        if persistent then
            cache.save(key, persistent)
        end
        if not persistent and value == nil then
            -- remove from the cache
            cacheData.data[key] = nil
        end
        cache.callListener(key, 'onChanged')
        -- else: value not changed and old persistent and calling persistent is the same. nothing happens.
    end

    return true
end

function cache.registerListener(key, listener, listenerName, eventName)
    if key == nil then return false end
    listenerName = listenerName or 'cacheDefaultListener'
    if listener == nil then
        if not eventName then
            local listeners = cacheData.listeners[key]
            if listeners then
                listeners[listenerName] = nil
                if not next(listeners) then
                    cacheData.listeners[key] = nil
                end
            end
        else
            local listeners = cacheData.listeners[key]
            if listeners then
                local info = listeners[listenerName]
                if info then
                    info[eventName] = nil
                    if not next(info) then
                        listeners[listenerName] = nil
                        if not next(listeners) then
                            cacheData.listeners[key] = nil
                        end
                    end
                end
            end
        end
    elseif type(listener) == 'function' then
        eventName = eventName or 'onChanged'
        local listeners = cacheData.listeners[key]
        if not listeners then
            listeners = { }
            cacheData.listeners[key] = listeners
        end
        local info = listeners[listenerName]
        if not info then
            info = { }
            listeners[listenerName] = info
        end
        info[eventName] = listener
    elseif type(listener) == 'table' then
        if eventName then
            if listener[eventName] then
                return cache.registerListener(key, listener[eventName], listenerName, eventName)
            end
        else
            local listeners = cacheData.listeners[key]
            if not listeners then
                listeners = { }
                cacheData.listeners[key] = listeners
            end
            listeners[listenerName] = listener
        end
    end
    return true
end

function cache.callListener(key, eventName)
    local listeners = cacheData.listeners[key]
    if listeners then
        for k, v in pairs(listeners) do
            local func = v
            if type(v) == "table" then
                func = v[eventName]
            end
            if type(func) == "function" then
                local value = cache.getLocalData(key)
                func(key, value, k, eventName)
            end
        end
    end
end

exports.cache = cache
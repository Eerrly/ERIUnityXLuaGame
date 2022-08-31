local unpack = unpack or table.unpack

local function async_to_sync(async_func, callback_pos)
    return function(...)
        local _co = coroutine.running() or error ('this function must be run in coroutine')
        local rets
        local waiting = false
        local function cb_func(...)
            if waiting then
                assert(coroutine.resume(_co, ...))
            else
                rets = {...}
            end
        end
        local params = {...}
        if #params > 0 then
            table.insert(params, callback_pos or (#params + 1), cb_func)
            async_func(unpack(params))
        else
            async_func(nil, cb_func)
        end

        if rets == nil then
            waiting = true
            rets = {coroutine.yield()}
        end

        return unpack(rets)
    end
end

local function coroutine_call(func)
    local co = coroutine.create(function(...)
        local params = {...}
        xpcall(function() func(unpack(params)) end, function(err) dumpe(err) end)
    end)
    assert(coroutine.resume(co))
    return co
end

local cs_coroutine_runner
local function get_coroutine_runner()
    if not cs_coroutine_runner or res.isNull(cs_coroutine_runner) then
        local gameobject = CS.UnityEngine.GameObject("Coroutine_Runner")
        res.DontDestroyOnLoad(gameobject)
        cs_coroutine_runner = gameobject:AddComponent(typeof(CS.Coroutine_Runner))
    end
    return cs_coroutine_runner
end

local function async_yield_return(to_yield, cb)
    get_coroutine_runner():YieldAndCallback(to_yield, cb)
end

local coroutine_yield = async_to_sync(async_yield_return)

local waitForEndOfFrame = CS.UnityEngine.WaitForEndOfFrame()
local waitForEndOfFrameIndex = -1

local function coroutine_yield_waitForEndOfFrame()
    local curFrame = CS.UnityEngine.Time.frameCount
    if waitForEndOfFrameIndex ~= curFrame then
        coroutine_yield(waitForEndOfFrame)
        waitForEndOfFrameIndex = curFrame
    end
end

local function coroutine_yield_waitForNextEndOfFrame()
    coroutine_yield(waitForEndOfFrame)
    local curFrame = CS.UnityEngine.Time.frameCount
    waitForEndOfFrameIndex = curFrame
end

exports.unpack = unpack
exports.async_to_sync = async_to_sync
exports.coroutine_call = coroutine_call
exports.coroutine_yield = coroutine_yield
exports.coroutine_yield_waitForEndOfFrame = coroutine_yield_waitForEndOfFrame
exports.coroutine_yield_waitForNextEndOfFrame = coroutine_yield_waitForNextEndOfFrame
-- exports.json = require("rapidjson")
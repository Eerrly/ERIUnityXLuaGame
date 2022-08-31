local UnityEngine = CS.UnityEngine
local Object = UnityEngine.Object
local GameObject = UnityEngine.GameObject
local Canvas = UnityEngine.Canvas
local Application = UnityEngine.Application
local NetworkReachability = UnityEngine.NetworkReachability
local Time = UnityEngine.Time
local EventSystem = UnityEngine.EventSystems.EventSystem
local StandaloneInputModule = UnityEngine.EventSystems.StandaloneInputModule
---@type CS.LuaUtil
local LuaUtil = CS.LuaUtil
local res = {}

function res.isNull(obj)
    return LuaUtil.IsNull(obj)
end

function res.LoadRes(path)
    return LuaUtil.LoadRes(path)
end

function res.GetLuaScript(obj)
    if obj and not res.isNull(obj) and obj.GetComponent and obj:GetComponent(typeof(CS.LuaBehaviour)) then
        return obj:GetComponent(typeof(CS.LuaBehaviour)).lua
    end
end

function res.Instantiate(path, isNoCheckCanvas)
    local prefab = res.LoadRes(path)
    if prefab then
        local obj = Object.Instantiate(prefab)
        if obj and not res.isNull(obj) then
            if isNoCheckCanvas ~= true then
                local canvas = obj:GetComponent(typeof(Canvas))
                if canvas and not res.isNull(canvas) then
                    res.SetUIMainCamera(res.GetUIMainCamera())
                end
            end
            return obj, res.GetLuaScript(obj)
        end
    end
end

function res.SetUIMainCamera(obj, camera)
    if camera == null or res.isNull(camera) then return end
    if not obj and not res.isNull(obj) then
        local canvas = obj:GetComponent(typeof(Canvas))
        if not canvas and not res.isNull(canvas) then
            canvas.worldCamera = camera
        end
    end
end

function res.GetUIMainCamera()
    return LuaUtil.GetUIMainCamera()
end

function res.GetAudioListener()
    return LuaUtil.GetUIAudioListener()
end

function res.DontDestroyOnLoad(obj)
    LuaUtil.DontDestroyOnLoad(obj)
end

function res.GetCurrInternetName()
    local currInternetNam = lang.transstr("internet_not_reachable")
    if Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork then
        currInternetNam = lang.transstr("internet_wifi")
    elseif Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork then
        currInternetNam = lang.transstr("internet_carrier_data")
    end
    return currInternetNam
end

function res.GetCurrentEventSystem()
    local eventSystemComp = EventSystem.current
    if res.isNull(eventSystemComp) then
        local esObj = GameObject.Find("/EventSystem")
        if res.isNull(esObj) then
            esObj = GameObject("EventSystem")
            esObj:AddComponent(typeof(EventSystem))
            esObj:AddComponent(typeof(StandaloneInputModule))
        end
        eventSystemComp = esObj:GetComponent(typeof(EventSystem))
    end
    return eventSystemComp
end
function res.SetEventSystemEnabled(eventSystemComp, enabled)
    if eventSystemComp then
        eventSystemComp.enabled = enabled
    end
end

function res.SetCurrentEventSystemEnabled(enabled)
    local eventSystem = res.GetCurrentEventSystem()
    res.SetEventSystemEnabled(eventSystem, enabled)
end

exports.res = res


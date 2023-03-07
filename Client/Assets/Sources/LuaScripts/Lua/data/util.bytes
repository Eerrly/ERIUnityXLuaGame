---@type CS.LuaUtil
local LuaUtil = CS.LuaUtil

local function CreateWindow(parentId, path, layer, args, callback)
    return LuaUtil.CreateWindow(parentId, path, layer, args, callback)
end

local function DestroyWindow(windowId, destroy)
    return LuaUtil.DestroyWindow(windowId, destroy)
end

local function IsNull(obj)
    return LuaUtil.IsNull(obj)
end

local function DontDestroyOnLoad(obj, isDontDestroy)
    LuaUtil.DontDestroyOnLoad(obj, isDontDestroy)
end

local function LoadScene(scene, callback)
    LuaUtil.LoadScene(scene, callback)
end

local function Patching(url, self, callback)
    LuaUtil.Patching(url, self, callback)
end

local function ClearUICache()
    LuaUtil.ClearUICache()
end

local function SetSelfPlayerId(id)
    LuaUtil.SetSelfPlayerId(id)
end

exports.Util = {
    CreateWindow = CreateWindow,
    IsNull = IsNull,
    DontDestroyOnLoad = DontDestroyOnLoad,
    LoadScene = LoadScene,
    DestroyWindow = DestroyWindow,
    Patching = Patching,
    ClearUICache = ClearUICache,
    SetSelfPlayerId = SetSelfPlayerId,
}
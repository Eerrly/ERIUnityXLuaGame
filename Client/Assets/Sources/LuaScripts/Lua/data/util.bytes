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

local function HttpGet(url, timeout, callback)
    LuaUtil.HttpGet(url, timeout, callback)
end

local function HttpDownload(url, path, progress, callback)
    LuaUtil.HttpDownload(url, path, progress, callback)
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
    HttpGet = HttpGet,
    HttpDownload = HttpDownload,
    ClearUICache = ClearUICache,
    SetSelfPlayerId = SetSelfPlayerId,
}
local LuaUtil = CS.LuaUtil

local function CreateWindow(parentId, path, layer, property, args, callback)
    return LuaUtil.CreateWindow(parentId, path, layer, property, args, callback)
end

local function IsNull(obj)
    return LuaUtil.IsNull(obj)
end

local function DontDestroyOnLoad(obj, isDontDestroy)
    LuaUtil.DontDestroyOnLoad(obj, isDontDestroy)
end

exports.CreateWindow = CreateWindow
exports.IsNull = IsNull
exports.DontDestroyOnLoad = DontDestroyOnLoad
local LuaUtil = CS.LuaUtil

local function CreateWindow(parentId, path, layer, property, args, callback)
    LuaUtil.CreateWindow(parentId, path, layer, property, args, callback)
end

exports.CreateWindow = CreateWindow
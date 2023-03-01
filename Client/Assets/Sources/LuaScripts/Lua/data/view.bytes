
---@class WindowType
local WindowType = {
    normal = "normal",
    fixed = "fixed",
    popup = "popup"
}

local UI_NORMAL_MIN_LAYER = 100
local UI_POPUP_MIN_LAYER = 300

local templates = {}
local scriptNameToTemplateID = {}

local allWindows = {}
local normalWindows = {}
local popupWindows = {}
local fixedWindows = {}

local function _GetWindowArray(windowType)
    if windowType == WindowType.normal then
        return normalWindows
    elseif windowType == WindowType.fixed then
        return fixedWindows
    elseif windowType == WindowType.popup then
        return popupWindows
    end
end

local function _GetLoadedWindow(windowId, windowType)
    local windows = _GetWindowArray(windowType)
    return table.find(windows, function(wnd) return wnd.id == windowId end)
end

local function _FindWindow(windowsArray, windowId)
    return table.find(windowsArray, function(window) return window.id == windowId end)
end

local function Initialize(config)

    templates = {}
    scriptNameToTemplateID = {}

    allWindows = {}
    normalWindows = {}
    popupWindows = {}
    fixedWindows = {}

    local BuildWindow = function(scriptName, windowType, parentScriptName, modelScriptName, prefabPath)
        if exports[scriptName] == nil then
            local fileViewPath = "ui/view/" .. scriptName
            exports[scriptName] = require(fileViewPath)
        end
        local id = #templates + 1
        table.insert(templates, {
            id = id,
            scriptName = scriptName,
            windowType = windowType,
            modelScriptName = modelScriptName,
            parentScriptName = parentScriptName,
            prefabPath = prefabPath,
        })
        scriptNameToTemplateID[scriptName] = #templates
    end

    for _, params in pairs(config) do
        BuildWindow(params[1], params[2], params[3], params[4], params[5])
    end

    for _, tmp in pairs(templates) do
        if tmp.windowType == WindowType.normal and tmp.parentScriptName ~= nil then
            tmp.parentWndId = templates[scriptNameToTemplateID[tmp.parentScriptName]]
        end
    end

    return templates
end

local function _DestroyWindow(windowId, destory)
    local window, index = table.find(allWindows, function(_window) return _window.id == windowId end)
    if window then
        Util.DestroyWindow(windowId, destory)
    end
end

local function _OnWindowCreate(window, instance, id, params)
    local template = templates[params.id]

    window.loaded = true
    instance.loaded = true
    
    instance.Close = function()
        if template.windowType == WindowType.popup then
            for i = #popupWindows, 1, -1 do
                if popupWindows[i].id == id then
                    _DestroyWindow(popupWindows[i].id, false)
                    table.remove(popupWindows, i)
                    break
                end
            end
        else
            View.Hide(params.id)
        end
    end

    if instance.onCreate then
        instance:onCreate()
    end
end

local function _CreateWindow(args)
    local class = exports[args.scriptName]
    if not class or not class.new then
        error("[UIManager] class(" .. args.name .. ") == null")
    end

    local instance = class.new()
    instance.Params = args.params
    instance.loaded = false
    instance.__isViewInstance = true

    local window = { 
        id = args.id, 
        scriptName = args.scriptName, 
        modelScriptName = args.modelScriptName, 
        parentScriptName = args.parentScriptName, 
        prefabPath = args.prefabPath,
        instance = instance, 
        loaded = false 
    }
    local id = Util.CreateWindow(args.parentWndId and args.parentWndId or -1, args.prefabPath, args.layer, instance,
            function(id) _OnWindowCreate(window, instance, id, args) end)
    window.id = id
    table.insert(allWindows, window)

    return window
end

local function _ShowNormalWindow(template, params, parentParams)
    if template.parentWndId then
        local parentWindow, parentIndex = _FindWindow(normalWindows, template.parentWndId)
        if parentWindow then
            for i = #normalWindows, parentIndex + 1, -1 do
                View.Hide(normalWindows[i].id)
            end
        else
            View.Show(parentWindow.id, parentParams and parentParams[parentWindow.id] or nil, parentParams)
        end
    else
        for i=#normalWindows, 1, -1 do
            View.Hide(normalWindows[i].id)
        end
    end
    table.insert(normalWindows, _CreateWindow({
        id = template.id,
        scriptName = template.scriptName,
        layer = UI_NORMAL_MIN_LAYER + #normalWindows,
        modelScriptName = template.modelScriptName,
        parentScriptName = template.parentScriptName,
        prefabPath = template.prefabPath,
        params = params,
    }))
end

local function _ShowPopWindow(template, params)
    table.insert(popupWindows, _CreateWindow({
        id = template.id,
        scriptName = template.scriptName,
        layer = UI_POPUP_MIN_LAYER + #popupWindows * 10,
        modelScriptName = template.modelScriptName,
        parentScriptName = template.parentScriptName,
        prefabPath = template.prefabPath,
        params = params,
    }))
end

local function Hide(windowId)
    local template = assert(templates[windowId], string.format("windowId = %s not exist", tostring(windowId)))
    local windows
    if template.windowType == WindowType.fixed then
        windows = fixedWindows
    elseif template.windowType == WindowType.normal then
        windows = normalWindows
    elseif template.windowType == WindowType.popup then
        windows = popupWindows
    end
    local found = table.any(windows, function(wnd) return wnd.id == windowId end)
    if found then
        for i = #windows, 1, -1 do
            local window = windows[i]
            if window.id == windowId then
                _DestroyWindow(window.id, false)
                table.remove(windows, i)
                return
            else
                _DestroyWindow(window.id, false)
                table.remove(windows, i)
            end
        end
    end
end

local function Show(templateID, params, parentParams)
    local template = assert(templates[templateID], string.format("templateID = %s not exist", tostring(templateID)))
    if template.windowType == WindowType.fixed then
    elseif template.windowType == WindowType.normal then
        _ShowNormalWindow(template, params, parentParams)
    elseif template.windowType == WindowType.popup then
        _ShowPopWindow(template, params)
    end
end

local function HideAll(destroy)
    local allWindows = { normalWindows, popupWindows, fixedWindows }
    for n=1, #allWindows do
        for i=#allWindows[n], 1, -1 do
            _DestroyWindow(allWindows[n][i].id, destroy)
            table.remove(allWindows[n], i)
        end
    end
    if destroy then
        Util.ClearUICache()
    end
end

exports.View = {
    Initialize = Initialize,
    Show = Show,
    Hide = Hide,
    HideAll = HideAll,
}
local CreateWindow = CreateWindow
local DestroyWindow = DestroyWindow

---@class WindowTemplate
---@field templateID number
---@field name string
---@field type WindowType
---@field layer string|number
---@field modelName string|string[]
---@field property number
---@field prefab string
---@field parent WindowTemplate

---@class ViewBase
---@field View CS.LuaBehaviour
---@field model ModelBase
---@field sharedData table
---@field Params any
---@field parentInst ViewBase
---@field loaded boolean

---@class Window
---@field id number
---@field templateID number
---@field name string
---@field instance ViewBase
---@field loaded boolean

---@class WindowType
local WindowType = {
    normal = "normal",
    fixed = "fixed",
    popup = "popup"
}

local allWindows ---@type Window[]
local normalWindows ---@type Window[]
local popupWindows ---@type Window[]
local fixedWindows ---@type Window[]

---@return Window[]
local function _GetWindowArray(windowType)
    if windowType == WindowType.normal then
        return normalWindows
    elseif windowType == WindowType.fixed then
        return fixedWindows
    elseif windowType == WindowType.popup then
        return popupWindows
    end
end

---@return Window
local function _GetLoadedWindow(templateID, windowType)
    local windows = _GetWindowArray(windowType)
    return table.find(windows, function(wnd) return wnd.templateID == templateID end)
end

---@param windowsArray Window[]
---@param templateID number
---@return Window|number
local function _FindWindow(windowsArray, templateID)
    return table.find(windowsArray, function(window) return window.templateID == templateID end)
end

---@param args CreateWindowParams
---@return Window
local function _CreateWindow(args)
    local class = exports[args.name]
    if class then
        if not class.New then
            error("[UIManager] class(" .. args.name .. ").New == null")
        end
    else
        error("[UIManager] class(" .. args.name .. ") == null")
    end

    local instance = class.New()
    instance.Params = args.params
    instance.loaded = false
    instance.__isViewInstance = true

    if instance.OnPrevCreate then
        instance:OnPrevCreate()
    end

    local window = { templateID = args.templateID, name = args.name, modelName = args.modelName, instance = instance, loaded = false }
    local id = CreateWindow(args.parentID and args.parentID or -1, args.prefab or args.name, args.layer, args.property, instance,
            function(id) _OnWindowCreate(window, instance, id, args) end)
    window.id = id
    table.insert(allWindows, window)

    return window
end

---@param tempalte WindowTemplate
---@param params any[]
local function _ShowFixed(template, params)
    for i = 1, #fixedWindows do
        if fixedWindows[i].templateID == template.templateID then
            warning(template.name..' is showing')
            return
        end
    end

    local window = _CreateWindow{
        name = template.name,
        templateID = template.templateID,
        layer = template.layer,
        modelName = template.modelName,
        property = template.property,
        params = params,
        prefab = template.prefab
    }
    table.insert(fixedWindows, window)
end

---@type WindowTemplate[]
local templates

---@param templateID number
---@param params any[]
---@param parentParams any[]
local function Show(templateID, params, parentParams)
    ---@type WindowTemplate
    local template = assert(templates[templateID], string.format("templateID = %s not exist", tostring(templateID)))
    if template.type == WindowType.fixed then
        _ShowFixed(template, params)
    elseif template.type == WindowType.normal then
        _ShowNormal(template, params, parentParams)
    elseif template.type == WindowType.popup then
        _ShowPopup(template, params, parentParams)
    end
end
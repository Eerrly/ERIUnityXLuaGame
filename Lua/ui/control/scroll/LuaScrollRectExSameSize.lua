local UnityEngine = CS.UnityEngine
local Object = UnityEngine.Object
local UI = UnityEngine.UI
local ScrollRect = UI.ScrollRect
local Vector2 = UnityEngine.Vector2
local ScrollRectExSameSize = CS.XLua.UI.ScrollRectExSameSize
local Direction = ScrollRectExSameSize.Direction

local GameObjectHelper = require("ui/common/GameObjectHelper")
local LuaScrollRectExSameSizeBase = require("ui/control/scroll/LuaScrollRectExSameSizeBase")
local LuaScrollRectExSameSize = class(LuaScrollRectExSameSizeBase)

-- A common script to use with ScrollViewExSameSize

-- requires one of the following extras:
--   itemRes: Game Object of the item template, should be child of it's Content Transform
--   itemResPath: path of the item prefab

-- use LuaScrollRectExSameSize:RegOnItemButtonClick to reg a callback for button on item

function LuaScrollRectExSameSize:ctor()
    LuaScrollRectExSameSize.super.ctor(self)

    self.itemResPath = self.___ex.itemResPath
    self.itemRes = self.___ex.itemRes

    self.itemLinePath = self.___ex.itemLinePath
    self.itemLine = self.___ex.itemLine

    assert((self.itemResPath == nil) ~= (self.itemRes == nil))

    GameObjectHelper.FastSetActive(self.itemRes, false)

    self.onItemButtonClicks = {}
end

function LuaScrollRectExSameSize:GetItemRes()
    if not self.itemRes and self.itemResPath then
        self.itemRes = res.LoadRes(self.itemResPath)
    end
    return self.itemRes
end

function LuaScrollRectExSameSize:ResetItemRes(path)
    self.itemResPath = path
    self.itemRes = res.LoadRes(self.itemResPath)
end

function LuaScrollRectExSameSize:createItem(index)
    local itemRes = self:GetItemRes()
    local obj
    if itemRes == nil or res.isNull(itemRes) then return end
    if index == 1 and itemRes.transform.parent then
        -- use the template if it's already in the view hierarchy
        obj = itemRes
    else
        obj = Object.Instantiate(itemRes)
    end

    GameObjectHelper.FastSetActive(obj, true)
    local spt = res.GetLuaScript(obj)

    if spt then
        self:resetItem(spt, index)
    end
    return obj
end

function LuaScrollRectExSameSize:resetItem(spt, index)
    local data = self.data[index]
    for name, func in pairs(self.onItemButtonClicks) do
        local button = spt[name]
        button.onClick:RemoveAllListeners()
        button.onClick:AddListener(
            function()
                func(data, index)
            end
        )
    end
    spt:InitView(data, index, unpack(self.args, 1, self.argc))
    self:updateItemIndex(spt, index)
end

function LuaScrollRectExSameSize:InitView(model, ...)
    self.data = model
    self.args = {...}
    self.argc = select("#", ...)
    self:refresh(self.data)
end

function LuaScrollRectExSameSize:RegOnItemButtonClick(buttonName, func)
    -- save func to table
    self.onItemButtonClicks[buttonName] = func
end

function LuaScrollRectExSameSize:UpdateItem(index, itemModel)
    self.data[index] = itemModel
    local spt = self:getItem(index)
    if spt ~= nil then
        self:resetItem(spt, index)
    end
end

function LuaScrollRectExSameSize:GetScrollRect()
    return self:GetComponent(typeof(CS.UnityEngine.UI.ScrollRect))
end

function LuaScrollRectExSameSize:GetScrollNormalizedPosition()
    local scrollRect = self:GetScrollRect()
    assert(scrollRect.horizontal ~= scrollRect.vertical)

    if scrollRect.horizontal then
        return scrollRect.horizontalNormalizedPosition
    elseif scrollRect.vertical then
        return scrollRect.verticalNormalizedPosition
    end
end

function LuaScrollRectExSameSize:SetScrollNormalizedPosition(scrollNormalizedPosition)
    local scrollRect = self:GetScrollRect()
    assert(scrollRect.horizontal ~= scrollRect.vertical)

    if scrollRect.horizontal then
        scrollRect.horizontalNormalizedPosition = scrollNormalizedPosition
    elseif scrollRect.vertical then
        scrollRect.verticalNormalizedPosition = scrollNormalizedPosition
    end
end

return LuaScrollRectExSameSize

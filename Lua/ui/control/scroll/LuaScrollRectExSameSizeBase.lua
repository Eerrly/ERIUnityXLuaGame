local UnityEngine = CS.UnityEngine
local Object = UnityEngine.Object
local Vector2 = UnityEngine.Vector2
local ScrollRectExSameSize = CS.Eerrly.ScrollRectExSameSize
local Direction = ScrollRectExSameSize.Direction

local LuaScrollRectExSameSizeBase = class(behaviour)

function LuaScrollRectExSameSizeBase:ctor()
    LuaScrollRectExSameSizeBase.super.ctor(self)
    self.cScroll = self.gameObject:GetComponent(typeof(ScrollRectExSameSize))
    self.itemDatas = {}
    self.onItemIndexChangedCallback = nil
    self.onScrollPositionChangedCallback = nil
    ndump("LuaScrollRectExSameSizeBase:ctor", self.cScroll, ScrollRectExSameSize)
end

-- scrollPos 滑动到指定位置不用刷新之前位置数据
function LuaScrollRectExSameSizeBase:refresh(data, scrollPos)
    if type(data) ~= "table" then
        data = self.itemDatas
    end
    self:clearData()
    self.itemDatas = data
    if scrollPos then
        self.cScroll:RefreshWithItemCountByScrollPos(#self.itemDatas, scrollPos)
    else
        self.cScroll:RefreshWithItemCount(#self.itemDatas)
    end
end

function LuaScrollRectExSameSizeBase:calcCellCount()
    self.cScroll:CalcCellCount()
end

function LuaScrollRectExSameSizeBase:recalcCellCountWithViewSize(width, height)
    self.cScroll:RecalcCellCountWithViewSize(width, height)
end

function LuaScrollRectExSameSizeBase:scrollToCell(index)
    self.cScroll:ScrollToCell(index - 1)
end

function LuaScrollRectExSameSizeBase:scrollToCellEx(index)
    self.cScroll:ScrollToCellEx(index - 1)
end

function LuaScrollRectExSameSizeBase:scrollToCellImmediate(index)
    self.cScroll:ScrollToCellImmediate(index - 1)
end

function LuaScrollRectExSameSizeBase:scrollToPreviousGroup()
    self.cScroll:ScrollToPreviousGroup()
end

function LuaScrollRectExSameSizeBase:scrollToNextGroup()
    self.cScroll:ScrollToNextGroup()
end

function LuaScrollRectExSameSizeBase:scrollToPosImmediate(normalizedPos)
    self.cScroll:ScrollToPosImmediate(normalizedPos)
end

function LuaScrollRectExSameSizeBase:getScrollNormalizedPos()
    return self.cScroll:GetScrollToPosNormalizedPos()
end

function LuaScrollRectExSameSizeBase:getItem(index)
    local selectItem = self.cScroll:GetSelectItem(index)
    if selectItem == nil then
        return nil
    else
        local item = selectItem.gameObject
        return item and item:GetComponent(typeof(CS.LuaBehaviour)) and item:GetComponent(typeof(CS.LuaBehaviour)).lua
    end
end

function LuaScrollRectExSameSizeBase:addItem(data, index)
    if not index then
        index = #self.itemDatas + 1
    end
    if type(index) ~= "number" or index < 1 or index > #self.itemDatas + 1 then
        dump(format("Invalid index: %s", index))
        return
    end

    table.insert(self.itemDatas, index, data)
    self.cScroll:AddItem(index - 1)
end

function LuaScrollRectExSameSizeBase:removeItem(index)
    if not index then
        index = #self.itemDatas
    end
    if type(index) ~= "number" or index < 1 or index > #self.itemDatas then
        dump(format("Invalid index: %s", index))
        return
    end

    table.remove(self.itemDatas, index)
    self.cScroll:RemoveItem(index - 1)
end

function LuaScrollRectExSameSizeBase:removeAll()
    self.cScroll:RemoveAllItem()
    self.itemDatas = {}
end

function LuaScrollRectExSameSizeBase:clearData()
    self.cScroll:ClearData()
    self.itemDatas = {}
end

function LuaScrollRectExSameSizeBase:getMaxPerLine()
    return self.cScroll:GetMaxPerLine()
end

function LuaScrollRectExSameSizeBase:createLine(index)
    local direction = self.cScroll:GetDirection()
    local obj
    if direction == Direction.Horizontal then
        obj = self:getLineObj()
        obj.transform.sizeDelta = Vector2(self.cScroll:GetLineSpace(), self.transform.rect.height)
    elseif direction == Direction.Vertical then
        obj = self:getLineObj()
        obj.transform.sizeDelta = Vector2(self.transform.rect.width, self.cScroll:GetLineSpace())
    end
    return obj
end

function LuaScrollRectExSameSizeBase:getLineObj()
    if self.itemLine ~= nil then
        return Object.Instantiate(self.itemLine)
    elseif self.itemLinePath ~= nil then
        return res.Instantiate(self.itemLinePath)
    else
        local direction = self.cScroll:GetDirection()
        local path
        if direction == Direction.Horizontal then
            path = "Assets/CapstonesRes/Game/UI/Common/Template/Line/V_Line.prefab"
        elseif direction == Direction.Vertical then
            path = "Assets/CapstonesRes/Game/UI/Common/Template/Line/H_Line.prefab"
        end
        return res.Instantiate(path)
    end
end

function LuaScrollRectExSameSizeBase:regOnCreateLine(func)
    if type(func) == "function" then
        self.createLine = func
    end
end

function LuaScrollRectExSameSizeBase:unregOnCreateLine()
    self.createLine = nil
end

function LuaScrollRectExSameSizeBase:regOnCreateItem(func)
    if type(func) == "function" then
        self.createItem = func
    end
end

function LuaScrollRectExSameSizeBase:unregOnCreateItem()
    self.createItem = nil
end

function LuaScrollRectExSameSizeBase:regOnResetItem(func)
    if type(func) == "function" then
        self.resetItem = func
    end
end

function LuaScrollRectExSameSizeBase:unregOnResetItem()
    self.resetItem = nil
end

function LuaScrollRectExSameSizeBase:updateItemIndex(spt, index)
    spt.getIndex = function(self)
        return index
    end
end

function LuaScrollRectExSameSizeBase:regOnItemIndexChanged(func)
    if type(func) == "function" then
        self.onItemIndexChangedCallback = func
    end
end

function LuaScrollRectExSameSizeBase:unregOnItemIndexChanged()
    self.onItemIndexChangedCallback = nil
end

function LuaScrollRectExSameSizeBase:onItemIndexChanged(index)
    if type(self.onItemIndexChangedCallback) == "function" then
        self.onItemIndexChangedCallback(index)
    end
end

function LuaScrollRectExSameSizeBase:regOnScrollPositionChanged(func)
    if type(func) == "function" then
        self.onScrollPositionChangedCallback = func
    end
end

function LuaScrollRectExSameSizeBase:unregOnScrollPositionChanged()
    self.onScrollPositionChangedCallback = nil
end

function LuaScrollRectExSameSizeBase:onScrollPositionChanged(position)
    if type(self.onScrollPositionChangedCallback) == "function" then
        self.onScrollPositionChangedCallback(position)
    end
end

function LuaScrollRectExSameSizeBase:ResetWithCellSize(width, height)
    self.cScroll:ResetWithCellSize(width, height)
end

function LuaScrollRectExSameSizeBase:ResetWithViewSize(width, height)
    self.cScroll:ResetWithViewSize(width, height)
end

function LuaScrollRectExSameSizeBase:ResetWithCellSpace(width, height)
    self.cScroll:ResetWithCellSpace(width, height)
end

function LuaScrollRectExSameSizeBase:destroyItem(index)
end

return LuaScrollRectExSameSizeBase

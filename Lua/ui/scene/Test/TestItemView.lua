local TestItemView = class()

function TestItemView:ctor()
    self.nameTxt = self.___ex.nameTxt
end

function TestItemView:InitView(itemData, index)
    ndump("TestItemView:InitView", itemData, index)
    self.nameTxt.text = itemData.name
end

return TestItemView
local EventSystem = require("EventSystem")
local Color = CS.UnityEngine.Color
local TestView = class(behaviour)

local RedColor = Color.red
local WhiteColor = Color.white

function TestView:ctor()
    ndump("TestView:ctor", behaviour, self)
    TestView.super.ctor(self)
    self.haha = self.___ex.haha
    self.button = self.___ex.button
    self.scrollView = self.___ex.scrollView
end

function TestView:GetEventList()
    return {
        ["SET_IMAGE_COLOR"] = self.SetImageColor,
    }
end

function TestView:start()
    self:AddListener(self.button.onClick, function()
        print("OnButtonClick obj param : " .. tostring(obj))
        self:TestCoroutine()
    end)
end

function TestView:InitView(model)
    self.model = model
    self.itemList = self.model:GetItemList()
    self.scrollView.lua:InitView(self.itemList)
    ndump("TestView:InitView", self, self.model:GetName(), self.model:GetAge(), self.model:GetSex())
    
    self:TestJson()
end

function TestView:TestCoroutine()
    coroutine_call(function()
        print("TestView:TestCoroutine coroutine_call")
        coroutine_yield(CS.UnityEngine.WaitForEndOfFrame())
        print("TestView:TestCoroutine coroutine_yield")
        EventSystem.SendEvent("SET_IMAGE_COLOR", "This is EventSystem function params ..")
    end)
    local uiImageObj = res.Instantiate("Prefabs/TestObj")
end

function TestView:SetImageColor(...)
    ndump("TestView:SetImageColor", ...)
    self.haha:SetAtlasSprite("Assets/Resources/Images/SpringRedPackage/SpringRedPackage_1.png")
    self.haha.color = self.haha.color == WhiteColor and RedColor or WhiteColor
end

function TestView:TestJson()
    local t = json.decode('{"a":123}')
    ndump("TestView:TestJson", t.a)
end

return TestView
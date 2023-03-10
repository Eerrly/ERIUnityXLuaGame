local Const = require("ui/const")

local viewA = class("viewa")

function viewA:onCreate()
    print("-------- viewA:onCreate --------")
    self:refresh()
end

function viewA:refresh()
    print("refresh ID.Text:" .. tostring(self.ID.Text))
    self.View:SetText(self.ID.Text, "SB")
    self.View:SetImage(self.ID.Image1, "Textures/B", "item_01.png")
    self.View:SetImage(self.ID.Image2, "Textures/A", "gongchengshi.png")
    self.View:SetImage(self.ID.Image3, "Textures/A", "golden_finger.png")

    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Start, self.OnBtnStartClicked)
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Get, self.OnBtnGetClicked)
end

function viewA:OnBtnGetClicked()
    Util.Patching("http://192.168.31.219:8080/pres", self, self.OnPatchingEvent)
end

function viewA:OnPatchingEvent(event, arg1, arg2)
    if event == "ready" then
        print("OnPatchingEvent ready !!!")
    elseif event == "download" then
        print("OnPatchingEvent download !!! arg1:" .. tostring(arg1))
    elseif event == "done" then
        print("OnPatchingEvent done !!!")
    end
end

function viewA:OnBtnStartClicked()
    print("Click Button !!!!")
    View.Show(2)
end

return viewA
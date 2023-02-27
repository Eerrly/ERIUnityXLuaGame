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
    Util.HttpGet("http://freestyle2cdn.changyou.kr/resources/FreeStyle/1.120.1/version_2002.txt", 5, function(state, text)
        print("state:" .. tostring(state) .. ", text:\n" .. text)
    end)
end

function viewA:OnBtnStartClicked()
    print("Click Button !!!!")
    View.Show(2)
    -- Util.LoadScene("Battle", function(state, progress)
    --     self:onloadedscene(state, progress)
    -- end)
end

return viewA
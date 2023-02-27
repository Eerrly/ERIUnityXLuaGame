local Const = require("ui/Const")

local ViewA = class("ViewA")

function ViewA:ctor()
    print("-------- ViewA:ctor --------")
    self.windowId = Util.CreateWindow(-1, "A", 1, self, function()
        self:refresh()
    end)
end

function ViewA:refresh()
    print("windowsId:" .. self.windowId .. ", ID.Text:" .. tostring(self.ID.Text))
    self.View:SetText(self.ID.Text, "SB")
    self.View:SetImage(self.ID.Image1, "Textures/B", "item_01.png")
    self.View:SetImage(self.ID.Image2, "Textures/A", "gongchengshi.png")
    self.View:SetImage(self.ID.Image3, "Textures/A", "golden_finger.png")

    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Start, self.OnBtnStartClicked)
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Get, self.OnBtnGetClicked)
end

function ViewA:OnBtnGetClicked()
    Util.HttpGet("http://freestyle2cdn.changyou.kr/resources/FreeStyle/1.120.1/version_2002.txt", 5, function(state, text)
        print("state:" .. tostring(state) .. ", text:\n" .. text)
    end)
end

function ViewA:OnBtnStartClicked()
    print("Click Button !!!!")
    Util.LoadScene("Battle", function(state, progress)
        self:onloadedscene(state, progress)
    end)
end

function ViewA:onloadedscene(state, progress)
    print("state:" .. state .. ", progress:" .. progress)
    if state == Const.LoadingState.LoadDone then
        print("DestroyWindow !!!")
        Util.DestroyWindow(self.windowId)
    end
end

return ViewA
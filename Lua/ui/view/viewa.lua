local Const = require("ui/const")

local viewA = class("viewa")

function viewA:onCreate()
    print("-------- viewA:onCreate --------")
    self:refresh()
end

function viewA:refresh()
    print("refresh ID.Text:" .. tostring(self.ID.Text))
    self.View:SetText(self.ID.Text, "Start Game!!!!!")
    self.View:SetImage(self.ID.Image1, "Textures/B", "item_01.png")
    self.View:SetImage(self.ID.Image2, "Textures/A", "gongchengshi.png")
    self.View:SetImage(self.ID.Image3, "Textures/A", "golden_finger.png")

    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Start, self.OnBtnStartClicked)
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Get, self.OnBtnGetClicked)
end

function viewA:OnBtnGetClicked()
    View.Show(3)
end

function viewA:OnBtnStartClicked()
    print("Click Start Button !!!!")
    View.Show(2)
end

function viewA:onloadedscene(state, progress)
    print("state:" .. state .. ", progress:" .. progress)
    if state == Const.LoadingState.LoadDone then
        print("DestroyWindow !!!")
        View.HideAll()
    end
end

return viewA
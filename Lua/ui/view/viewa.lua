local Const = require("ui/const")

local viewA = class("viewa")

function viewA:OnCreate()
    print("-------- viewA:onCreate --------")
    self:BindEvent()
    self:RefreshView()
end

function viewA:BindEvent()
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Start, self.OnBtnStartClicked)
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Get, self.OnBtnGetClicked)
end

function viewA:RefreshView()
    self.View:SetText(self.ID.Text, "Start Game")
    self.View:SetImage(self.ID.Image1, "Textures/B", "item_01.png")
    self.View:SetImage(self.ID.Image2, "Textures/A", "gongchengshi.png")
    self.View:SetImage(self.ID.Image3, "Textures/A", "golden_finger.png")
end

function viewA:OnBtnGetClicked()
    View.Show(3)
end

function viewA:OnBtnStartClicked()
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
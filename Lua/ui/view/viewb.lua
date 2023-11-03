local Const = require("ui/const")
local viewbase = require("ui/view/viewbase")

local viewB = class("viewb", viewbase)

function viewB:OnCreate()
    self.super.OnCreate(self)
    print("-------- viewB:onCreate --------")
    self:BindEvent()
    self:RefreshView()
end

function viewB:RefreshView()
    self.View:SetText(self.ID.Text_Desc, "Hello World.")
end

function viewB:BindEvent()
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Player1, self.OnBtnPlayer1Clicked)
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Player2, self.OnBtnPlayer2Clicked)
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Close, self.OnBtnCloseClicked)
end

function viewB:StartBattle()
    Util.LoadScene("Battle", function(state, progress)
        self:onloadedscene(state, progress)
    end)
end

function viewB:OnBtnPlayer1Clicked()
    Util.SetSelfPlayerId(1)
    self:StartBattle()
end

function viewB:OnBtnPlayer2Clicked()
    Util.SetSelfPlayerId(2)
    self:StartBattle()
end

function viewB:OnBtnCloseClicked()
    self:Close()
end

function viewB:onloadedscene(state, progress)
    print("state:" .. state .. ", progress:" .. progress)
    if state == Const.LoadingState.LoadDone then
        print("DestroyWindow !!!")
        View.HideAll()
    end
end

return viewB
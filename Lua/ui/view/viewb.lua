local Const = require("ui/const")

local viewB = class("viewb")

function viewB:onCreate()
    print("-------- viewB:onCreate --------")
    self:onRefresh()
end

function viewB:onRefresh()
    print("-------- viewB:onRefresh --------")
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Start, self.OnBtnStartClicked)
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Close, self.OnBtnCloseClicked)
end

function viewB:OnBtnStartClicked()
    print("-------- viewB:OnBtnStartClicked --------")
    Util.LoadScene("Battle", function(state, progress)
        self:onloadedscene(state, progress)
    end)
end

function viewB:OnBtnCloseClicked()
    print("-------- viewB:OnBtnCloseClicked --------")
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
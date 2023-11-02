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
    Util.Patching("http://192.168.16.158:8080/pres", self, self.OnPatchingEvent)
end

function viewA:OnPatchingEvent(event, arg1, arg2)
    if event == "ready" then
        print("OnPatchingEvent ready !!!")
    elseif event == "setdownload" then
        print("OnPatchingEvent setdownload !!!" .. ", file count:" .. arg1 .. ", size:" .. arg2)
    elseif event == "candownload" then
        print("OnPatchingEvent candownload !!!")
        return 1
    elseif event == "download" then
        print("OnPatchingEvent download !!! arg1:" .. tostring(arg1))
    elseif event == "done" then
        print("OnPatchingEvent done !!!")
    elseif event == "error" then
        print("OnPatchingEvent error !!!")
    end
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
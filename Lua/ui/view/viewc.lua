local Const = require("ui/const")

local viewC = class("viewc")

function viewC:onCreate()
    print("-------- viewC:onCreate --------")
    self:BindEvent()
end

function viewC:BindEvent()
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Confirm, self.OnBtnConfirmClicked)
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Cancel, self.OnBtnCancelClicked)
    self.View:BindEvent(Const.EventID.ButtonClicked, self.ID.Button_Close, self.OnBtnCloseClicked)
end

function viewC:OnBtnConfirmClicked()
    Util.Patching("http://192.168.16.158:8080/pres", self, self.OnPatchingEvent)
end

function viewC:OnBtnCancelClicked()
    CS.UnityEngine.Application.Quit()
end

function viewC:OnBtnCloseClicked()
    self:Close()
end

function viewC:OnPatchingEvent(event, arg1, arg2)
    if event == "ready" then
        print("OnPatchingEvent ready !!!")
    elseif event == "setdownload" then
        print("OnPatchingEvent setdownload !!!" .. ", file count:" .. arg1 .. ", size:" .. arg2)
        self.View:SetText(self.ID.Text_Info, "File Count:" .. tostring(arg1) .. ", Size:" .. tostring(arg2))
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

return viewC
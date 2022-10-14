local test = {}

local LoadingState = {
    None = 0,
    Ready = 1,
    Loading = 2,
    Finished = 3,
    LoadDone = 4,
}

local EventID = {
    ButtonClicked = 0,
}

function test:start()
    print("-------- test:start --------")
    self.windowId = CreateWindow(-1, "A", 1, 0x10, self, function()
        self:refresh()
    end)
end

function test:refresh()
    print("windowsId:" .. self.windowId .. ", ID.Text:" .. tostring(self.ID.Text))
    self.View:SetText(self.ID.Text, "SB")
    self.View:SetImage(self.ID.Image1, "Textures/B", "item_01.png")
    self.View:SetImage(self.ID.Image2, "Textures/A", "gongchengshi.png")
    self.View:SetImage(self.ID.Image3, "Textures/A", "golden_finger.png")

    self.View:BindEvent(EventID.ButtonClicked, self.ID.Button, self.onbtnclick)
end

function test:onbtnclick()
    print("Click Button !!!!")
    LoadScene("Battle", function(state, progress)
        self:onloadedscene(state, progress)
    end)
end

function test:onloadedscene(state, progress)
    print("state:" .. state .. ", progress:" .. progress)
    if state == LoadingState.LoadDone then
        print("DestroyWindow !!!")
        DestroyWindow(self.windowId)
    end
end

return test
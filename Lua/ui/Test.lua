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
    local windowId
    windowId = CreateWindow(-1, "A", 1, 0x10, self, function(id)
        print("id:" .. id .. "windowsId:" .. windowId .. ", ID.Text:" .. tostring(self.ID.Text))
        self.View:SetText(self.ID.Text, "SB")
        self.View:SetImage(self.ID.Image1, "Textures/B", "item_01.png")
        self.View:SetImage(self.ID.Image2, "Textures/A", "gongchengshi.png")
        self.View:SetImage(self.ID.Image3, "Textures/A", "golden_finger.png")

        self.View:BindEvent(EventID.ButtonClicked, self.ID.Button, function()
            print("Click Button !!!!")
            LoadScene("Battle", function(state, progress)
                print("state:" .. state .. ", progress:" .. progress)
                if state == LoadingState.LoadDone then
                    print("DestroyWindow !!!")
                    DestroyWindow(windowId)
                end
            end)
        end)
    end)
    
end

return test
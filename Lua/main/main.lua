print("Hello World!")

local CreateWindow = CS.LuaUtil.CreateWindow

local view = {}

function view:start()
    CreateWindow(-1, "A", 1, 0x10, self, function(id)
        print("id:" .. id .. ", TextID:" .. tostring(self.ID.Text))
        print("txt:" .. self.View:GetText(self.ID.Text))
        self.View:SetImage(self.ID.Image1, "Textures/B/item_01.png")
        self.View:SetImage(self.ID.Image2, "Textures/B/item_02.png")
        self.View:SetImage(self.ID.Image3, "Textures/B/item_03.png")
    end)
end

view:start()
local test = {}

function test:start()
    print("-------- start --------")
    LoadScene("Main", function(state, progress)
        print("state:" .. state .. ", progress:" .. progress)
        if state == 4 then
            CreateWindow(-1, "A", 1, 0x10, self, function(id)
                print("id:" .. id .. ", TextID:" .. tostring(self.ID.Text))
                self.View:SetText(self.ID.Text, "SB")
                self.View:SetImage(self.ID.Image1, "Textures/B", "item_01.png")
                self.View:SetImage(self.ID.Image2, "Textures/A", "gongchengshi.png")
                self.View:SetImage(self.ID.Image3, "Textures/A", "golden_finger.png")
            end)
        end
    end)
end

return test
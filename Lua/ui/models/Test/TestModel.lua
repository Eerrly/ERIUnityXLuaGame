local Model = require("ui/models/Model")
local TestModel = class(Model, "TestModel")

function TestModel:ctor()
    TestModel.super.ctor(self)
end

--@data: 服务器传输过来的数据[TODO:模拟数据]
function TestModel:InitWithProtocol(data)
    self.data = {
        name = "Eerrly",
        sex = "Man",
        age = "24",
        itemList = {
            { name = "a" },
            { name = "b" },
            { name = "c" },
            { name = "d" },
        }
    }
end

function TestModel:GetName()
    return self.data.name
end

function TestModel:GetSex()
    return self.data.sex
end

function TestModel:GetAge()
    return self.data.age
end

function TestModel:GetItemList()
    return self.data.itemList
end

return TestModel
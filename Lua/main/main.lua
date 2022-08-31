local TestCtrl = require("ui/controllers/Test/TestCtrl")

local main = {}

function main:init()
    ndump("[== Main Lua ==] [NetWork State] " .. res.GetCurrInternetName())
    TestCtrl.new()
end

main:init()
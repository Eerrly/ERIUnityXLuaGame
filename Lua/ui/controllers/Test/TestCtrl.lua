local UnityEngine = CS.UnityEngine
local Object = UnityEngine.Object
local GameObject = UnityEngine.GameObject
local Canvas = UnityEngine.Canvas
local TestModel = require("ui/models/Test/TestModel")
local TestCtrl = class()

function TestCtrl:ctor()
    self.canvas = GameObject.FindObjectOfType(typeof(Canvas))
    self.gameObject, self.view = Object.Instantiate(GameObject())
    self.transform = self.gameObject.transform
    self.transform:SetParent(self.canvas.transform, false)
    self.model = TestModel.new()
    self.model:InitWithProtocol()
    self:Init()
end

function TestCtrl:Init()
    ndump("TestCtrl:Init", self)
    self.view:InitView(self.model)
end

return TestCtrl
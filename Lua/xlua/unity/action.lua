local action = class()

function action:ctor()
    self.handlerList = {}
end

function action:AddHandler(func)
    table.insert(self.handlerList, func)
end

function action:Trigger(...)
    for _, func in pairs(self.handlerList) do
        func(...)
    end
end

function action:RemoveAllHandler()
    self.handlerList = {}
end

exports.action = action
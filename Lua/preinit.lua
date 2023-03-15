local __g = _G

-- export global variable
---@type _G
exports = {
}

setmetatable(exports, {
    __newindex = function(_, name, value)
        rawset(__g, name, value)
    end,

    __index = function(_, name)
        return rawget(__g, name)
    end
})

-- disable create unexpected global variable
setmetatable(__g, {
    __newindex = function(_, name, value)
        local msg = "USE 'exports.%s = value' INSTEAD OF SET GLOBAL VARIABLE"
        error(string.format(msg, name), 0)
    end
})

require("init")
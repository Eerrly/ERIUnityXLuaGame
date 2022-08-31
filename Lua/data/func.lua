local function clone(object)
    local lookup_table = {}
    local function _copy(_object)
        if type(_object) ~= "table" then
            return _object
        elseif lookup_table[_object] then
            return lookup_table[_object]
        end
        local new_table = {}
        lookup_table[_object] = new_table
        for key, value in pairs(_object) do
            new_table[_copy(key)] = _copy(value)
        end
        return setmetatable(new_table, getmetatable(_object))
    end
    return _copy(object)
end

local function class(super, name)
    local cls = {}
    if super then
        cls = clone(super)
        cls.super = super
        if super.__index == super then
            cls.__index = nil
        end
    else
        cls = { ctor = function()end }
    end
    cls.__cname = name
    if not cls.__index then
        cls.__index = cls
    end
    function cls.new(...)
        local instance = setmetatable({}, cls)
        instance.class = cls
        instance:ctor(...)
        return instance
    end
    return cls
end

local function getmt(base)
    return function(table, key)
        local cs = rawget(table, "___cs")
        if cs then
            local v = cs[key]
            if v and type(v) == "function" then
                return function(obj, ...)
                    return v(cs, ...)
                end
            end
            return v
        end
        return base[key]
    end
end

exports.getmt = getmt
exports.clone = clone
exports.class = class
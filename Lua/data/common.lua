--- 拷贝
---@param object 对象
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
        for k, v in pairs(_object) do
            new_table[_copy(k)] = _copy(v)
        end
        return setmetatable(new_table, getmetatable(_object))
    end
    return _copy(object)
end

--- 创建对象
---@param name 类名
---@param super 父类
local function class(name, super)
    local super_type = type(super)
    local cls
    if super_type ~= "function" and super_type ~= "table" then
        super_type = nil
        super = nil
    end
    if super_type == "function" or (super and super.__ctype == 1) then
        cls = {}
        if super_type == "table" then
            for k, v in pairs(super) do
                cls[k] = v
            end
            cls.__create = super.__create
            cls.super = super
        else
            cls.super = super
        end
        cls.ctor = function() end
        cls.__cname = name
        cls.__ctype =1
        function cls.new(...)
            local instance = cls.__create(...)
            for k, v in pairs(cls) do
                instance[k] = v
            end
            instance.class = cls
            instance:ctor(...)
            return instance
        end
    else
        if super then
            cls = clone(super)
            cls.super = super
        else
            cls = { ctor = function() end }
        end
        cls.__cname = name
        cls.__ctype = 2
        cls.__index = cls
        function cls.new(...)
            local instance = setmetatable({}, cls)
            instance.class = cls
            instance:ctor(...)
            return instance
        end
    end
    return cls
end

exports.class = class
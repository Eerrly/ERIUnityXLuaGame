local move_end = {}

local generator_mt = {
    __index = {
        MoveNext = function(self)
            self.Current = self.co()
            if self.Current == move_end then
                self.Current = nil
                return false
            else
                return true
            end
        end;
        Reset = function(self)
            self.co = coroutine.wrap(self.w_func)
        end
    }
}

local function cs_generator(func, ...)
    local params = {...}
    local generator = setmetatable({
        w_func = function()
            func(unpack(params))
            return move_end
        end
    }, generator_mt)
    generator:Reset()
    return generator
end

local function loadpackage(...)
    for _, loader in ipairs(package.searchers) do
        local func = loader(...)
        if type(func) == 'function' then
            return func
        end
    end
end

local function auto_id_map()
    local hotfix_id_map = require 'hotfix_id_map'
    local org_hotfix = xlua.hotfix
    xlua.hotfix = function(cs, field, func)
        local map_info_of_type = hotfix_id_map[typeof(cs):ToString()]
        if map_info_of_type then
            if func == nil then func = false end
            local tbl = (type(field) == 'table') and field or {[field] = func}
            for k, v in pairs(tbl) do
                local map_info_of_methods = map_info_of_type[k]
                local f = type(v) == 'function' and v or nil
                for _, id in ipairs(map_info_of_methods or {}) do
                    CS.XLua.HotfixDelegateBridge.Set(id, f)
                end
                --CS.XLua.HotfixDelegateBridge.Set(
            end
        else
            return org_hotfix(cs, field, func)
        end
    end
end

--和xlua.hotfix的区别是：这个可以调用原来的函数
local function hotfix_ex(cs, field, func)
    assert(type(field) == 'string' and type(func) == 'function', 'invalid argument: #2 string needed, #3 function needed!')
    local function func_after(...)
        xlua.hotfix(cs, field, nil)
        local ret = {func(...)}
        xlua.hotfix(cs, field, func_after)
        return unpack(ret)
    end
    xlua.hotfix(cs, field, func_after)
end

local function bind(func, obj)
    return function(...)
        return func(obj, ...)
    end
end

--为了兼容luajit，lua53版本直接用|操作符即可
local enum_or_op = debug.getmetatable(CS.System.Reflection.BindingFlags.Public).__bor
local enum_or_op_ex = function(first, ...)
    for _, e in ipairs({...}) do
        first = enum_or_op(first, e)
    end
    return first
end

-- description: 直接用C#函数创建delegate
local function createdelegate(delegate_cls, obj, impl_cls, method_name, parameter_type_list)
    local flag = enum_or_op_ex(CS.System.Reflection.BindingFlags.Public, CS.System.Reflection.BindingFlags.NonPublic, 
        CS.System.Reflection.BindingFlags.Instance, CS.System.Reflection.BindingFlags.Static)
    local m = parameter_type_list and typeof(impl_cls):GetMethod(method_name, flag, nil, parameter_type_list, nil)
             or typeof(impl_cls):GetMethod(method_name, flag)
    return CS.System.Delegate.CreateDelegate(typeof(delegate_cls), obj, m)
end

local function state(csobj, state)
    local csobj_mt = getmetatable(csobj)
    for k, v in pairs(csobj_mt) do rawset(state, k, v) end
    local csobj_index, csobj_newindex = state.__index, state.__newindex
    state.__index = function(obj, k)
        return rawget(state, k) or csobj_index(obj, k)
    end
    state.__newindex = function(obj, k, v)
        if rawget(state, k) ~= nil then
            rawset(state, k, v)
        else
            csobj_newindex(obj, k, v)
        end
    end
    debug.setmetatable(csobj, state)
    return state
end

local function print_func_ref_by_csharp()
    local registry = debug.getregistry()
    for k, v in pairs(registry) do
        if type(k) == 'number' and type(v) == 'function' and registry[v] == k then
            local info = debug.getinfo(v)
            print(string.format('%s:%d', info.short_src, info.linedefined))
        end
    end
end

exports.cs_generator = cs_generator
exports.loadpackage = loadpackage
exports.auto_id_map = auto_id_map
exports.hotfix_ex = hotfix_ex
exports.bind = bind
exports.createdelegate = createdelegate
exports.state = state
exports.print_func_ref_by_csharp = print_func_ref_by_csharp


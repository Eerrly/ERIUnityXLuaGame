local function vardump(object, label)
    local lookupTable = {}
    local indexed = {}
    local result = {}

    local function _v(v)
        if type(v) == "string" then
            local str = string.gsub(v, "\\", "\\\\")
            str = string.gsub(str, "\"", "\\\"")
            return "\""..str.."\""
        end
        if type(v) == 'number' or type(v) == 'boolean' then
            return tostring(v)
        end
        if type(v) == 'userdata' then
            if v == CS.null then
                return "'"..tostring(v)..", null'"
            else
                return "'"..tostring(v)..", "..CS.System.String.Format("{0}", v).."'"
            end
        end
        return "'"..tostring(v).."'"
    end

    local _vardump -- local function _vardump
    local function _k(k, indent, nest)
        if type(k) == "string" then
            if k ~= "" then
                local firstChar = string.byte (k, 1)
                if firstChar >= 65 and firstChar <= 90 or firstChar == 95 or firstChar >= 97 and firstChar <= 122 then
                    --a-zA-Z_
                    return k
                end
            end
            return string.format('["%s"]', k)
        end
        if type(k) == 'number' or type(k) == 'boolean' then
            return string.format("[%s]", tostring(k))
        end
        if type(k) == 'table' then
            if lookupTable[k] then
                local line = lookupTable[k]
                local rv = string.format("['*%s*']", tostring(line))
                if not indexed[line] then
                    result[line] = result[line]..string.format(" --*%s*", line)
                    indexed[line] = line
                end
                return rv
            else
                local line = #result + 1
                local rv = string.format("['*%s*']", tostring(line))
                local kkey = string.format("['*%s*:']", tostring(line))
                _vardump(k, kkey, indent, nest)
                if nest == 1 then
                    result[#result] = result[#result]..','
                end
                --lookupTable[k] = line
                indexed[line] = line
                return rv
            end
        end
        return string.format("['%s']", tostring(k))
    end

    function _vardump(object, label, indent, nest, isArr)
        if (label == nil) then label = "var" end
        local postfix = ""
        if nest > 1 then postfix = "," end
        local reallabel = indent
        if not isArr then
            local key = _k(label, indent, nest)
            reallabel = string.format("%s%s = ", indent, key)
        end
        if type(object) ~= "table" then
            result[#result +1] = string.format("%s%s%s", reallabel, _v(object), postfix)
        elseif lookupTable[object] then
            local line = lookupTable[object]
            result[#result +1] = string.format("%s'*%s*'%s", reallabel, line, postfix)
            if not indexed[line] then
                result[line] = result[line]..string.format(" --*%s*", line)
                indexed[line] = line
            end
        else
            local line = #result + 1
            lookupTable[object] = line
            result[line] = string.format("%s{", reallabel)

            local indent2 = indent .. "    "
            local keys = {}
            local allKeyIsInt = true
            local minKey, maxKey = #object, 0
            for k, v in pairs(object) do
                keys[#keys + 1] = k
                if type(k) == 'number' and k == math.floor(k) then
                    if k > maxKey then
                        maxKey = k
                    end
                    if k < minKey then
                        minKey = k
                    end
                else
                    allKeyIsInt = false
                end
            end
            local isObjArr = allKeyIsInt and #keys == maxKey and minKey == 1
            table.sort(keys, function(a, b)
                if type(a) == "number" and type(b) == "number" then
                    return a < b
                else
                    return tostring(a) < tostring(b)
                end
            end)
            for i, k in ipairs(keys) do
                _vardump(object[k], k, indent2, nest + 1, isObjArr)
            end
            result[#result +1] = string.format("%s}%s", indent, postfix)
        end
    end
    _vardump(object, label, "", 1)

    return result
end
------------dumpq--------------
local function dumpq(object, label)
    local result = vardump(object, label)
    local str = table.concat(result, "\n")

    local traceback = debug.traceback("", 2)
    return str..'\n'..(traceback or "dump from out of lua-state")
end
------------ndumpq------------
local function ndumpq(...)
    local str
    for i = 1, select('#', ...) do
        local arg = select(i, ...)
        local result = vardump(arg, i)
        if str then
            str = str..'\n'
        else
            str = ""
        end
        str = str..table.concat(result, "\n")..','
    end

    local traceback = debug.traceback("", 2)
    return str..'\n'..(traceback or "multi dump from out of lua-state")
end

--日志echo--
local function echo(...)
    local arr = {}
    for i = 1, select('#', ...) do
        local arg = select(i, ...)
        arr[#arr + 1] = tostring(arg)
    end
    print(table.concat(arr, "\t"))
end

local function dump(object, label)
    print(dumpq(object, label))
end

local function ndump(...)
    print(ndumpq(...))
end

exports.dump = dump
exports.ndump = ndump
exports.echo = echo
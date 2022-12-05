---查找一个元素
---@param t table<K,V>
---@param predict fun(item:V):boolean
---@return value|key
function table.find(t, predict)
    for k, v in pairs(t) do
        if predict(v) then
            return v, k
        end
    end
end

---查找一个元素的索引
---@param t table<K,V>
---@param item V
---@return K
function table.findIndex(t, item)
    for k, v in pairs(t) do
        if v == item then
            return k
        end
    end
end

---是否包含元素
---@param t table<K,V>
---@param item V
---@return boolean
function table.contains(t, item)
    if not t then return false end
    return table.findIndex(t, item) and true or false
end

---浅拷贝
---@param t table<k,V>
---@return table<K,V>
function table.refCopy(t)
    if not t then return nil end
    local copy = {}
    for i,v in pairs(t) do
        copy[i] = v
    end
    return copy
end

---深拷贝
---@param t table<k,V>
---@return table<K,V>
function table.deepCopy(t)
    return Util.Clone(t)
end

---将array'from' insert到to
function table.addArray(to, from)
    if not to or not from then return end
    for _, v in ipairs(from) do
        table.insert(to, v)
    end
    return to
end

---将array'from' insert到to
function table.addDic(to, from)
    if not to or not from then return end
    for _, v in pairs(from) do
        to[_] = v
    end
    return to
end

--tbl需要删除对象的表， func用于判断是否是需要删除的对象的函数，
--func返回两个参数，1、是否remove 2、是否继续
--该函数会copy一次table
function table.removeEx(tbl, func)
    if tbl == nil or func == nil then
        return tbl
    end
    local newTbl = {}
    for k, v in pairs(tbl) do
        local needRemove, continue = func(k, v)

        if not needRemove and v then
            newTbl[#newTbl + 1] = v
        end

        if not continue then
            break
        end
    end
    return newTbl
end

-- 反转阵列
function table.revert(tableAy)
    local length = #tableAy
    local half = length / 2
    local temp
    for i = 1, half do
        temp = tableAy[i]
        tableAy[i] = tableAy[length - i + 1]
        tableAy[length - i + 1] = temp
    end
    return tableAy
end

--- 筛选
---@param src table
---@param func fun(v:any):boolean
---@param isKeyValuePairs boolean 返回键值对还是数组
---@return table
function table.where(src, func, isKeyValuePairs)
    local result = {}
    for k, v in pairs(src) do
        if func(v) then
            if isKeyValuePairs then
                result[k] = v
            else
                result[#result + 1] = v
            end
        end
    end
    return result
end

--- 选择过滤
---@param src table
---@param func fun(v:any):boolean
---@param isKeyValuePairs boolean 返回键值对还是数组
---@return table
function table.select(src, func, isKeyValuePairs)
    local result = {}
    for k, v in pairs(src) do
        if isKeyValuePairs then
            result[k] = func(v)
        else
            result[#result + 1] = func(v)
        end
    end
    return result
end

--- 获取表的长度
---@param src table
---@return number
function table.len(src)
    local count = 0
    for _, _ in pairs(src) do
        count = count + 1
    end
    return count
end

--满足任意条件
function table.any(tbl, func)
    for _, v in pairs(tbl) do
        if func(v) then
            return true
        end
    end
    return false
end

function table.foreach(tbl, func)
    for _, v in pairs(tbl) do
       func(v)
    end
end

function table.iforeach(tbl, func)
    for k, v in ipairs(tbl) do
        func(v, k)
    end
end

function table.count(tbl, func)
    local result = 0
    for _, v in pairs(tbl) do
        result = result + func(v)
    end
    return result
end

--打印
function table.tostring(tab)
    local typeTab = type({})
    local typeStr = type("")
    local str = "{"
    for k, v in pairs(tab) do
        local kStr = nil
        local vStr = nil

        if type(k) == typeTab then
            kStr = table.tostring(k)
        elseif type(k) == typeStr then
            kStr = string.format("\"%s\"", k)
        else
            kStr = tostring(k)
        end

        if type(v) == typeTab then
            vStr = table.tostring(v)
        elseif type(v) ==  typeStr then
            vStr = string.format("\"%s\"", v)
        else
            vStr = tostring(v)
        end

        str = string.format("%s %s:%s,", str, kStr, vStr)
    end

    str = str .. "}"

    return str
end

function table.print(tab)
    print(table.tostring(tab))
end

function table.error(tab)
    error(table.tostring(tab))
end

function table.nums(tbl)
    local count = 0
    for _, v in pairs(tbl) do
        count = count + 1
    end
    return count
end

function table.toArray(tab, dataId)
    local result = {}
    if dataId then
        for i, v in pairs(tab) do
            result[#result + 1] = v[dataId]
        end
    else
        for i, v in pairs(tab) do
            result[#result + 1] = v
        end
    end
    return result
end

--
function table.makeValueDic(to, from)
    if not to or not from then return end
    for _, v in pairs(from) do
        to[v] = true
    end
    return to
end

--
function table.makeValueDic2(to, from, key)
    if not to or not from then return end
    for _, v in pairs(from) do
        to[v[key]] = true
    end
    return to
end
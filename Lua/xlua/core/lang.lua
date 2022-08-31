local lang = {}

local languagepack = {
    internet_not_reachable = "无网络",
    internet_wifi = "WiFi或有线网络",
    internet_carrier_data = "移动数据网络",
}

function lang.trans(key)
    if type(key) ~= "string" then error("lang.trans key have to be string type!") end
    return languagepack[key]
end

function lang.transstr(key, ...)
    if type(key) ~= "string" then error("lang.trans key have to be string type!") end
    return languagepack[key]
end

exports.lang = lang
local multilingual = {
	["现金"] = {
		["zh"] = "现金",
		["en"] = "",
		["jp"] = "",	},
	["通用货币1"] = {
		["zh"] = "通用货币1",
		["en"] = "",
		["jp"] = "",	},
	["钻石"] = {
		["zh"] = "钻石",
		["en"] = "",
		["jp"] = "",	},
	["通用货币2"] = {
		["zh"] = "通用货币2",
		["en"] = "",
		["jp"] = "",	},
	["演示1"] = {
		["zh"] = "演示1",
		["en"] = "",
		["jp"] = "",	},
	["这是一个演示1的表述"] = {
		["zh"] = "这是一个演示1的表述",
		["en"] = "",
		["jp"] = "",	},
	["演示2"] = {
		["zh"] = "演示2",
		["en"] = "",
		["jp"] = "",	},
	["这是一个演示2的表述"] = {
		["zh"] = "这是一个演示2的表述",
		["en"] = "",
		["jp"] = "",	},
	["演示3"] = {
		["zh"] = "演示3",
		["en"] = "",
		["jp"] = "",	},
	["这是一个演示3的表述"] = {
		["zh"] = "这是一个演示3的表述",
		["en"] = "",
		["jp"] = "",	},
	["演示4"] = {
		["zh"] = "演示4",
		["en"] = "",
		["jp"] = "",	},
	["这是一个演示4的表述"] = {
		["zh"] = "这是一个演示4的表述",
		["en"] = "",
		["jp"] = "",	},}
---@class Cfg_I18n
---@field language string
local i18n = {
	Items_name_1 = multilingual["现金"],
	Items_desc_1 = multilingual["通用货币1"],
	Items_name_2 = multilingual["钻石"],
	Items_desc_2 = multilingual["通用货币2"],
	TempConfig_name_1 = multilingual["演示1"],
	TempConfig_desc_1 = multilingual["这是一个演示1的表述"],
	TempConfig_name_2 = multilingual["演示2"],
	TempConfig_desc_2 = multilingual["这是一个演示2的表述"],
	TempConfig_name_3 = multilingual["演示3"],
	TempConfig_desc_3 = multilingual["这是一个演示3的表述"],
	TempConfig_name_4 = multilingual["演示4"],
	TempConfig_desc_4 = multilingual["这是一个演示4的表述"],}

---@type Cfg_I18n
local cfg_i18n = { language = "en" }
setmetatable(cfg_i18n, {
	__index = function (t, key)
		if i18n[key] then return i18n[key][t.language] end
		print(i18n[key],("index == nil 多语言中不存在键：%s\n%s"):format(tostring(key), debug.traceback()))
		return tostring(key)
	end
})

return cfg_i18n

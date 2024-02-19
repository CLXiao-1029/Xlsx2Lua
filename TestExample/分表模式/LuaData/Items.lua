---@type Cfg_Items[]
local Items = {
	[1] = {
		id = 1,
		name = "Items_name_1",
		desc = "Items_desc_1",
		type = 1,
		subtype = 1,
		rare = 3,
		icon = "item_1",
		randomvalue = {
			1000,
			10000,		},
	},	
	[2] = {
		id = 2,
		name = "Items_name_2",
		desc = "Items_desc_2",
		type = 1,
		subtype = 2,
		rare = 4,
		icon = "item_2",
		randomvalue = {
			10,
			100,		},
	},	
}

return Items

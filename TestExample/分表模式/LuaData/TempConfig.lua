---@type Cfg_TempConfig[]
local TempConfig = {
	[1] = {
		id = 1,
		name = "TempConfig_name_1",
		desc = "TempConfig_desc_1",
		info = "这个文本不用翻译",
		Int = 1,
		Long = 123,
		Float = 1.5,
		String = "你好",
		Bool = false,
		Table = {tid=1,count=100},
		IntArr1 = {
			1,
			2,
			3,		},
		IntArr2 = {
			4,
			5,
			6,		},
		ArrayLong1 = {
			2,
			3,
			4,		},
		ArrayLong2 = {
			3,
			5,
			6,		},
		FloatArr1 = {
			1.1,
			2.2,
			3.3,		},
		FloatArr2 = {
			4.4,
			5.5,
			6.6,		},
		StringArr1 = {
			"你",
			"好",
			"世",
			"界",		},
		StringArr2 = {
			"晚",
			"安",
			"好",
			"梦",		},
		BoolArr1 = {
			true,
			false,
			true,		},
		BoolArr2 = {
			false,
			true,
			false,		},
		hashTable1 ={
			{
				tid = 1,
				value = 100,			},
			{
				tid = 2,
				value = 1000,			},
		},
		hashTable2 ={
			{
				tid = 3,
				value = 30,			},
		},
		anyValue = "你好世界",
	},	
	[2] = {
		id = 2,
		name = "TempConfig_name_2",
		desc = "TempConfig_desc_2",
		info = "这个文本不用翻译",
		Int = 10,
		Long = 1234,
		Float = 2.5,
		String = "世界",
		Bool = true,
		Table = {type=2,name="类型2"},
		IntArr1 = {
			1,
			2,
			3,		},
		IntArr2 = {
			4,
			5,
			6,		},
		ArrayLong1 = {
			2,
			3,
			4,		},
		ArrayLong2 = {
			3,
			5,
			6,		},
		FloatArr1 = {
			1.1,
			2.2,
			3.3,		},
		FloatArr2 = {
			4.4,
			5.5,
			6.6,		},
		StringArr1 = {
			"你",
			"好",
			"世",
			"界",		},
		StringArr2 = {
			"晚",
			"安",
			"好",
			"梦",		},
		BoolArr1 = {
			true,
			false,
			true,		},
		hashTable2 ={
			{
				tid = 5,
				value = 10,			},
			{
				tid = 6,
				value = 2,			},
		},
		anyValue = 123,
	},	
	[3] = {
		id = 3,
		name = "TempConfig_name_3",
		desc = "TempConfig_desc_3",
		info = "这个文本不用翻译",
		Int = 100,
		Long = 12345,
		Float = 3.5,
		String = "晚安",
		Bool = false,
		Table = {tid=1,count=100,isTrue=false},
		IntArr1 = {
			1,
			2,
			3,		},
		IntArr2 = {
			4,
			5,
			6,		},
		ArrayLong1 = {
			2,
			3,
			4,		},
		ArrayLong2 = {
			3,
			5,
			6,		},
		FloatArr1 = {
			1.1,
			2.2,
			3.3,		},
		FloatArr2 = {
			4.4,
			5.5,
			6.6,		},
		StringArr1 = {
			"你",
			"好",
			"世",
			"界",		},
		StringArr2 = {
			"晚",
			"安",
			"好",
			"梦",		},
		BoolArr2 = {
			true,		},
		hashTable1 ={
			{
				tid = 3,
				value = 100,			},
			{
				tid = 101,
				value = 1,			},
		},
		anyValue = True,
	},	
	[4] = {
		id = 4,
		name = "TempConfig_name_4",
		desc = "TempConfig_desc_4",
		info = "这个文本不用翻译",
		Int = 1000,
		Long = 123456,
		Float = 4.5,
		String = "好梦",
		Bool = true,
		Table = {{tid=1,count=100},{tid=2,count=1000}},
		IntArr1 = {
			1,
			2,
			3,		},
		IntArr2 = {
			4,
			5,
			6,		},
		ArrayLong1 = {
			2,
			3,
			4,		},
		ArrayLong2 = {
			3,
			5,
			6,		},
		FloatArr1 = {
			1.1,
			2.2,
			3.3,		},
		FloatArr2 = {
			4.4,
			5.5,
			6.6,		},
		StringArr1 = {
			"你",
			"好",
			"世",
			"界",		},
		StringArr2 = {
			"晚",
			"安",
			"好",
			"梦",		},
		BoolArr1 = {
			false,		},
		anyValue = {id=5,type=2,count=10,isTrue=false},
	},	
}

return TempConfig

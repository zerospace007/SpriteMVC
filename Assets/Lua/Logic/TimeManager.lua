--region SoundManager.lua
--Date 2019/05/29
--时间管理
--endregion
local Time = Time
local UpdateBeat = UpdateBeat
local setmetatable = setmetatable
local insert = table.insert
local remove = table.remove

local TimeHandler = {};
local ob = TimeHandler;
ob.__index = ob;

--创建对象
function TimeHandler.New(name, update, complete)
	return setmetatable({name = name, update = update, complete = complete, isDelete = false}, ob);
end

--重设
function TimeHandler:Reset(name, update, complete)
	self.name = name;
	self.update = update;
	self.complete = complete;
	self.isDelete = false;
end

TimeManager = {};								--时间管理
local this = TimeManager;
this.__index = this;
this.timestamp = os.time();						--系统时间戳（服务器返回，单位秒s）
local handlers = {};							--时间管理列表
local pool = {};								--时间管理对象池

local HOUR_OF_DAY = 24;
local SECOND_OF_MINUTE = 60;
local SECOND_OF_HOUR = 3600;
local SECOND_OF_DAY = HOUR_OF_DAY * SECOND_OF_HOUR;

--开始运行
function TimeManager.Start()
	this.running = true;
	if not this.Handler then
		this.Handler = UpdateBeat:CreateListener(this.Update);			--创建监听
	end
	UpdateBeat:AddListener(this.Handler);
end

--停止运行
function TimeManager.Stop()
	this.running = false;
	if this.Handler then UpdateBeat:RemoveListener(this.Handler); end
end

--每帧执行
function TimeManager.Update()
	if not this.running then return end
	this.timestamp = this.timestamp + Time.deltaTime;
	-------------------标记清除--------------------------
	for name, timer in pairs(handlers) do
		if timer.isDelete then
			this.Remove(name);
		end
	end
	-------------------每帧执行--------------------------
	for name, timer in pairs(handlers) do
		if not timer.isDelete then
			local isComplete = timer.update();
			if isComplete then 
				timer.isDelete = true;
				spawn(timer.complete);
			end
		end
	end
end

--注册时间监听
function TimeManager.Register(name, update, complete)
	local timer = handlers[name];
	if not timer then
		local timerHandler = nil;
		if #pool > 0 then
			timerHandler = remove(pool);
			timerHandler:Reset(name, update, complete);
		else
			timerHandler = TimeHandler.New(name, update, complete);
		end
		handlers[name] = timerHandler;
	end
end

--移除时间监听
function TimeManager.Remove(name)
	local timer = handlers[name];
	if timer then
		insert(pool, timer);
		handlers[name] = nil;
	end
end

--时间规范显示
function TimeManager.TimeNorm(time, style)
	time = time > 0 and time or 0;
	style = style and style or StyleType.HOUR_MIN_SEC;
	local sec = time % SECOND_OF_MINUTE;
	local min = math.floor(time / SECOND_OF_MINUTE) % SECOND_OF_MINUTE;
	local hour = math.floor(time / SECOND_OF_HOUR);
	if style == StyleType.HOUR_MIN_SEC then
		return string.format("%02d:%02d:%02d", hour, min, sec);
	elseif style == StyleType.HOUR_MIN then
		return string.format("%02d:%02d", min, sec);
	end
end

--分钟换算花费（每分钟1黄金）
function GetCost(ticks)
	return ticks > 0 and (math.floor(ticks/SECOND_OF_MINUTE) + 1) or 0;
end

StyleType =
{
	HOUR_MIN_SEC = 1,			--00:00:00 时分秒
	HOUR_MIN = 2,				--00:00 时分
}
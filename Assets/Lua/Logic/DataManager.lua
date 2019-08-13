--region DataManager.lua
--Date 2019/04/15
--配置文件管理
--endregion
json = require "cjson"										--cjson工具
local util = require "cjson/util"

require 'Config/ConfigAll'									--配置表（可优化）
TempPath = Application.temporaryCachePath .. '/';			--临时缓存目录

local UserDataFile = "UserData.bytes"						--用户信息（登录、角色信息等）
local CacheDataFile = "CacheData.bytes"						--游戏数据

--配置文件管理--
DataManager = {};
local this = DataManager;
----怪物和掩夹数据

function DataManager.Init()
	logWarn(TempPath);
	log("DataManager.Init----->>>");
	ConfigAll.LoadAll();
	
	--初始化赋值--
    this.CacheData = this.LoadFile(CacheDataFile, true) or {};
	this.SaveFile(CacheDataFile, this.CacheData);

	this.UserData = this.LoadFile(UserDataFile, true) or {};
end

--加载文件
function DataManager.LoadFile(filename, isJson)
    local path = TempPath .. filename 
    local file = this.IsFileExists(path) and util.file_load(path) 
    if not file then return end
    return isJson and json.decode(file) or file
end

--保存文件
function DataManager.SaveFile(filename, data) Print("SaveFile", filename, data, type(data))
    if not filename or not data or type(filename) ~= "string" then 
        logError("SaveFile err:"..filename)
        return 
    end    
    local path = TempPath .. filename --写入地址
	log(path)
    if not this.IsFileExists(path) then
        local name = string.gsub(path, ".*/", "") 
        local dir = string.sub(path, 1, -string.len(name)-1) 
        if "windows" then --是否为windows  需要写入权限
            local dir = string.gsub(dir, "/", "\\") 
			log(dir)
            os.execute("mkdir " .. dir)
        else
            --在Linux下是OK，Android无法创建目录，iOS尚不知
            os.execute("mkdir -p " .. dir)
        end
    end
    util.file_save(path, type(data) == "table" and json.encode(data) or data)
end

--当前是否存在某文件
function DataManager.IsFileExists(filename)
    local file, err = io.open(filename, "rb")
    if file then file:close() end
    return file, err
end

--保存用户数据
function DataManager.SaveUserData()
	this.SaveFile(UserDataFile, this.UserData);
end

--保存游戏缓存数据
function DataManager.SaveCacheData()
	this.SaveFile(CacheDataFile, this.CacheData);
end
return this;
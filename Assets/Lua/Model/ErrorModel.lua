--region ErrorModel.lua
--服务器错误码逻辑
--endregion

--客户端tips
function ShowTips(content, ...)
	TipsManager:ShowTips(string.format(content, ...));
end

--配置tips
function ShowCfgTips(content, ...)
	TipsManager:ShowTips(LangManager.GetText(content, ...));
end

ErrorModel = BaseModel:New();
local this = ErrorModel;

--{ INT id: 0, INT code: ErrorCode }
function ErrorModel.S2CErrorCode(data)
	local code = data.code;--错误码
	logWarn('错误码'.. code);
	if code == 130 then
	    TipsManager:ShowTips( tostring( data.datas[1] ) );
	else
	    TipsManager:ShowTips(LangManager.GetText('TxtErrorCode' .. code));
	end
end

--网络协议，对应函数
this.MsgIdList = 
{
	{msgid = Protocal.ErrorCode, func = this.S2CErrorCode},
}

return this
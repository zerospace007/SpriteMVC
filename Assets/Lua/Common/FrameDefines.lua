--region ViewName.lua
--Date 2019/3/15
--Author NormanYang
--endregion
require 'Model/BaseModel'													--基础业务类
require 'View/AWrap/List/BaseDataList'										--基础数据列表封装
require 'View/AWrap/List/BaseListItem'										--基础显示列表项封装
require 'View/AWrap/List/BaseViewList'										--基础显示列表封装
require 'View/AWrap/UI/BaseLoad'											--基础加载封装
require 'View/AWrap/UI/BaseImage'											--基础Image加载封装
require 'View/AWrap/UI/BaseView'											--基础View显示类
require 'View/AWrap/UI/PopView'												--基础Pop弹出界面
require 'View/AWrap/UI/TipsView'											--基础Tips弹出框

--View名称列表--
ViewList =
{   
	CameraHandler = require 'View/Camera/CameraHandler',					--地图摄像机控制
	LoginView = require 'View/Login/LoginView',								--登录界面
	ChooseRoleView = require 'View/ChooseRole/ChooseRoleView',				--选择角色界面
	ChooseSvrView = require 'View/ChooseSvr/ChooseSvrView',					--选择服务器界面
	
	MsgTips = require 'View/MsgBox/MsgTips',								--显示弹出Tips
	MsgBox = require 'View/MsgBox/MsgBox',									--弹出确认框
}

--Model名称列表--
ModelList = 
{
	ErrorModel = require 'Model/ErrorModel',								--错误码
	LoginModel = require 'Model/LoginModel',								--登录模块
}
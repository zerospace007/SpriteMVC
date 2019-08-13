HandleType = 			--切换类型（分别切换到内城、野外、世界）
{
	Build = 1,			--内城
	BuildFeild = 2,		--野外
	World = 3,			--世界
	GameLevelMap = 4,	--关卡地图
}

local transform;
local gameObject;

--地图摄像机控制
CameraHandler = BaseView:New('CameraHandler');
local this = CameraHandler;

local BuildLimitX = {minX = 0, maxX = 820};							--内城建筑界面摄像机X轴限制
local BuildFeildLimitX = {minX = 0, maxX = 2300};					--野外建筑界面摄像机X轴限制
local GameLvlMapLimitY = {minY = 0, maxY = 1330};					--关卡地图界面摄像机Y轴限制
local SmoothCoefficient = 500;										--平滑系数
local CameraMoveSpeed = 0.03;										--摄像机移动速度

local m_CenterPos = Vector2.zero;									--当前中心点坐标（x，y）int
local m_PosList = {};												--九宫格坐标

--初始化相机控制
function CameraHandler.InitCameraHandler()
	gameObject = GameObject.FindWithTag('MapCamera');
	transform = gameObject.transform;
	
	this.PanelWorldPos = transform:Find('PanelWorldPos');
	this.PosImage = this.PanelWorldPos:Find('ImgBg'):GetComponent(typeof(Image));
	this.TxtWorldPos = this.PanelWorldPos:Find('TxtPos'):GetComponent(typeof(Text));
	this.PanelWorldPos.gameObject:SetActive(false);
	this.Color1 = this.PosImage.color;
	this.Color2 = this.TxtWorldPos.color;
	this.Color1_Alpha = Color.New(this.Color1.r, this.Color1.g, this.Color1.b, 0);
	this.Color2_Alpha = Color.New(this.Color2.r, this.Color2.g, this.Color2.b, 0);
	
	this.IsLeftButtonPressed = false;								--鼠标左键或者手指是否按下
end

--开始运行
function CameraHandler.Start()
	this.running = true;
	if not this.Handler then
		this.Handler = UpdateBeat:CreateListener(this.Update);			--创建监听
	end
	
	UpdateBeat:AddListener(this.Handler);
end

--停止运行
function CameraHandler.Stop()
	this.running = false;
	if this.Handler then UpdateBeat:RemoveListener(this.Handler); end
end

--设置相机类型
function CameraHandler.SetSwitchType(handleType)
	this.Start();
	this.HandleType = handleType;
	if this.HandleType == HandleType.World then
		transform:SetAsLastSibling();	--显示最前
	end
end

local coTween;
--设置中心点
function CameraHandler.SetPosition(position, isSetPos)
	if this.HandleType == HandleType.Build then
		transform.localPosition = position;
	elseif this.HandleType == HandleType.BuildFeild then
		transform.localPosition = position;
	elseif this.HandleType == HandleType.World then
		m_CenterPos = position;
		this.GetPosList();					--获取中心点周围九宫格
		if coTween then coroutine.stop(coTween) end;
		coTween = coroutine.start(this.TweenColor);	--透明度渐变Tween
		this.TxtWorldPos.text = position.x .. ',' .. position.y;
		
		if isSetPos then
			local localPositionX = MapSetUp.ItemWidth * (position.x - position.y);
			local localPositionY = MapSetUp.ItemHeight * (position.x + position.y);
			transform.localPosition = Vector3.New(localPositionX, localPositionY, 0);
		end
		
		this.SendNotification(NotifyName.UpdateCenterPos, m_CenterPos);
	elseif this.HandleType == HandleType.GameLevelMap then
		transform.localPosition = position;
	end
end

--Tween动画
function CameraHandler.TweenColor()
	this.PanelWorldPos.gameObject:SetActive(true);
	DOTween.Complete(this.PosImage.gameObject);				--完成Tween
	DOTween.Complete(this.TxtWorldPos.gameObject);			--完成Tween
	this.PosImage.color = this.Color1_Alpha;
	this.TxtWorldPos.color = this.Color2_Alpha;
	this.PosImage:DOColor(this.Color1, 1);
	this.TxtWorldPos:DOColor(this.Color2, 1);
	coroutine.wait(3.5);
	this.PosImage:DOColor(this.Color1_Alpha, 1);
	coroutine.wait(1);
	this.TxtWorldPos:DOColor(this.Color2_Alpha, 1);
	coroutine.wait(1);
	this.PanelWorldPos.gameObject:SetActive(false); 
end

local up = Vector2.up;
local down = Vector2.down;
local left = Vector2.left;
local right = Vector2.right;
local up_left = Vector2.__add(up, left);
local up_right = Vector2.__add(up, right);
local down_left = Vector2.__add(down, left);
local down_right = Vector2.__add(down, right);

--获取中心点周围九宫格
function CameraHandler.GetPosList()
	m_PosList = {};
	table.insert(m_PosList, m_CenterPos);
	
	local tempPos = Vector2.__add(m_CenterPos, up);
	table.insert(m_PosList, tempPos);
	local tempPos = Vector2.__add(m_CenterPos, down);
	table.insert(m_PosList, tempPos);
	local tempPos = Vector2.__add(m_CenterPos, left);
	table.insert(m_PosList, tempPos);
	local tempPos = Vector2.__add(m_CenterPos, right);
	table.insert(m_PosList, tempPos);
	
	local tempPos = Vector2.__add(m_CenterPos, up_left);
	table.insert(m_PosList, tempPos);
	local tempPos = Vector2.__add(m_CenterPos, up_right);
	table.insert(m_PosList, tempPos);
	
	local tempPos = Vector2.__add(m_CenterPos, down_left);
	table.insert(m_PosList, tempPos);
	local tempPos = Vector2.__add(m_CenterPos, down_right);
	table.insert(m_PosList, tempPos);
end

local mapLayer = LayerMask.NameToLayer('MapLayer');
--长按
local function LeftPressed()
	local current = EventSystem.current;
	local currentGameObject = current.currentSelectedGameObject;
	if currentGameObject and Util.IsNotNull(currentGameObject) then
		if currentGameObject.layer == mapLayer then
			this.IsLeftButtonPressed = true;
		end
	else 
		this.IsLeftButtonPressed = true;
	end
end

--Update
function CameraHandler.Update()
	if Application.isMobilePlatform then
		if (Input.touchCount > 0 and Input.GetTouch(0).phase == TouchPhase.Moved) then
			LeftPressed();
		end
	else
		if Input.GetMouseButtonDown(0) then
			LeftPressed();
		end
	end
	
	if Application.isMobilePlatform then
		if (Input.touchCount > 0 and Input.GetTouch(0).phase == TouchPhase.Ended) then
			this.IsLeftButtonPressed = false;
		end
	else
		if Input.GetMouseButtonUp(0) then
			this.IsLeftButtonPressed = false;
		end
	end
	
	if this.HandleType == HandleType.World then
		if this.IsLeftButtonPressed then
			local posX, posY;
			if Application.isMobilePlatform then
				local deltaPosition = Input.GetTouch(0).deltaPosition;
				posX = deltaPosition.x * CameraMoveSpeed;
				posY = deltaPosition.y * CameraMoveSpeed;
			else
				posX = Input.GetAxis("Mouse X");
				posY = Input.GetAxis("Mouse Y");
			end
			
			local movPos = Vector3.New(-posX * CameraMoveSpeed, -posY * CameraMoveSpeed, 0);
			this.UpdateCenterPos(movPos);
		end
	elseif this.HandleType == HandleType.Build then
		
		if this.IsLeftButtonPressed then
			local posX;
			if Application.isMobilePlatform then
				local deltaPosition = Input.GetTouch(0).deltaPosition;
				posX = deltaPosition.x * CameraMoveSpeed;
			else
				posX = Input.GetAxis("Mouse X");
			end
			if transform.localPosition.x <= BuildLimitX.maxX and transform.localPosition.x >= BuildLimitX.minX then 
				local movPos = Vector3.New(-posX * CameraMoveSpeed, 0, 0);
				transform:Translate(movPos);
			end
		end
		
		if transform.localPosition.x > BuildLimitX.maxX then
			transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.New(BuildLimitX.maxX - 5, 0, 0), Time.deltaTime * SmoothCoefficient);
		elseif transform.localPosition.x < BuildLimitX.minX then
			transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.New(BuildLimitX.minX + 5, 0, 0), Time.deltaTime * SmoothCoefficient);
		end
	elseif this.HandleType == HandleType.BuildFeild then
		
		if this.IsLeftButtonPressed then
			local posX;
			if Application.isMobilePlatform then
				local deltaPosition = Input.GetTouch(0).deltaPosition;
				posX = deltaPosition.x * CameraMoveSpeed;
			else
				posX = Input.GetAxis("Mouse X");
			end
			if transform.localPosition.x <= BuildFeildLimitX.maxX and transform.localPosition.x >= BuildFeildLimitX.minX then 
				local movPos = Vector3.New(-posX * CameraMoveSpeed, 0, 0);
				transform:Translate(movPos);
			end
		end
		
		if transform.localPosition.x > BuildFeildLimitX.maxX then
			transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.New(BuildFeildLimitX.maxX - 5, 0, 0), Time.deltaTime * SmoothCoefficient);
		elseif transform.localPosition.x < BuildFeildLimitX.minX then
			transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.New(BuildFeildLimitX.minX + 5, 0, 0), Time.deltaTime * SmoothCoefficient);
		end
	elseif this.HandleType == HandleType.GameLevelMap then
		
		if this.IsLeftButtonPressed then
			local posY;
			if Application.isMobilePlatform then
				local deltaPosition = Input.GetTouch(0).deltaPosition;
				posY = deltaPosition.y * CameraMoveSpeed;
			else
				posY = Input.GetAxis("Mouse Y");
			end
			if transform.localPosition.y <= GameLvlMapLimitY.maxY and transform.localPosition.y >= GameLvlMapLimitY.minY then 
				local movPos = Vector3.New(0,-posY * CameraMoveSpeed, 0);
				transform:Translate(movPos);
			end
		end
		
		if transform.localPosition.y > GameLvlMapLimitY.maxY then
			transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.New(0,GameLvlMapLimitY.maxY - 5,  0), Time.deltaTime * SmoothCoefficient);
		elseif transform.localPosition.y < GameLvlMapLimitY.minY then
			transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.New(0,GameLvlMapLimitY.minY + 5, 0), Time.deltaTime * SmoothCoefficient);
		end
	end
end

local tipsNode = 1;
--相机移动，计算地图中心点
function CameraHandler.UpdateCenterPos(movPos)
	local index = 1;--假设第一个元素为最小值 那么下标设为0
	for node, pos in ipairs(m_PosList) do
		local minPos = m_PosList[index];
		local position = m_PosList[node];
		minPos = Vector2.New(MapSetUp.ItemWidth * (minPos.x - minPos.y), MapSetUp.ItemHeight * (minPos.x + minPos.y));
		position = Vector2.New(MapSetUp.ItemWidth * (position.x - position.y), MapSetUp.ItemHeight * (position.x + position.y));
		
		if Vector2.Distance(transform.localPosition, minPos) > Vector2.Distance(transform.localPosition, position) then
			index = node;
		end
	end
	
	local newPosition = m_PosList[index];
	if (Vector2.Distance(m_CenterPos, newPosition) > 0) then 
		local isOutOfBounds = Util.IsOutofBounds(newPosition, MapSetUp.MinPos, MapSetUp.MaxPos);
		if isOutOfBounds then
			log("越界了！" .. " x:" .. newPosition.x .. " y:" .. newPosition.y);
			if tipsNode == 1 then TipsManager:ShowTips("<color=yellow>越界了！</color>"); end
			local localPositionX = MapSetUp.ItemWidth * (m_CenterPos.x - m_CenterPos.y);
			local localPositionY = MapSetUp.ItemHeight * (m_CenterPos.x + m_CenterPos.y);
			transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.New(localPositionX, localPositionY, 0), Time.deltaTime * SmoothCoefficient);
			tipsNode = tipsNode + 1;
			return;
		end
		tipsNode = 1;
		this.SetPosition(newPosition, false);
	end
	transform:Translate(movPos);
end

--释放View对象
function CameraHandler.onDestroy()
	this:Destroy();
end

--通知列表--
this.NotifyList = 
{
	{notifyName = NotifyName.InitCameraHandler, func = this.InitCameraHandler},
	{notifyName = NotifyName.SetSwitchType, func = this.SetSwitchType},
	{notifyName = NotifyName.SetPosition, func = this.SetPosition},
}

return this;
--region SoundManager.lua
--Date 2019/05/29
--声音音效管理
--endregion
SoundName =													--音乐音效名称
{
	--背景音乐
	BgmLogin = 'BgmLogin',									--登录背景音乐
	
	--普通音效
	Start = 'Start',										--开始游戏
	Click = 'Click',										--点击按钮
	Close = 'Close',										--关闭按钮
	OpenUI = 'OpenUI',										--打开显示界面
}

SoundManager = {};											--声音音效管理
local this = SoundManager;
this.__index = this;
local Audios = {};

--声音音效管理初始化，载入并存储音效剪辑
function SoundManager.Init()
	this.isCanBgm = true;		--临时赋值
	this.isCanAudio = true;
	
	if (not this.AudioSource) then
		local GameManagerObj = Find('GameManager');
		this.AudioSource = GameManagerObj:AddComponent(typeof(AudioSource));
		this.AudioSource.playOnAwake = false;
	end
	
	this.LoadAudios();
end

--加载所有声音声效
function SoundManager.LoadAudios()
	local function LoadAudio(key)
		local function AudioLoaded(objs)
			local audioClip = objs[0];
			Audios[key] = audioClip;
		end
		ResourceManager:LoadPrefab(BundleType.Audios .. '/' .. key, key, AudioLoaded);
	end
	
	this.Sound = ConfigAll.GetConfig(ConfigName.Sound);
	local Sound = this.Sound;
	for key, voSoundItem in pairs(Sound) do
		LoadAudio(key);
	end
end

-- 播放背景声音
-- <param name="audioName">背景音乐名称</param>
function SoundManager.PlayBackSound(audioName)
	local audioClip = Audios[audioName];
	if (not audioClip) then return; end
	
	if (this.isCanBgm) then
		this.AudioSource.loop = true;
		this.AudioSource.clip = audioClip;
		this.AudioSource:Play();
	else
		this.AudioSource:Stop();
	end
end

--暂停背景音乐
function SoundManager.StopBacksound()
	this.AudioSource:Stop();
end

-- 播放一段音效
-- <param name="path">音效名称</param>
function SoundManager.Play(audioName)
	if (not this.isCanAudio) then return end
	local audioClip = Audios[audioName];
	if (not audioClip) then return end
	this.PlayAtPoint(audioClip, Vector3.zero);
end
	
-- 在position上播放一段音效
-- <param name="clip">音效剪辑</param>
-- <param name="position">坐标位置</param>
function SoundManager.PlayAtPoint(clip, position)
	AudioSource.PlayClipAtPoint(clip, position);
end
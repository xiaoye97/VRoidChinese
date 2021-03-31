# VRoidChinese
VRoid汉化插件

## 特性
- 运行时对软件进行汉化，不修改软件本体。
- 通过配置文件进行文本配置

## 使用方法
#### 安装
1. 确保软件路径没有中文或其他特殊字符!!!确保软件路径没有中文或其他特殊字符!!!确保软件路径没有中文或其他特殊字符!!!
2. 下载Releases中的最新版本
3. 解压到软件根目录，解压之后的文件结构如下(标星号*的为汉化插件，请确保路径正确)

注:Steam版本不带后面的-vx.y.z-win，其他同理
```
|-VRoidStudio-vx.y.z-win
  |-BepInEx*
    |-config
      |-BepInEx.cfg
      |-VRoid.Chinese.cfg
    |-core
      |- ...
    |-plugins
      |-VRoidChinese.dll
  |-VRoidStudio_Data
  |-doorstop_config.ini*
  |-UnityCrashHandler64.exe
  |-UnityPlayer.dll
  |-VRoidStudio.exe
  |-winhttp.dll*
```
4. GIF图示

![image](https://github.com/xiaoye97/VRoidChinese/blob/master/VRoidStudioChineseInstallTutorial.gif) 

#### 更新翻译
1. 如果插件没有更新，但是仓库中的翻译有更新，则从仓库下载VRoid.Chinese.cfg覆盖到BepInEx/config文件夹即可


## 问题排查
##### 因软件更新导致的汉化失效?在GitHub上提交issue提醒开发人员!
##### 出现了不可名状的bug?在GitHub上提交issue提醒开发人员!并汇报插件运行日志.
(日志文件定位:%RootLetter%:\install location...\VRoid Studio\BepInEx\LogOutput.log)
ps: 在..BepInEx\config\BepInEx.cfg中可以修改日志等级,如果你希望你的issue能够得到更有用的回复, 或者帮助开发人员了解bug的详细, 请根据此文件内的提示把日志记录等级开到最大.

##### 不知道为什么汉化失效?按照Q&A进行自我排查!
### Q&A
Q: 我的vroid是steam上下载的, 在一次更新后汉化失效, 完全无法在vroid欢迎页面选择到中文!我要如何解决.
A:  1.检查GitHub上插件是否更新, 若更新请在release中下载最新版本, 在文件目录中**覆盖**之前的文件.
    2.尝试重新安装插件, 甚至重新安装软件后安装插件.
    3.下载使用集成了插件的vroid软件,例如
    4.待续, 任何问题请积极提交.

## 帮助翻译
1. 使用外部文本编辑器，如notepad++，记事本等打开VRoid.Chinese.cfg
2. 对照英文默认值进行翻译
3. 运行VRoid Studio进行测试
4. 确认无误后，提交PR或者直接发给我(QQ:1066666683 ~~VRoid交流群:418069375~~ QQ群:684544577)

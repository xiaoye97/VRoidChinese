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
4. 启动软件，软件启动时会比加汉化之前稍慢一些，是正常现象
5. GIF图示

![image](https://github.com/xiaoye97/VRoidChinese/VRoidStudioChineseInstallTutorial.gif) 

#### 更新翻译
1. 如果插件没有更新，但是仓库中的翻译有更新，则从仓库下载VRoid.Chinese.cfg覆盖到BepInEx/config文件夹即可

## 帮助翻译
#### 修改配置文件法
1. 使用外部文本编辑器，如VSCode，记事本等打开VRoid.Chinese.cfg
2. 对照英文默认值进行翻译
3. 运行VRoid Studio进行测试

#### 软件运行时修改翻译文本法
1. 下载ConfigurationManager(https://github.com/BepInEx/BepInEx.ConfigurationManager)
2. 将ConfigurationManager.dll放入BepInEx/plugins文件夹
3. 在VRoid Studio中，按F1打开配置文件管理器，直接修改对应词条的翻译文本
4. 按F5保存配置文件，部分翻译在刷新页面时会直接生效，部分需要重启软件生效

#### 提交翻译或校对
1. 提交PR或者直接发给我(QQ:1066666683 VRoid交流群:418069375)

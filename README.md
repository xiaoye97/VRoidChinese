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

## 帮助翻译
1. 使用外部文本编辑器，如VSCode，记事本等打开VRoid.Chinese.cfg
2. 对照英文默认值进行翻译
3. 运行VRoid Studio进行测试
4. 确认无误后，提交PR或者直接发给我(QQ:1066666683 VRoid交流群:418069375)

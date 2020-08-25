# VRoidChinese
VRoid汉化插件

### 特性
- 运行时对软件进行汉化，不修改软件本体。
- 通过配置文件进行文本配置

### 使用方法
1. 下载BepInEx 64位，解压到软件根目录(https://github.com/BepInEx/BepInEx/releases)
2. 将VRoidChinese.dll放入BepInEx/plugins文件夹(如果没有则新建)
3. 将me.xiaoye97.plugin.VRoid.Chinese.cfg放入BepInEx/config文件夹(如果没有则新建)
4. 启动软件

### 帮助翻译
#### 修改配置文件法
1. 使用外部文本编辑器，如VSCode，记事本等打开me.xiaoye97.plugin.VRoid.Chinese.cfg
2. 对照英文默认值进行翻译
3. 运行软件进行测试

#### 软件运行时修改翻译文本法
1. 下载ConfigurationManager(https://github.com/BepInEx/BepInEx.ConfigurationManager)
2. 将ConfigurationManager.dll放入BepInEx/plugins文件夹
3. 在软件中，按F1打开配置文件管理器，直接修改对应词条的翻译文本
4. 按F5保存配置文件，部分翻译在刷新页面时会直接生效，部分需要重启软件生效

#### 提交翻译
1. 提交PR或者直接发给我(QQ:1066666683 VRoid交流群:418069375)
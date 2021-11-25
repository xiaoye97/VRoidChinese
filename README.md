# VRoidChinese

VRoid 汉化插件

## 免费声明
本插件为完全免费插件，禁止任何形式的售卖，近日看到有淘宝无良卖家在贩卖VRoidStudio软件本体和汉化插件，请大家注意警惕，如果看到请帮忙举报一下，谢谢。

## 简介

- 基于[BeplnEx][1]
- 运行时对软件进行汉化, 不修改软件本体。
- 通过配置文件进行翻译配置

## 使用方法

tldr: [免安装绿色硬盘汉化版][2]

#### 视频教程

+ [bilibili][3]

#### 文字教程

1. 下载 `Releases` 中的最新版本(`不是点击绿色的Code下的Download Zip，而是右侧的Releases`)
2. 解压到软件根目录, 解压之后的目录结构如下 (标星号\* 的为汉化插件, 请确保路径正确)

```
|-VRoid Studio
  |-BeplnEx*                  BeplnEx 框架
    |-config                  BepInEx 的设置文件夹
      |-BepInEx.cfg           BeplnEx 的设置文件
    |-plugins                 BepInEx 插件存放目录
      |-VRoidChinese.dll      汉化插件
  |-Chinese*                  汉化资源文件夹
    |-MessagesChinese.json    汉化文件
    |-StringChinese.txt       汉化文件
  |-MonoBleedingEdge          软件的资源文件夹
  |-VRoidStudio_Data          软件的资源文件夹
  |-VRoidStudio.exe           启动软件的执行文件
  |-doorstop_config.ini*      BeplnEx 的文件
  |-winhttp.dll*              BeplnEx 的文件
  |-..                        乱七八糟的文件们...
```

3. GIF 图示

![教程](https://cdn.jsdelivr.net/gh/xiaoye97/VRoidChinese@master/Asset/VRoidStudioChineseInstallTutorial.gif)

#### 更新翻译

- 若插件本身未更新, 但是翻译有更新, 则从仓库下载 Chinese 文件夹, 并覆盖软件根目录中的 Chinese 文件夹.

## 问题排查

##### 因软件更新导致的汉化失效? 提交 issue 提醒开发人员.

##### 出现了不可名状的 bug? 提交 issue 提醒开发人员! 并汇报插件运行日志.

(日志文件定位:%RootLetter%:\install location...\VRoid Studio\BepInEx\LogOutput.log)

ps:

在..BepInEx\config\BepInEx.cfg 中可以修改日志等级,如果你希望你的 issue 能够得到更有用的回复, 或者帮助开发人员了解 bug 的详细, 请根据此文件内的提示把日志记录等级开到最大.

##### 不知道为什么汉化失效?按照 Q&A 进行自我排查

### Q&A

Q:

我的 vroid 是 steam 上下载的, 在一次更新后汉化失效, 完全无法在 vroid 欢迎页面选择到中文! 我要如何解决?

A:

1. 检查 GitHub 上插件是否更新, 若有更新请在 release 中下载最新版本, 在文件目录中**覆盖**之前的文件. 若无效, 请参考下一条.
2. 尝试重新安装插件, 甚至重新安装软件后安装插件. 若无效请参考下一条.
3. 下载使用集成了插件的 vroid 软件, 例如[微云文件][2], 若无效请参考下一条.
4. 定位到 c:\用户\你的用户名\AppData\LocalLow\pixiv\VRoid Studio\Player.log
   浏览此文件, 若存在如下字段

   ```
   ...
    <RI> Initialized touch support.
    UnloadTime: x.xxxxxx ms
   ...
   ```

   且在此之后没有看到任何形如

   ```
   [Message:   BepInEx] BepInEx 5.4.17.0 - VRoidStudio
   [Info   :   BepInEx] Running under Unity v2020.3.19.6877495
   ```

   的字段出现, 请按照以下流程进行解决:

   ```
   * 删除位于 c:\用户\你的用户名\AppData\LocalLow 下的pixiv文件夹, 即上述Player.log所在的父目录.
   * 卸载vroid软件及汉化插件 (直接删除也行).
   * 重新启动计算机.
   * 安装vroid软件及汉化插件.
   * done!
   ```

5. 待续, 任何问题请积极提交.

## 帮助翻译/校对

1. 直接修改 Chinese\MessagesChinese.json 中的值即可实现翻译.
2. 校对时, 请参考 Asset\旧版校对用翻译.txt 和 Asset\通用翻译参考.json 中的词条 (\_en 的文件是英文原文).
3. 完成校对或翻译后, 运行 VRoid Studio 进行测试.
4. 若确认无误, 请提交 PR 或者直接反馈给我 (QQ:1066666683 & VRoidStudio交流群:684544577 & 宵夜食堂:528385469).

[1]: https://github.com/BepInEx/BepInEx/releases
[2]: https://share.weiyun.com/cgPKjcxc
[3]: https://www.bilibili.com/video/BV1BL41137Tc/

# VRoidChinese

VRoid 汉化插件

## 1.26.1 开发环境配置
1. 下载最新编译的 https://builds.bepinex.dev/projects/bepinex_be
2. 放进 vroid 运行一次, 会在 `VRoid Studio\BepInEx\interop` 生成原先 `*_Managed` 里的各种 .dll 
3. 引用这些 dll, done.

## 免费声明

本插件 (软件) 为[自由软件][5], 使用 [MIT][6] 协议, 永久免费, 禁止任何形式的售卖.

有不法分子在网络上贩卖 VRoidStudio 软件本体和汉化插件, 请注意警惕, 如有遇到请务必举报, 谢谢.

## 为什么在VRoid Studio 1.26.1版本之后用不了了？
从1.26.1版本开始, VroidStudio从mono切换到了il2cpp, 所有的插件都失效了. 并且因为il2cpp的插件开发较为麻烦并且本人最近也没有什么时间. 所以, 如果你想继续使用插件, 可以先从官网下载1.26.0版本后安装插件. 或者在QQ交流群684544577下载1.26.0带插件版本整合包.

---

## 简介

- 基于 [BeplnEx][1]
- 运行时对软件进行汉化, 不修改软件本体.
- 通过配置文件进行翻译配置.

### [视频教程][3]

### 文字教程

1. 下载 `Releases` 中的[最新版本][4]
2. 解压到软件根目录, 解压之后的目录结构如下 (标星号\* 的为汉化插件, 请确保路径正确)

```files
|-VRoid Studio
  |-BeplnEx*                  BeplnEx 框架
    |-config                   BepInEx 的设置文件夹
      |-BepInEx.cfg           BeplnEx 的设置文件
    |-plugins                 BepInEx 插件存放目录
      |-VRoidChinese.dll      汉化插件
  |-Chinese*                  汉化资源文件夹
    |-MessagesChinese.json    汉化文件
    |-StringChinese.txt       汉化文件
  |-MonoBleedingEdge          软件的资源文件夹
  |-VRoidStudio_Data          软件的资源文件夹
  |-VRoidStudio.exe           启动软件的执行文件
  |-doorstop_config.ini*       BeplnEx 的文件
  |-winhttp.dll*              BeplnEx 的文件
  |-..                        乱七八糟的文件们...
```

#### GIF 图示

![教程](Asset/VRoidStudioChineseInstallTutorial.gif)

#### 更新翻译

- 若插件本身未更新, 但是翻译有更新, 则从仓库下载 Chinese 文件夹, 并覆盖软件根目录中的 Chinese 文件夹.

## 问题排查

### 因软件更新导致的汉化失效?

提交 issue 提醒开发人员

### 出现了不可名状的 bug? 提交 issue 提醒开发人员! 并汇报插件运行日志

日志文件定位: %RootLetter%:\install location...\VRoid Studio\BepInEx\LogOutput.log

ps:

在 ..BepInEx\config\BepInEx.cfg 中可以修改日志等级,如果你希望你的 issue 能够得到更有用的回复, 或者帮助开发人员了解 bug 的详细, 请根据此文件内的提示把日志记录等级开到最大.

### 不知道为什么汉化失效?按照 Q&A 进行自我排查

### Q&A

Q:

我的 vroid 是 steam 上下载的, 在一次更新后汉化失效, 完全无法在 vroid 欢迎页面选择到中文! 我要如何解决?

A:

1. 检查 GitHub 上插件是否更新, 若有更新请在 release 中下载最新版本, 在文件目录中**覆盖**之前的文件. 若无效, 请参考下一条.
2. 尝试重新安装插件, 甚至重新安装软件后安装插件. 若无效请参考下一条.
3. 下载使用集成了插件的 vroid 软件, 例如[微云文件][2], 若无效请参考下一条.
4. 定位到 c:\用户\你的用户名\AppData\LocalLow\pixiv\VRoid Studio\Player.log
   浏览此文件, 若存在如下字段

    ```text
    ...
     <RI> Initialized touch support.
     UnloadTime: x.xxxxxx ms
    ...
    ```

    且在此之后没有看到任何形如

    ```text
    [Message:   BepInEx] BepInEx 5.4.17.0 - VRoidStudio
    [Info   :   BepInEx] Running under Unity v2020.3.19.6877495
    ```

    的字段出现, 请按照以下流程进行解决:

    ```text
    * 删除位于 c:\用户\你的用户名\AppData\LocalLow 下的 pixiv 文件夹, 即上述 Player.log 所在的父目录.
    * 卸载 vroid 软件及汉化插件 (直接删除也行).
    * 重新启动计算机.
    * 安装 vroid 软件及汉化插件.
    * done!
    ```

5. 待续, 任何问题请积极提交.

## 帮助翻译/校对

1. 直接修改 Chinese\MessagesChinese.json 中的值即可实现翻译.
2. 校对时, 请参考 Asset\旧版校对用翻译.txt 和 Asset\通用翻译参考.json 中的词条 (\_en 的文件是英文原文).
3. 完成校对或翻译后, 运行 VRoid Studio 进行测试.
4. 若确认无误, 请提交 PR 或者直接反馈给我 (QQ: 1066666683 & VRoidStudio 交流群: 684544577 & 宵夜食堂: 528385469).

[1]: https://github.com/BepInEx/BepInEx/releases
[3]: https://www.bilibili.com/video/BV1BL41137Tc/
[4]: https://github.com/xiaoye97/VRoidChinese/releases/latest
[5]: https://zh.wikipedia.org/zh-cn/%E8%87%AA%E7%94%B1%E8%BD%AF%E4%BB%B6
[6]: https://mit-license.org/

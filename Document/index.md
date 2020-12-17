See [Document](https://macacagames.github.io/OnionDataEditor/) for more detail.

# Onion Data Editor #

Onion 是一個於 Unity 使用的資料檢視與編輯工具。

### 他可以做什麼？ ###

* 可以透過簡單的 Attribute 快速建構資料間的階層關係。
* 在定義好階層關係的資料下圖像化階層關係，可快速訪問與編輯各個資料。
* 針對特殊需求，可以自訂方法，並可在介面快速使用。

### 如何開始使用？ ###

### Option 1: Unity Package manager
Add it to your editor's `manifest.json` file like this:
```json
    {
    "dependencies": {
        "com.macacagames.oniondataeditor": "https://github.com/MacacaGames/OnionDataEditor.git",
    }
}
```

### Option 2: Git SubModule

```bash
git submodule add https://github.com/MacacaGames/OnionDataEditor.git Assets/OnionDataEditor
```
* 加入後，可在 Unity 的 Window/Onion Data Editor 開啟視窗介面。

### 基本使用範例 ###

我們先寫兩個 Script ，分別為 AreaData 與 MonsterData，他們都繼承自QueryableData。

```
using OnionCollections.DataEditor;

public class AreaData : QueryableData
{
    [NodeTitle]
    public string areaName;

    [OnionCollections.DataEditor.NodeElement]
    public MonsterData[] monsterDatas;
}
```
```
using OnionCollections.DataEditor;

public class MonsterData : QueryableData
{
    [NodeTitle]
    public string monsterName;
    
    public int hp;
    public int atk;    
}
```

可以注意到 AreaData 中包含了數個 MonsterData，在任意 IEnumerable 的 Field 或 Property 上加上 [OnionCollections.DataEditor.NodeElement] 的 Attribute 後，這些內容就會在視窗介面上成為這個 AreaData 的子節點。

![...](https://i.imgur.com/XPxe2DS.png)

這樣就會有最基本的階層狀態，可以開始使用這個工具最核心的功能了。


### 進階使用範例 ###

若有需要，請參閱專案中的 Examples。
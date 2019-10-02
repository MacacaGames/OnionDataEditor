# Onion Data Editor #

Onion 是一個於 Unity 使用的資料檢視與編輯工具。

### 他可以做什麼？ ###

* 可以透過簡單的 Attribute 快速建構資料間的階層關係。
* 在定義好階層關係的資料下圖像化階層關係，可快速訪問與編輯各個資料。
* 針對特殊需求，可以自訂方法，並可在介面快速使用。

### 如何開始使用？ ###

* 建議以 SubModules 方式加入你的專案。
* 加入後，可在 Unity 的 Window/Onion Data Editor 開啟視窗介面。

### 基本使用範例 ###

我們先寫兩個 ScriptableObject ，分別為 AreaData 與 MonsterData。

```
[CreateAssetMenu(fileName = "AreaData", menuName = "Custom/AreaData")]
public class AreaData : ScriptableObject
{
    public string areaName;

    [Onion.NodeElement]
    public MonsterData[] monsterDatas;
}
```
```
[CreateAssetMenu(fileName = "MonsterData", menuName = "Custom/MonsterData")]
public class MonsterData : ScriptableObject
{
    public string monsterName;
    
    public int hp;
    public int atk;    
}
```

可以注意到 AreaData 中包含了數個 MonsterData，在任意 IEnumerable 的 Field 或 Property 上加上 [Onion.NodeElement] 的 Attribute 後，這些內容就會在視窗介面上成為這個 AreaData 的子節點。

注意：一個 ScriptableObject 中，只會承認第一個套上 NodeElement 的 Field/Property 其餘的會被無視。

![...](https://i.imgur.com/XPxe2DS.png)

這樣就會有最基本的階層狀態，可以開始使用這個工具最核心的功能了。


### 進階使用範例 ###

開啟專案中的 Examples ，依照實用度、難易度，將功能範例分成數份，若有需要可以參閱。

因為，我...沒辦法掰出更多字了。
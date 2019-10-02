using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AreaData", menuName = "Custom/AreaData")]
public class AreaData : ScriptableObject
{
    /*
     * 掛上NodeTitle的Field、Property，型別須為string，
     * 加入後即可成為節點的標題，因為可以允許Property，所以也可以有一些特殊變化。
     */

    [Onion.NodeTitle]
    public string areaName;

    /*
    [Onion.NodeTitle]
    public string areaName => $"Area [{this.name}]";
    */




    /*
     * 掛上NodeElement的Field、Property，型別須為IEnumerable<ScriptableObject>，
     * 加入後即可成為子節點，是最核心的功能。
     */

    [Onion.NodeElement]
    public MonsterData[] monsterDatas;



}


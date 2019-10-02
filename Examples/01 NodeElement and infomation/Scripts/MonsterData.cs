using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Custom/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Onion.NodeTitle]
    public string monsterName;

    /*
     * 掛上NodeDescription的Field、Property，型別須為string，
     * 加入後即可成為節點的描述，在選擇節點時會顯示在資訊區塊，同樣你也可以用Property做一些處理。
     */
    
    [Onion.NodeDescription]
    public string monsterDescription;
    
    /*
    [Onion.NodeDescription]
    public string monsterDescription => $"HP = {hp}\nATK = {atk}";
    */

    public int hp;
    public int atk;
    
}

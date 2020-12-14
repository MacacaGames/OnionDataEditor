using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OnionCollections.DataEditor;  // Using namespace to use attribute easily.

//[CreateAssetMenu(fileName = "MonsterData", menuName = "Custom/MonsterData")]    //If you want to test, uncomment this line and create asset.
public class MonsterData : QueryableData
{
    /*
     * The attribute 'NodeTitle' can attach on field and property.
     * Field or property type must be string.
     */

    [NodeTitle]
    public string monsterName;

    /*
     * 掛上NodeDescription的Field、Property，型別須為string，
     * 加入後即可成為節點的描述，在選擇節點時會顯示在資訊區塊，同樣你也可以用Property做一些處理。
     */

    /*
     * The attribute 'NodeDescription' can attach on field and property.
     * Field or property type must be string.
     * Description will be display in Onion Data Editor when you select this node.
     */

    [NodeDescription]
    public string monsterDescription;
    

    //[Onion.NodeDescription]
    //public string monsterDescription => $"HP = {hp}\nATK = {atk}";
    

    public int hp;
    public int atk;

    public override string GetID() => monsterName;
}

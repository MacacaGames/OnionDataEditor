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
     * The attribute 'NodeDescription' can attach on field and property.
     * Field or property type must be string.
     * Description will be display in Onion Data Editor when you select this node.
     */

    [NodeDescription]
    public string monsterDescription;
    

    //[NodeDescription]
    //public string monsterDescription => $"HP = {hp}\nATK = {atk}";
    

    public int hp;
    public int atk;

    public override string GetID() => monsterName;
}

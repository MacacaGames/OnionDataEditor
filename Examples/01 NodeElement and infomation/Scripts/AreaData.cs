using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OnionCollections.DataEditor;  // Using namespace to use attribute easily.

//[CreateAssetMenu(fileName = "AreaData", menuName = "Custom/AreaData")]    //If you want to test, uncomment this line and create asset.
public class AreaData : QueryableData
{
    /*
     * The attribute 'NodeTitle' can attach on field and property.
     * Field or property type must be string.
     */

    [NodeTitle]
    public string areaName;

    /*
     * [NodeTitle]
     * public string areaName => $"Area [{this.name}]";
     */


    /*
     * The attribut 'NodeElement' can attach on field and property.
     * Field or property type must be IEnumerable<IQueryableData>.
     */

    [NodeElement]
    public MonsterData[] monsterDatas;


    public override string GetID() => areaName;
}


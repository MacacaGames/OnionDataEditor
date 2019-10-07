using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionData", menuName = "Custom/ActionData")]
public class ActionData : QueryableData
{
    [SerializeField]
    string speaker;

    [SerializeField]
    string speakContent;


    /*
     * 掛上NodeAction的Method，在選擇節點時，會成為按鈕讓你可以快速執行動作。
     */

    [Onion.NodeAction("Action01")]
    public void DoSomething()
    {
        Debug.Log($"{speaker} : {speakContent}");
    }



    /*
     * 掛上NodeAction的Method，在選擇節點時，會成為按鈕讓你可以快速執行動作。
     */

    [Onion.NodeAction("Action02")]
    public void DoMore()
    {
        Debug.Log($"{speaker} : !!!!!!!!!!!!!!!!!!!");
    }


    /*
     * 因為這個資料不需要再往下查找，就不用這麼做了
     */

    public override IEnumerable<IQueryableData> GetData()
    {
        throw new System.NotImplementedException();
    }

    public override string GetID()
    {
        throw new System.NotImplementedException();
    }
}


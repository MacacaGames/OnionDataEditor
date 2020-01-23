using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "ActionData", menuName = "Custom/ActionData")]    //想要測試時，可以使用這行來創造新的資料
public class ActionData : DataGroup
{
    [SerializeField]
    string speaker;

    [SerializeField]
    string speakContent;

#if(UNITY_EDITOR)

    /*
     * 掛上NodeAction的Method，在選擇節點時，會成為按鈕讓你可以快速執行動作。
     */

    [OnionCollections.DataEditor.NodeAction("Action01")]
    public void DoSomething()
    {
        Debug.Log($"{speaker} : {speakContent}");
    }



    /*
     * 掛上NodeAction的Method，在選擇節點時，會成為按鈕讓你可以快速執行動作。
     */

    [OnionCollections.DataEditor.NodeAction("Action02")]
    public void DoMore()
    {
        Debug.Log($"{speaker} : !!!!!!!!!!!!!!!!!!!");
    }

#endif 


    /*
     * 因為這個資料不需要再往下查找，就不用這麼做了
     */

    public IEnumerable<IQueryableData> GetData()
    {
        throw new System.NotImplementedException();
    }

    public string GetID()
    {
        throw new System.NotImplementedException();
    }
}


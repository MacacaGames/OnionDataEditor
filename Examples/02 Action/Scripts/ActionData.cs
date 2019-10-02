using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionData", menuName = "Custom/ActionData")]
public class ActionData : ScriptableObject
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

}


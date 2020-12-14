using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OnionCollections.DataEditor;  // Using namespace to use attribute easily.

//[CreateAssetMenu(fileName = "ActionData", menuName = "Custom/ActionData")]    //If you want to test, uncomment this line and create asset.
public class ActionData : QueryableData
{
    [SerializeField]
    string speaker;

    [SerializeField]
    string speakContent;


    /*
     * The attribute 'NodeAction' can attach on void function.
     * Action will be a button. Display in Onion Data Editor when you select this node.
     */


    [NodeAction("Action01")]
    public void DoSomething()
    {
        Debug.Log($"{speaker} : {speakContent}");
    }


    [NodeAction("Action02")]
    public void DoMore()
    {
        Debug.Log($"{speaker} : !!!!!!!!!!!!!!!!!!!");
    }

    public override string GetID() => speaker;
}


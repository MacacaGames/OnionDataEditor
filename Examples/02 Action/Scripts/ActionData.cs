using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OnionCollections.DataEditor;  // Using namespace to use attribute easily.

//[CreateAssetMenu(fileName = "ActionData", menuName = "Custom/ActionData")]    //If you want to test, uncomment this line and create asset.
public class ActionData : QueryableData
{
    [NodeTitle]
    [SerializeField]
    string speaker;

    [NodeDescription]
    [SerializeField]
    string speakContent;


    /*
     * The attribute 'NodeAction' can attach on void function.
     * Action will be a button. Display in Onion Data Editor when you select this node.
     */


    [NodeAction("Say something")]
    public void Action01()
    {
        Debug.Log($"{speaker} : {speakContent}");
    }


    [NodeAction("Say more")]
    public void Action02()
    {
        Debug.Log($"{speaker} : I CAN SAY MORE!!!!!!!!!!!!!!!!!!!");
    }

    public override string GetID() => speaker;
}


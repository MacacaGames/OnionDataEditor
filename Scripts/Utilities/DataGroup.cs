using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using OnionCollections.DataEditor;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

[OpenWithOnionDataEditor(true)]
[CreateAssetMenu(menuName = "Onion Data Editor/Data Group", fileName = "DataGroup")]
public class DataGroup : QueryableData, IEnumerable<IQueryableData>
{
    [SerializeField]
    string title;

    [SerializeField]
    string description;

    [NodeElement]
    [SerializeField]
    protected QueryableData[] data = new QueryableData[0];
    
    public IEnumerator<IQueryableData> GetEnumerator()
    {
        foreach(var item in data)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var item in data)
            yield return item;
    }

    public override string GetID() => title;

    public QueryableData this[string id] => this.QueryByID<QueryableData>(id);
    public QueryableData this[int index] => this.ElementAt(index) as QueryableData;


#if (UNITY_EDITOR)

    [NodeTitle]
    string Title => string.IsNullOrEmpty(title) ? name : title;

    [NodeDescription]
    string Description => description;

    [NodeIcon]
    Texture Icon => (IsDataHaveNull || IsDataHaveRepeatId) ?
        EditorGUIUtility.FindTexture("console.erroricon.sml") :
        EditorGUIUtility.IconContent("d_Folder Icon").image;

    public bool IsDataHaveNull => this.Any(_ => _ == null);
    public bool IsDataHaveRepeatId => this.GroupBy(_ => _.GetID()).Any(_ => _.Count() > 1);

#endif

}



#if (UNITY_EDITOR)

[CustomEditor(typeof(DataGroup))]
class DataGroupEditor : Editor
{
    DataGroup targetDataGroup;

    GUIStyle backgrounStyle;
    Texture2D backgroundTex;

    GUIStyle iconStyle;

    private void OnEnable()
    {
        targetDataGroup = target as DataGroup;


        backgroundTex = MakeTex(new Color(0.3F, 0.1F, 0.1F, 0.5F));

        Texture2D MakeTex(Color col)
        {
            Texture2D result = new Texture2D(1, 1);
            result.SetPixels(new[] { col });
            result.Apply();
            return result;
        }
    }

    public override void OnInspectorGUI()
    {
        if (backgrounStyle == null)
        {
            backgrounStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(0, 0, 0, 0)
            };
            backgrounStyle.normal.background = backgroundTex;
        }

        if(iconStyle == null)
        {
            iconStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(5, 5, 5, 5)
            };
            iconStyle.normal.background = EditorGUIUtility.IconContent("console.erroricon").image as Texture2D;
        }

        if (targetDataGroup.IsDataHaveNull == true)
        {
            ErrorInfo("Data have null!");
        }
        else if (targetDataGroup.IsDataHaveRepeatId == true)
        {
            ErrorInfo("Data have repeat id!");
        }


        base.OnInspectorGUI();


        void ErrorInfo(string text)
        {

            GUILayout.Space(10);
            using (new GUILayout.HorizontalScope(backgrounStyle, GUILayout.Height(32 + 10)))
            {
                GUILayout.Label("", iconStyle, GUILayout.Width(32), GUILayout.Height(32));

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUI.color = new Color(1F, 0.3F, 0.3F);
                    GUILayout.Label(text);
                    GUILayout.FlexibleSpace();
                }
            }
            GUILayout.Space(10);

            GUI.backgroundColor = new Color(1F, 0.5F, 0.5F);
            GUI.color = Color.white;

        }
    }
}


#endif
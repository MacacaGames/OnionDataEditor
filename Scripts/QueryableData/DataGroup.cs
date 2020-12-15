using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using OnionCollections.DataEditor;

#if(ODIN_INSPECTOR)
using Sirenix.OdinInspector;
#endif

[CreateAssetMenu(menuName = "Data/Data Group", fileName = "DataGroup")]
public class DataGroup : ScriptableObject, IEnumerable<IQueryableData>
{
    [SerializeField]
    string title;

    [SerializeField]
    string description;

#if (ODIN_INSPECTOR)

    bool dataHaveRepeatId() => this.GroupBy(_ => _.GetID()).Any(_ => _.Count() > 1);
    bool dataHaveNull() => this.Any(_ => _ == null);

    [InfoBox("Data have repeat id!", "dataHaveRepeatId", InfoMessageType = InfoMessageType.Error)]
    [InfoBox("Data have null element!", "dataHaveNull", InfoMessageType = InfoMessageType.Error)]
#endif
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

    public QueryableData this[string id] => this.QueryByID<QueryableData>(id);
    public QueryableData this[int index] => this.GetDataAt<QueryableData>(index);


#if (UNITY_EDITOR)

    [NodeTitle]
    string nodeTitle => string.IsNullOrEmpty(title) ? name : title;

    [NodeDescription]
    string nodeDescription => description;

#endif

}

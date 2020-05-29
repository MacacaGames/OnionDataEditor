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
public class DataGroup : QueryableData
{
    [SerializeField]
    string title;

    [SerializeField]
    string description;

#if (ODIN_INSPECTOR)

    bool dataHaveRepeatId() => GetData().GroupBy(_ => _.GetID()).Any(_ => _.Count() > 1);
    bool dataHaveNull() => GetData().Any(_ => _ == null);

    [InfoBox("Data have repeat id!", "dataHaveRepeatId", InfoMessageType = InfoMessageType.Error)]
    [InfoBox("Data have null element!", "dataHaveNull", InfoMessageType = InfoMessageType.Error)]
#endif
    [NodeElement]
    [SerializeField]
    protected QueryableData[] data = new QueryableData[0];

    public override string GetID()
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<IQueryableData> GetData()
    {
        return data;
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

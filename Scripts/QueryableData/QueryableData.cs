using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class QueryableData : ScriptableObject, IQueryableData
{
    public abstract string GetID();
}

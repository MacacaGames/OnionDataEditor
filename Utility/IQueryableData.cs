using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public interface IQueryableData
{
    string GetID();

    IEnumerable<IQueryableData> GetData();

    IEnumerable<T> GetData<T>() where T : IQueryableData;
    
}

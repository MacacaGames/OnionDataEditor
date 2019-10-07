using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public interface IQueryableData : IEnumerable, IEnumerable<IQueryableData>
{
    string GetID();
}

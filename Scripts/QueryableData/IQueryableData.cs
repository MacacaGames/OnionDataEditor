using System.Collections;
using System.Collections.Generic;

public interface IQueryableData : IEnumerable, IEnumerable<IQueryableData>
{
    string GetID();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace OnionCollections.DataEditor
{
    /// <summary>
    /// The target field or property will lock on GUI by default. You can unlock to edit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class LockableAttribute : PropertyAttribute
    {
        public LockableAttribute()
        {

        }

    }
}
﻿using System;

namespace OnionCollections.DataEditor
{
    /// <summary>
    /// Display custom description in data editor.
    /// Only can attach on String.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeDescriptionAttribute : Attribute
    {

    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{

    [System.Serializable]
    internal class ObjectNodeDefine
    {
        public string objectType;

        public string objectTypeFullName;


        public string titlePropertyName;
        public bool HasTitle => string.IsNullOrEmpty(titlePropertyName) == false;

        public string descriptionPropertyName;
        public bool HasDescription => string.IsNullOrEmpty(descriptionPropertyName) == false;

        public string iconPropertyName;
        public bool HasIcon => string.IsNullOrEmpty(iconPropertyName) == false;

        public string tagColorPorpertyName;
        public bool HasTagColor => string.IsNullOrEmpty(tagColorPorpertyName) == false;

        public string[] elementPropertyNames;
        public bool HasElement => elementPropertyNames != null && elementPropertyNames.Length > 0;



        public ObjectNodeDefine OverrideWith(ObjectNodeDefine overrideDefine)
        {
            if (objectTypeFullName != overrideDefine.objectTypeFullName)
                throw new System.Exception($"Override type is not equal. {objectTypeFullName} != {overrideDefine.objectTypeFullName}");


            return new ObjectNodeDefine
            {
                objectType = objectType,
                objectTypeFullName = objectTypeFullName,
                titlePropertyName = overrideDefine.HasTitle ? overrideDefine.titlePropertyName : titlePropertyName,
                descriptionPropertyName = overrideDefine.HasDescription ? overrideDefine.descriptionPropertyName : descriptionPropertyName,
                iconPropertyName = overrideDefine.HasIcon ? overrideDefine.iconPropertyName : iconPropertyName,
                elementPropertyNames = overrideDefine.HasElement ? overrideDefine.elementPropertyNames : elementPropertyNames,
                tagColorPorpertyName = overrideDefine.HasTagColor ? overrideDefine.tagColorPorpertyName : tagColorPorpertyName,

            };
        }
    }
}
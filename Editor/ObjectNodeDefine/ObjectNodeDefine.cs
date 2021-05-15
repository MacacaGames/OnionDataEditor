using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{

    [System.Serializable]
    internal class ObjectNodeDefine
    {
        public string objectType;


        public string titlePropertyName;
        public bool HasTitle => string.IsNullOrEmpty(titlePropertyName) == false;

        public string descriptionPropertyName;
        public bool HasDescription => string.IsNullOrEmpty(descriptionPropertyName) == false;

        public string iconPorpertyName;
        public bool HasIcon => string.IsNullOrEmpty(iconPorpertyName) == false;

        public string tagColorPorpertyName;
        public bool HasTagColor => string.IsNullOrEmpty(tagColorPorpertyName) == false;

        public string[] elementPropertyNames;
        public bool HasElement => elementPropertyNames != null && elementPropertyNames.Length > 0;



        public ObjectNodeDefine OverrideWith(ObjectNodeDefine overrideDefine)
        {
            if (objectType != overrideDefine.objectType)
                throw new System.Exception("Override type is not equal.");


            return new ObjectNodeDefine
            {
                objectType = objectType,
                titlePropertyName = overrideDefine.HasTitle ? overrideDefine.titlePropertyName : titlePropertyName,
                descriptionPropertyName = overrideDefine.HasDescription ? overrideDefine.descriptionPropertyName : descriptionPropertyName,
                iconPorpertyName = overrideDefine.HasIcon ? overrideDefine.iconPorpertyName : iconPorpertyName,
                elementPropertyNames = overrideDefine.HasElement ? overrideDefine.elementPropertyNames : elementPropertyNames,
                tagColorPorpertyName = overrideDefine.HasTagColor ? overrideDefine.tagColorPorpertyName : tagColorPorpertyName,

            };
        }
    }
}
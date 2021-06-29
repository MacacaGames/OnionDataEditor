using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace OnionCollections.DataEditor.Editor 
{
    public class OnionToggle : Toggle
    {
        public OnionToggle() : base()
        {
            string ussPath = $"{OnionDataEditor.Path}/Editor/UIComponents/OnionToggle/OnionToggle.uss";
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            this.styleSheets.Add(styleSheet);

            this.AddClass("onion-toggle");

            VisualElement boxRoot = this.Q(className: "unity-toggle__input");

            VisualElement box = new VisualElement()
                .AddTo(boxRoot)
                .AddClass("toggle-box")
                .AddClass("pointer");

            VisualElement inBox = new VisualElement()
                .AddTo(boxRoot)
                .AddClass("toggle-inner-box")
                .AddClass("pointer");


            this.RegisterValueChangedCallback(n =>
            {
                box.AddClassIf("active", n.newValue);
                inBox.AddClassIf("active", n.newValue);
            });
            

        }

    }
}
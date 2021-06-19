using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace OnionCollections.DataEditor.Editor
{
    internal static class UIElementsExtensions
    {
        #region Style

        public static T Show<T>(this T ve) where T: VisualElement
        {
            ve.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            return ve;
        }

        public static T Hide<T>(this T ve) where T : VisualElement
        {
            ve.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            return ve;
        }

        public static T Toggle<T>(this T ve) where T : VisualElement
        {
            if(ve.style.display.value == DisplayStyle.Flex)
            {
                ve.Hide();
            }
            else
            {
                ve.Show();
            }
            return ve;
        }

        public static T SetPadding<T>(this T ve, float padding) where T : VisualElement
        {
            var style = ve.style;

            style.paddingBottom = padding;
            style.paddingLeft = padding;
            style.paddingRight = padding;
            style.paddingTop = padding;

            return ve;
        }
        public static T SetMargin<T>(this T ve, float margin) where T : VisualElement
        {
            var style = ve.style;

            style.marginBottom = margin;
            style.marginLeft = margin;
            style.marginRight = margin;
            style.marginTop = margin;

            return ve;
        }

        public static T SetFlexGrow<T>(this T ve, float value) where T : VisualElement
        {
            ve.style.flexGrow = new StyleFloat(value);
            return ve;
        }

        public static T SetFlexDirection<T>(this T ve, FlexDirection flexDirection) where T : VisualElement
        {
            ve.style.flexDirection = new StyleEnum<FlexDirection>(flexDirection);
            return ve;
        }

        public static T SetWidth<T>(this T ve, float width) where T : VisualElement
        {
            ve.style.width = new StyleLength(width);
            return ve;
        }

        public static T SetHeight<T>(this T ve, float height) where T : VisualElement
        {
            ve.style.height = new StyleLength(height);
            return ve;
        }

        public static T SetName<T>(this T ve, string name) where T : VisualElement
        {
            ve.name = name;
            return ve;
        }

        #endregion

        #region Event
        public static T OnClicked<T>(this T ve, Action action) where T : Button
        {
            ve.clicked += action;
            return ve;
        }



        #endregion


        public static T AddTo<T>(this T ve, VisualElement parent) where T : VisualElement
        {
            parent.Add(ve);
            return ve;
        }


    }


}
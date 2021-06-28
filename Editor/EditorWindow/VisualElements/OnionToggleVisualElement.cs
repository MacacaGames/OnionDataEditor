using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace OnionCollections.DataEditor.Editor 
{
    public class OnionToggleVisualElement : Toggle
    {

        public OnionToggleVisualElement() : base()
        {
            this.Q("unity-checkmark").Hide();


            Label label = this.labelElement;
            label.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);

            VisualElement boxRoot = this.Q(className: "unity-toggle__input");
            boxRoot.SetFlexDirection(FlexDirection.Row);

            boxRoot.style.marginRight = new StyleLength(8F);
            boxRoot.style.alignItems = new StyleEnum<Align>(Align.Center);
            boxRoot.style.justifyContent = new StyleEnum<Justify>(Justify.FlexEnd);



            VisualElement box = new VisualElement()
                .AddTo(boxRoot)
                .SetWidth(24F)
                .SetHeight(8F)
                .SetBorderRadius(4F)
                .AddClass("pointer");

            VisualElement inBox = new VisualElement()
                .AddTo(boxRoot)
                .SetWidth(14F)
                .SetHeight(14F)
                .SetBorderRadius(999F)
                .AddClass("pointer");


            box.style.backgroundColor = new StyleColor(new Color(1, 1, 1, 0.1F));

            inBox.style.position = new StyleEnum<Position>(Position.Absolute);


            this.RegisterValueChangedCallback(n =>
            {
                inBox.style.right = new StyleLength(n.newValue ? 0F : 12F);

                Color inboxColor = n.newValue ? new Color(0.8F, 0.8F, 0.8F, 1F) : new Color(0.5F, 0.5F, 0.5F, 1F);
                inBox.style.backgroundColor = new StyleColor(inboxColor);
            });
            

        }

    }
}
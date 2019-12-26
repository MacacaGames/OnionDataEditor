﻿#if(UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace OnionCollections.DataEditor
{
    public class VisualElementResizer : MouseManipulator
    {
        private Vector2 m_Start;
        protected bool m_Active;

        public enum Direction { Horizontal, Vertical }

        readonly VisualElement m_ContainerA;
        readonly VisualElement m_ContainerB;
        readonly Direction m_Direction;


        public VisualElementResizer(VisualElement containerA, VisualElement containerB, VisualElement spliter, Direction direction)
        {
            m_ContainerA = containerA;
            m_ContainerB = containerB;
            m_Direction = direction;

            spliter.AddClass(direction == Direction.Horizontal ? "spliter-h" : "spliter-v");

            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            m_Active = false;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected void OnMouseDown(MouseDownEvent e)
        {
            if (m_Active)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (CanStartManipulation(e))
            {
                m_Start = e.localMousePosition;

                m_Active = true;
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        protected void OnMouseMove(MouseMoveEvent e)
        {
            if (!m_Active || !target.HasMouseCapture())
                return;

            Vector2 diff = e.localMousePosition - m_Start;

            var t = m_ContainerA;
            if (m_Direction == Direction.Horizontal)
            {
                int w = (int)t.style.width.value.value;

                float minWidth = m_ContainerA.resolvedStyle.minWidth.value;
                float maxWidth = m_ContainerA.resolvedStyle.maxWidth.value == 0 ? float.PositiveInfinity : m_ContainerA.resolvedStyle.maxWidth.value;

                t.style.width = Mathf.Clamp(w + diff.x, minWidth, maxWidth);
            }
            else if (m_Direction == Direction.Vertical)
            {
                int h = (int)t.style.height.value.value;

                float minHeight = m_ContainerA.resolvedStyle.minHeight.value;
                float maxHeight = m_ContainerA.resolvedStyle.maxHeight.value == 0 ? float.PositiveInfinity : m_ContainerA.resolvedStyle.maxHeight.value;

                t.style.height = Mathf.Clamp(h + diff.y, minHeight, maxHeight);
            }


            e.StopPropagation();
        }

        protected void OnMouseUp(MouseUpEvent e)
        {
            if (!m_Active || !target.HasMouseCapture() || !CanStopManipulation(e))
                return;

            m_Active = false;
            target.ReleaseMouse();
            e.StopPropagation();
        }
    }
}

#endif
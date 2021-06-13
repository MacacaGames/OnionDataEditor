using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{
    internal static class RectExtensions
    {

        #region Move

        public static Rect MoveRight(this Rect rect, float v)
        {
            rect.x += v;
            return rect;
        }
        public static Rect MoveLeft(this Rect rect, float v)
        {
            rect.x -= v;
            return rect;
        }
        public static Rect MoveDown(this Rect rect, float v)
        {
            rect.y += v;
            return rect;
        }
        public static Rect MoveUp(this Rect rect, float v)
        {
            rect.y -= v;
            return rect;
        }



        #endregion

        #region Extend

        public static Rect ExtendRight(this Rect rect, float v)
        {
            rect.width += v;
            return rect;
        }
        public static Rect ExtendLeft(this Rect rect, float v)
        {
            rect.x -= v;
            rect.width += v;
            return rect;
        }
        public static Rect ExtendDown(this Rect rect, float v)
        {
            rect.height += v;
            return rect;
        }
        public static Rect ExtendUp(this Rect rect, float v)
        {
            rect.y -= v;
            rect.height += v;
            return rect;
        }

        #endregion

        #region ExtendToFit

        public static Rect ExtendRightToFit(this Rect rect, float v)
        {
            return rect.SetWidth(v);
        }
        public static Rect ExtendLeftToFit(this Rect rect, float v)
        {
            return rect.ExtendLeft(-rect.width + v);
        }
        public static Rect ExtendDownToFit(this Rect rect, float v)
        {
            return rect.SetHeight(v);
        }
        public static Rect ExtendUpToFit(this Rect rect, float v)
        {
            return rect.ExtendUp(-rect.height + v);
        }

        #endregion


        #region Property

        public static Rect SetWidth(this Rect rect, float v)
        {
            rect.width = v;
            return rect;
        }
        public static Rect SetHeight(this Rect rect, float v)
        {
            rect.height = v;
            return rect;
        }
        public static Rect SetX(this Rect rect, float v)
        {
            rect.x = v;
            return rect;
        }
        public static Rect SetY(this Rect rect, float v)
        {
            rect.y = v;
            return rect;
        }

        public static Rect SetPosition(this Rect rect, float x, float y)
        {
            rect.x = x;
            rect.y = y;
            return rect;
        }

        public static Rect SetSize(this Rect rect, float w, float h)
        {
            rect.width = w;
            rect.height = h;
            return rect;
        }


        #endregion

        public static Rect[] HorizontalSplit(this Rect rect, int count, float space = 0F)
        {
            Rect[] result = new Rect[count];

            rect.width = (rect.width - space * (count - 1)) / count;


            for(int i = 0; i < count; i++)
            {
                result[i] = new Rect(rect);
                rect.x += rect.width + space;
            }


            return result;
        }



    }
}

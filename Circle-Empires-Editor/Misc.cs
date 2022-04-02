using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Circle_Empires_Editor
{
    public static class Misc
    {
        public static void Copy(Image _this, Image source)
        {
            _this.color = source.color;
            _this.material = source.material;
            _this.sprite = source.sprite;
            _this.fillMethod = source.fillMethod;
            _this.preserveAspect = source.preserveAspect;
            _this.fillCenter = source.fillCenter;
            _this.alphaHitTestMinimumThreshold = source.alphaHitTestMinimumThreshold;
            _this.useSpriteMesh = source.useSpriteMesh;
            _this.fillOrigin = source.fillOrigin;
            _this.fillClockwise = source.fillClockwise;
            _this.type = source.type;
            _this.overrideSprite = source.overrideSprite;
            _this.fillAmount = source.fillAmount;
        }
    }
}

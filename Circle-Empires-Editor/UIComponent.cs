using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Circle_Empires_Editor
{
    public class UIComponent
    {
        public class Text : UnityEngine.UI.Text
        {
            private RectTransform mRectTransform;

            public RectTransform RectTransform => mRectTransform;

            protected Text()
            {
                mRectTransform = gameObject.GetComponent<RectTransform>();
            }
        }

        public class Button : UnityEngine.UI.Button
        {
            private RectTransform mRectTransform;
            private Image mImage;
            private HotkeyButtonCaller mHotkeyButtonCaller;

            private Text mText;
            private UnityEngine.EventSystems.EventTrigger mEventTrigger;

            public RectTransform RectTransform => mRectTransform;
            public Image Image => mImage;
            public Text Text => mText;
            public UnityEngine.EventSystems.EventTrigger EventTrigger => mEventTrigger;

            private class HotkeyButtonCaller : MonoBehaviour
            {
                public KeyboardShortcut Hotkey;
                public UnityEngine.UI.Button Button;

                void Update()
                {
                    if (Hotkey.IsDown())
                        Button.onClick.Invoke();
                }
            }

            public KeyboardShortcut? Hotkey
            {
                get
                {
                    if (mHotkeyButtonCaller == null)
                        return null;
                    else
                        return mHotkeyButtonCaller.Hotkey;
                }
                set
                {
                    if (mHotkeyButtonCaller == null)
                    {
                        if (value == null)
                            return;
                        mHotkeyButtonCaller = gameObject.AddComponent<HotkeyButtonCaller>();
                        mHotkeyButtonCaller.Hotkey = (KeyboardShortcut)value;
                        mHotkeyButtonCaller.Button = this;
                    }
                    else
                    {
                        if (value == null)
                            Destroy(mHotkeyButtonCaller);
                        else
                            mHotkeyButtonCaller.Hotkey = (KeyboardShortcut)value;
                    }
                }
            }

            protected Button()
            {
                mRectTransform = gameObject.GetComponent<RectTransform>();
                if (mRectTransform == null)
                    mRectTransform = gameObject.AddComponent<RectTransform>();
                mImage = gameObject.AddComponent<Image>();
                mEventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

                mText = new GameObject("Text").AddComponent<Text>();
                mText.RectTransform.SetParent(gameObject.transform);
                mText.RectTransform.anchorMin = new Vector2(0, 0);
                mText.RectTransform.anchorMax = new Vector2(1, 1);

                mText.alignment = TextAnchor.MiddleCenter;
            }

            public void SetImage(Image image)
            {
                Misc.Copy(mImage, image);
            }
        }
    }
}

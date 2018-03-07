using System;
using System.Collections.Generic;
using static newHack.Aimbot;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct2D1;

namespace newHack
{
    public class Menu
    {
        Button[] buttons;
        int margin = 5;
        int verticalOffset = 0;
        int horizontalOffset = 0;
        int buttonHeight = 50;
        int buttonWidth = 200;

        public Menu(Button[] buttons, int verticalOffset, int horizontalOffset)
        {
            this.buttons = buttons;
            this.buttons[0].selected = true;
            this.verticalOffset = verticalOffset;
            this.horizontalOffset = horizontalOffset;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] is ToggleableButton)
                {
                    var button = (ToggleableButton)buttons[i];
                    if (button.active)
                    {
                        button.activate(button);
                    }
                }
            }
        }

        public void showMenu(WindowRenderTarget device, TextFormat font)
        {

            SolidColorBrush buttonBrush = new SolidColorBrush(device, new RawColor4(0f, 0f, 0f, 0.7f));
            SolidColorBrush textBrush = new SolidColorBrush(device, new RawColor4(1f, 1f, 1f, 1f));
            SolidColorBrush highlightBrush = new SolidColorBrush(device, new RawColor4(1f, 1f, 0f, 1f));
            var rect2 = new RoundedRectangle();
            rect2.Rect = new RawRectangleF(margin + horizontalOffset, margin + verticalOffset, buttonWidth + 2 * margin + horizontalOffset, buttonHeight * buttons.Length + 2 * margin + verticalOffset);
            rect2.RadiusX = 3;
            rect2.RadiusY = 3;
            device.FillRoundedRectangle(rect2, textBrush);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] is ToggleableButton)
                {
                    var thisButton = (ToggleableButton)buttons[i];
                    var rect = new RoundedRectangle();
                    rect.Rect = new RawRectangleF(2 * margin + horizontalOffset, i * buttonHeight + 2 * margin + verticalOffset, buttonWidth + margin + horizontalOffset, i * buttonHeight + margin + buttonHeight + verticalOffset);
                    rect.RadiusX = 2;
                    rect.RadiusY = 2;
                    device.FillRoundedRectangle(rect, buttonBrush);
                    var useBrush = buttons[i].selected ? highlightBrush : textBrush;
                    device.DrawText((thisButton.active ? "✓ " : "✗ ") + thisButton.text, font, new RawRectangleF(3 * margin + horizontalOffset, i * buttonHeight + buttonHeight / 2 + verticalOffset, 99999, 99999), useBrush);
                }
                else if (buttons[i] is SliderButton)
                {
                    var thisButton = (SliderButton)buttons[i];
                    var rect = new RoundedRectangle();
                    rect.Rect = new RawRectangleF(2 * margin + horizontalOffset, i * buttonHeight + 2 * margin + verticalOffset, buttonWidth + margin + horizontalOffset, i * buttonHeight + margin + buttonHeight + verticalOffset);

                    float left = 2 * margin + horizontalOffset;
                    float right = (buttonWidth + margin + horizontalOffset);
                    float length = right - left;
                    float newLength = length * thisButton.value;
                    right = newLength + left;

                    var coloredRect = new RoundedRectangle();
                    coloredRect.Rect = new RawRectangleF(left, i * buttonHeight + 2 * margin + verticalOffset, right, i * buttonHeight + margin + buttonHeight + verticalOffset);
                    
                    rect.RadiusX = 2;
                    rect.RadiusY = 2;
                    coloredRect.RadiusX = 2;
                    coloredRect.RadiusY = 2;
                    device.FillRoundedRectangle(rect, buttonBrush);
                    device.FillRoundedRectangle(coloredRect, new SolidColorBrush(device, new RawColor4(0,1,0,1)));
                    var useBrush = buttons[i].selected ? highlightBrush : textBrush;
                    var value = thisButton.abstractValue * thisButton.value;
                    thisButton.text = thisButton.text.Replace("%value%", (value).ToString());
                    device.DrawText(thisButton.text, font, new RawRectangleF(3 * margin + horizontalOffset, i * buttonHeight + buttonHeight / 2 + verticalOffset, 99999, 99999), useBrush);
                    thisButton.text = thisButton.text.Replace(value.ToString(), "%value%");
                }
            }
        }

        public void activateCurrent()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] is ToggleableButton)
                {
                    var thisButton = (ToggleableButton)buttons[i];
                    if (thisButton.selected)
                    {
                        thisButton.active = !thisButton.active;
                        thisButton.activate(thisButton);
                    }
                } else if (buttons[i] is SliderButton)
                {
                    var thisButton = (SliderButton)buttons[i];
                    if (thisButton.selected)
                    {
                        thisButton.value += 0.01f;
                        if (thisButton.value > 1f)
                        {
                            thisButton.value = 0.01f;
                        }
                        thisButton.replaceAction(thisButton.value);
                    }
                }
            }
        }

        public void shiftSelected(bool down)
        {
            if (down)
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i].selected)
                    {
                        if (i == buttons.Length - 1)
                        {
                            buttons[0].selected = true;
                        }
                        else
                            buttons[i + 1].selected = true;
                        buttons[i].selected = false;
                        break;
                    }
                }
            else
                for (int i = buttons.Length - 1; i >= 0; i--)
                {
                    if (buttons[i].selected)
                    {
                        if (i == 0)
                        {
                            buttons[buttons.Length - 1].selected = true;
                        }
                        else
                            buttons[i - 1].selected = true;
                        buttons[i].selected = false;
                        break;
                    }
                }
        }

    }

    public abstract class Button
    {
        public string text;
        public bool selected = false;
    }

    public class ToggleableButton : Button
    {
        public bool active = false;
        public Action<ToggleableButton> activate;

        public ToggleableButton(string text, Action<ToggleableButton> activate, bool active = false)
        {
            this.text = text;
            this.active = active;
            this.activate = activate;
        }
    }

    public class SliderButton : Button
    {
        public float value;
        public Action<float> replaceAction;
        public float abstractValue;

        public SliderButton(string text, Action<float> replaceAction, float value = 0f, float abstractValue = 1f)
        {
            this.text = text;
            this.replaceAction = replaceAction;
            this.value = value;
            this.abstractValue = abstractValue;
        }
    }
}

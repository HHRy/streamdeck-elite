﻿using BarRaider.SdTools;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;

namespace Elite
{
    public abstract class EliteBase : PluginBase
    {

        protected bool InputRunning;
        protected bool ForceStop = false;

        protected EliteBase(SDConnection connection, InitialPayload payload) : base(connection, payload) { }


        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        protected async void SendInput(string inputText)
        {
            InputRunning = true;
            await Task.Run(() =>
            {
                var text = inputText;

                for (var idx = 0; idx < text.Length && !ForceStop; idx++)
                {
                    var macro = CommandTools.ExtractMacro(text, idx);
                    idx += macro.Length - 1;
                    macro = macro.Substring(1, macro.Length - 2);

                    HandleMacro(macro);
                }
            });
            InputRunning = false;
        }

        private void HandleMacro(string macro)
        {
            var keyStrokes = CommandTools.ExtractKeyStrokes(macro);

            // Actually initiate the keystrokes
            if (keyStrokes.Count > 0)
            {
                var iis = new InputSimulator();
                var keyCode = keyStrokes.Last();
                keyStrokes.Remove(keyCode);

                if (keyStrokes.Count > 0)
                {
                    iis.Keyboard.ModifiedKeyStroke(keyStrokes.Select(ks => ks).ToArray(), keyCode);
                }
                else // Single Keycode
                {
                    iis.Keyboard.KeyPress(keyCode);
                }
            }
        }

        protected void SendKeypress(StandardBindingInfo keyInfo, int repeatCount = 1)
        {
            var inputText = "";

            if (keyInfo.Primary.Device == "Keyboard")
            {
                inputText =
                    "{" + keyInfo.Primary.Key.Replace("Key_", "DIK") + "}";
            }
            else if (keyInfo.Secondary.Device == "Keyboard")
            {
                inputText =
                    "{" + keyInfo.Secondary.Key.Replace("Key_", "DIK") + "}";
                foreach (var m in keyInfo.Secondary.Modifier)
                {
                    if (m.Device == "Keyboard")
                    {
                        inputText =
                            "{" + m.Key.Replace("Key_", "DIK") +
                            "}" + inputText;
                    }
                }
            }

            if (!string.IsNullOrEmpty(inputText))
            {

                inputText = inputText.Replace("_", "")
                    .Replace("Backspace", "BACK")
                    .Replace("UpArrow", "UP")
                    .Replace("DownArrow", "DOWN")
                    .Replace("LeftArrow", "LEFT")
                    .Replace("RightArrow", "RIGHT")
                    .Replace("LeftAlt", "LMENU")
                    .Replace("RightAlt", "RMENU")
                    .Replace("RightControl", "RCONTROL")
                    .Replace("LeftControl", "LCONTROL")
                    .Replace("RightShift", "RSHIFT")
                    .Replace("LeftShift", "LSHIFT");

                //Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{inputText}");

                for (var i = 0; i < repeatCount; i++)
                {
                    if (repeatCount > 1 && i > 0)
                    {
                        Thread.Sleep(20);
                    } 
                    SendInput("{" + inputText + "}");

                }

                // keyboard test page : https://w3c.github.io/uievents/tools/key-event-viewer.html
            }

        }

    }
}

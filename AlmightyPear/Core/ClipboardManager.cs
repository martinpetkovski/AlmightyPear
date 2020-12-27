using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace Core
{
    public class ClipboardManager
    {
        private static string _prevClipboardText;
        private static async Task ClipboardChanged(string inputValue)
        {
            await Task.Factory.StartNew(() =>
            {
                string a = GetClipboardText();
                while (inputValue == a)
                {
                    Thread.Sleep(10);
                    a = GetClipboardText();
                }
            });
        }

        public static async Task<string> ExecuteCopyAsync()
        {
            InputSimulator inputSim = new InputSimulator();
            string clipboardText = GetClipboardText();
            inputSim.Keyboard.KeyUp(VirtualKeyCode.LWIN);
            inputSim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

            if (clipboardText != "" && clipboardText != _prevClipboardText)
            {
                await ClipboardChanged(clipboardText);
            }

            _prevClipboardText = GetClipboardText();

            return _prevClipboardText;
        }

        public static string GetClipboardText()
        {

            string clipboardData = null;
            Exception threadEx = null;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        clipboardData = Clipboard.GetText(TextDataFormat.Text);
                    }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            return clipboardData;
        }

        public static void ClearClipboard()
        {
            Exception threadEx = null;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        Clipboard.Clear();
                    }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
        }

        public static void CopyToClipboard(string text)
        {
            Exception threadEx = null;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        Clipboard.SetText(text);
                    }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
        }
    }
}

using CitizenFX.Core;
using System;
using System.Text.RegularExpressions;

namespace MpChat.Server
{
    internal class Tools
    {
        #region Safe string

        public static string SafeString(string message, bool half = false)
        {
            if (!string.IsNullOrEmpty(message))
            {
                var safeName = message.Replace("^", "").Replace("~", "");
                safeName = Regex.Replace(safeName, @"[^\u0000-\u007F]+", string.Empty);
                safeName = safeName.Trim(['.', ',', ' ', '?']);
                if (!half)
                    safeName = safeName.Trim(['<', '!', '@', '>']);

                if (string.IsNullOrEmpty(safeName))
                    safeName = "Invalid Name";

                safeName = safeName.Replace("^1", "");
                safeName = safeName.Replace("^2", "");
                safeName = safeName.Replace("^3", "");
                safeName = safeName.Replace("^4", "");
                safeName = safeName.Replace("^5", "");
                safeName = safeName.Replace("^6", "");
                safeName = safeName.Replace("^7", "");
                safeName = safeName.Replace("^8", "");
                safeName = safeName.Replace("^9", "");
                safeName = safeName.Replace("^0", "");
                safeName = safeName.Replace("^*", "");
                safeName = safeName.Replace("^_", "");
                safeName = safeName.Replace("^~", "");
                safeName = safeName.Replace("^=", "");
                safeName = safeName.Replace("^m", "");
                safeName = safeName.Replace("^l", "");
                safeName = safeName.Replace("^b", "");
                safeName = safeName.Replace("^p", "");
                safeName = safeName.Replace("^g", "");
                safeName = safeName.Replace("^y", "");
                safeName = safeName.Replace("^t", "");
                safeName = safeName.Replace("^k", "");
                safeName = safeName.Replace("^c", "");
                safeName = safeName.Replace("\\", "");
                safeName = safeName.Replace("~b~", "");
                safeName = safeName.Replace("~c~", "");
                safeName = safeName.Replace("~d~", "");
                safeName = safeName.Replace("~f~", "");
                safeName = safeName.Replace("~g~", "");
                safeName = safeName.Replace("~h~", "");
                safeName = safeName.Replace("~i~", "");
                safeName = safeName.Replace("~m~", "");
                safeName = safeName.Replace("~f~", "");
                safeName = safeName.Replace("~p~", "");
                safeName = safeName.Replace("~q~", "");
                safeName = safeName.Replace("~r~", "");
                safeName = safeName.Replace("~s~", "");
                safeName = safeName.Replace("~t~", "");
                safeName = safeName.Replace("~w~", "");
                safeName = safeName.Replace("~y~", "");

                return safeName;
            }

            return "Invalid Name";
        }

        #endregion
    }
}

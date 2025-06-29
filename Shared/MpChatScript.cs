using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

#if CLIENT

using CitizenFX.Core.UI;
using Mpchat;
using MpChat.Client;
using Newtonsoft.Json;

#endif

#if SERVER

using Mpchat;
using MpChat.Server;

#endif

namespace MpChat
{
    public class MpChatScript
    {
        #region Fields

#if CLIENT

        /// <summary>
        /// To indicate if the scaleform is ready.
        /// </summary>
        private bool _scaleformIsReady;

        /// <summary>
        /// The main configuration for the chat.
        /// </summary>
        private MainConf _mainConf;

        /// <summary>
        /// MP chat handler.
        /// </summary>
        private MpChatScriptHandler _mpChat;

        /// <summary>
        /// The time when the last message was sent.
        /// </summary>
        private int _lastMessageTime = 0;

        /// <summary>
        /// The tick time when the user just pasted something from the clipboard.
        /// </summary>
        private int _justPastedTime = 0;

        /// <summary>
        /// Whether the NUI is focused or not.
        /// </summary>
        private bool _isNuiFocused = false;

        /// <summary>
        /// The default position for the input box.
        /// </summary>
        private PointF _defaultInputPosition = new(452, 25);
        
        /// <summary>
        /// The default position for the feed box.
        /// </summary>
        private PointF _defaultFeedPosition = new(412, 325);

        /// <summary>
        /// The position offset for the input box.
        /// </summary>
        private PointF? _inputPositionOffset = null;

        /// <summary>
        /// The position offset for the feed box.
        /// </summary>
        private PointF? _feedPositionOffset = null;


#endif

#if SERVER

        public List<string> BadWords = [];

#endif

        #endregion

        #region Constructor

#if CLIENT

        public MpChatScript()
        {
            // Load the configs
            bool success = LoadConfigs();
            if (!success)
                return;

            // Add event handlers
            Main.Instance.AddEventHandler("swfLiveEditor:scaleformUpdated", ScaleformUpdated);
            Main.Instance.AddEventHandler("onResourceStop", OnResourceStop);
            Main.Instance.AddEventHandler("mpchat:addMessage:Client", AddMessage);

            // Add exports
            Main.Instance.ExportList.Add("setInputPositionOffset", SetInputPositionOffset);
            Main.Instance.ExportList.Add("setFeedPositionOffset", SetFeedPositionOffset);

            // Initialize scaleform
            ScaleformInit();
        }

#endif

#if SERVER

        public MpChatScript()
        {
            // Add event handlers
            Main.Instance.AddEventHandler("playerDropped", PlayerDropped);
            Main.Instance.AddEventHandler("mpchat:addMessage:Server", AddMessage);

            // Load the configs
            LoadConfigs();
        }

#endif

        #endregion

        #region Events

#if CLIENT

        #region Scaleform updated

        private void ScaleformUpdated(string name) => ScaleformInit(name);

        #endregion

        #region On resource stop

        private async void OnResourceStop(string resourceName)
        {
            if (Main.Instance.ResourceName != resourceName) return;
            _mpChat?.Dispose();
        }

        #endregion

        #region Add message

        private async void AddMessage(string name, string message, int scope, bool team, int color, bool isCensored)
        {
            _mpChat.AddMessage(name, message, (MpChatScriptHandler.ChatScope)scope, team, (MpChatScriptHandler.HudColor)color, isCensored);
            _mpChat.Show();
        }

        #endregion

        #region Set input position offset

        private void SetInputPositionOffset(float x, float y) => _inputPositionOffset = new PointF(x, y);

        #endregion

        #region Set feed position offset

        private void SetFeedPositionOffset(float x, float y) => _feedPositionOffset = new PointF(x, y);

        #endregion

#endif

#if SERVER

        #region Player dropped

        private async void PlayerDropped([FromSource] Player source)
        {
            $"Player dropped {source.Name}".Log();
        }

        #endregion

        #region Add message

        private void AddMessage([FromSource] Player source, string message, int scope, bool team, int color)
        {
            if (string.IsNullOrEmpty(message))
                return;

            // Handle message censorship
            bool isCensored = SanatizeMessage(ref message);

            BaseScript.TriggerClientEvent("mpchat:addMessage:Client", Tools.SafeString(source.Name), message, scope, team, color, isCensored);
        }

        #endregion

#endif

        #endregion

        #region Ticks

#if CLIENT

        #region Scaleform thread

        private async Task ScaleformThread()
        {
            // Don't do anything if the scaleform isn't ready
            if (!CanInteractWithScaleform(false))
                return;

            // Update the scaleform
            _mpChat.Update();

            // Update the positions of the input and feed boxes if they are not at the default positions
            if (_mainConf.InputPosition != _defaultInputPosition)
                _mpChat.SetInputPosition(_inputPositionOffset != null ? new PointF(_mainConf.InputPosition.X + ((PointF)_inputPositionOffset).X, _mainConf.InputPosition.Y + ((PointF)_inputPositionOffset).Y) : _mainConf.InputPosition);
            if (_mainConf.FeedPosition != _defaultFeedPosition)
                _mpChat.SetFeedPosition(_feedPositionOffset != null ? new PointF(_mainConf.FeedPosition.X + ((PointF)_feedPositionOffset).X, _mainConf.FeedPosition.Y + ((PointF)_feedPositionOffset).Y) : _mainConf.FeedPosition);

            await Task.FromResult(0);
        }

        #endregion

        #region Input handler

        private async Task InputHandler()
        {
            // Don't do anything if the scaleform isn't ready
            if (!CanInteractWithScaleform(false))
                return;

            // Don't interact if on gamepad
            if (Game.CurrentInputMode != InputMode.MouseAndKeyboard)
                return;

            // Show the chat
            if (Game.IsControlJustReleased((int)InputMode.MouseAndKeyboard, Control.MpTextChatAll))
            {
                if (!_mpChat.IsTyping())
                    _mpChat.StartTyping();
            }

            // Don't continue if the chat isn't visible
            if (_mpChat.ChatVisibility != MpChatScriptHandler.ChatVisibilities.Default && _mpChat.ChatVisibility != MpChatScriptHandler.ChatVisibilities.Typing)
                return;

            // Scroll history (broken currently)
            if (Game.IsControlJustPressed((int)InputMode.MouseAndKeyboard, Control.FrontendRt))
            {
                _mpChat.PageUp();
            }
            else if (Game.IsControlJustPressed((int)InputMode.MouseAndKeyboard, Control.FrontendLt))
            {
                _mpChat.PageDown();
            }

            if (_mpChat.IsTyping())
            {
                Game.DisableAllControlsThisFrame((int)InputMode.MouseAndKeyboard);
                if (Game.IsControlJustReleased((int)InputMode.MouseAndKeyboard, Control.FrontendPauseAlternate))
                {
                    _mpChat.StopTyping();
                }
                else if (Game.IsControlJustReleased((int)InputMode.MouseAndKeyboard, Control.FrontendEndscreenAccept))
                {
                    string text = "";
                    int calculatedIterations = (int)Math.Ceiling((double)_mpChat.GetCharLimit() / 64);
                    for (var i = 0; i < calculatedIterations; i++)
                    {
                        var result = await _mpChat.Scaleform.GetResult<string>($"GET_TEXT", i);
                        text += result;
                    }

                    // Make sure the text is not empty
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        _mpChat.StopTyping();
                        return;
                    }

                    // Check if we need to delay the next message
                    if (_mainConf.NextMessageDelayMs > 0)
                    {
                        if (Game.GameTime - _lastMessageTime < _mainConf.NextMessageDelayMs)
                        {
                            var waitTime = Math.Ceiling((_mainConf.NextMessageDelayMs - (Game.GameTime - _lastMessageTime)) / 1000);
                            $"You need to wait {waitTime} second{(waitTime == 1 ? "" : "s")} before sending the next message".Help();
                            return;
                        }
                    }

                    API.ClearAllHelpMessages();
                    _lastMessageTime = Game.GameTime;
                    BaseScript.TriggerServerEvent("mpchat:addMessage:Server", text, (int)MpChatScriptHandler.ChatScope.None, false, (int)MpChatScriptHandler.HudColor.HUD_COLOUR_BLUELIGHT);
                    _mpChat.StopTyping();
                }
                else if (Game.IsControlJustReleased((int)InputMode.MouseAndKeyboard, Control.FrontendUp))
                {
                    _mpChat.InputMessageHistoryUp();
                }
                else if (Game.IsControlJustReleased((int)InputMode.MouseAndKeyboard, Control.FrontendDown))
                {
                    _mpChat.InputMessageHistoryDown();
                }

                #region WIP pasting fromm nui

                if (_mainConf.EnableWipPasting)
                {
                    if (Game.IsControlPressed((int)InputMode.MouseAndKeyboard, Control.ReplayCtrl) && !_isNuiFocused)
                    {
                        _justPastedTime = Game.GameTime;
                        API.SetNuiFocus(true, true);
                        API.SetNuiFocusKeepInput(true);
                        API.SendNuiMessage(Json.Stringify(new
                        {
                            type = "show"
                        }));
                        _isNuiFocused = true;
                    }

                    if (Game.IsControlPressed((int)InputMode.MouseAndKeyboard, Control.ReplayCtrl) && Game.IsControlJustReleased((int)InputMode.MouseAndKeyboard, Control.NextCamera))
                    {
                        API.SendNuiMessage(Json.Stringify(new
                        {
                            type = "getClipboard"
                        }));
                        "Requesting clipboard data".Log();
                    }

                    if (Game.GameTime - _justPastedTime > 2000 && _isNuiFocused)
                    {
                        _isNuiFocused = false;
                        API.SetNuiFocus(false, false);
                        API.SetNuiFocusKeepInput(false);
                        "Clipboard data request timed out".Log();
                    }
                }

                #endregion
            }

            await Task.FromResult(0);
        }

        #endregion

#endif

        #endregion

        #region Tools

#if CLIENT

        #region Load configs

        private bool LoadConfigs()
        {
            var mainConf = Json.Parse<MainConf>(API.LoadResourceFile(Main.Instance.ResourceName, "configs/main.json"));
            if (mainConf == null)
            {
                "Main config has an error, please check the config syntax.".Error();
                return false;
            }
            _mainConf = mainConf;
            return true;
        }

        #endregion

        #region Scaleform init

        private async void ScaleformInit(string gfx = null)
        {
            if (!string.IsNullOrEmpty(gfx) && !gfx.StartsWith("multiplayer_chat"))
            {
                "[MpChat] Invalid scaleform name".Error();
                return;
            }

            // Not ready yet
            _scaleformIsReady = false;

#if DEBUG

            // Create a TaskCompletionSource to await the event completion
            var tc = new TaskCompletionSource<string>();

            // This is only for live editing from the scaleform editor (which is private)
            if (API.GetResourceState("swfLiveEditor") == "started")
            {
                // Create a TaskCompletionSource to await the event completion
                tc = new TaskCompletionSource<string>();

                // Get the correct scaleform from the server
                BaseScript.TriggerServerEvent("swfLiveEditor:getCorrectScaleform", "multiplayer_chat", new Action<string>(tc.SetResult));

                // Wait until the event is completed
                gfx = await tc.Task;
            }
            else
                gfx ??= "multiplayer_chat";

#else

            // Use the default scaleform if not specified
            gfx ??= "multiplayer_chat";

#endif

            // Request scaleform
            _mpChat = new MpChatScriptHandler(gfx);

            // Wait for the scaleform to load
            int timeout = 7000;
            int start = Game.GameTime;
            while (!_mpChat.Scaleform.IsLoaded && Game.GameTime - start < timeout)
            {
                //"Waiting for scaleform to load".Log();
                await BaseScript.Delay(0);
            }
            if (!_mpChat.Scaleform.IsLoaded)
            {
                "Scaleform failed to load".Log();
                return;
            }

            // Apply chat settings
            ApplyChatSettings();

            // Attach the ticks
            Main.Instance.AttachTick(ScaleformThread);
            Main.Instance.AttachTick(InputHandler);

            // Scaleform is ready
            _scaleformIsReady = true;

            "Scaleform is ready".Log();
        }

        #endregion

        #region Apply scaleform settings

        private void ApplyChatSettings()
        {
            _mpChat.Reset();
            _mpChat.SetFocus(MpChatScriptHandler.ChatVisibilities.Hidden);
            _mpChat.SetCharLimit(_mainConf.InputCharLimit);
            _mpChat.SetFeedLineAmount(_mainConf.FeedLineAmount);
            _mpChat.SetDuration(_mainConf.FeedHideFadeMs);
            if (_mainConf.EnableWipPasting)
            {
                API.RegisterNuiCallbackType("getClipboard");
                Main.Instance.AddEventHandler("__cfx_nui:getClipboard", (dynamic cb) =>
                {
                    API.SetNuiFocus(false, false);
                    $"Data: {cb.text}".Log();
                    _mpChat.AddText(cb.text);
                });
            }
        }

        #endregion

        #region Can intereact with scaleform

        public bool CanInteractWithScaleform(bool notify)
        {
            bool state = _scaleformIsReady && !API.IsPauseMenuActive() && Screen.Fading.IsFadedIn && !API.IsPlayerSwitchInProgress();
            if (!state && notify)
                "You can't interact with the scaleform right now".Error();
            return state;
        }

        #endregion

#endif

#if SERVER

        #region Load configs

        private void LoadConfigs()
        {
            var badwords = Json.Parse<List<string>>(API.LoadResourceFile(Main.Instance.ResourceName, "configs/badwords.json"));
            if (badwords == null)
                "The badwords.json has an error, please check if there's a syntax ".Error();
            else
                BadWords = badwords;
        }

        #endregion

        #region Sanatize message

        /// <summary>
        /// Yes AI helped with this one lol, returns a sanatized message and returns whether the message was or not censored.
        /// </summary>
        /// <param name="message"></param>
        private bool SanatizeMessage(ref string message)
        {
            if (BadWords == null || BadWords.Count == 0 || string.IsNullOrWhiteSpace(message))
                return false;

            var refMsg = message;
            foreach (var badWord in BadWords)
            {
                var replacement = new string('*', badWord.Length);

                // Censor exact matches
                message = System.Text.RegularExpressions.Regex.Replace(
                    message,
                    $@"\b{System.Text.RegularExpressions.Regex.Escape(badWord)}\b",
                    replacement,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Censor attempts to bypass by adding spaces or special characters between letters
                var bypassPattern = string.Join(@"\W*", badWord.ToCharArray().Select(c => System.Text.RegularExpressions.Regex.Escape(c.ToString())));
                message = System.Text.RegularExpressions.Regex.Replace(
                    message,
                    bypassPattern,
                    replacement,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Censor attempts to bypass by adding extra characters within the bad word
                var extraCharPattern = string.Join(@".*", badWord.ToCharArray().Select(c => System.Text.RegularExpressions.Regex.Escape(c.ToString())));
                message = System.Text.RegularExpressions.Regex.Replace(
                    message,
                    extraCharPattern,
                    replacement,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            return refMsg != message;
        }

        #endregion

#endif

        #endregion

        #region Classes

#if CLIENT

        #region Main config

        private class MainConf
        {
            [JsonProperty("inputCharLimit")]
            public int InputCharLimit { get; set; }

            [JsonProperty("feedLineAmount")]
            public int FeedLineAmount { get; set; }

            [JsonProperty("enableWipPasting")]
            public bool EnableWipPasting { get; set; }

            [JsonProperty("nextMessageDelayMs")]
            public int NextMessageDelayMs { get; set; }

            [JsonProperty("feedHideFadeMs")]
            public int FeedHideFadeMs { get; set; }

            [JsonProperty("inputPosition")]
            public PointF InputPosition { get; set; }

            [JsonProperty("feedPosition")]
            public PointF FeedPosition { get; set; }
        }

        #endregion

#endif

        #endregion
    }
}

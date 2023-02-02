// Adapted from https://github.com/MoltenMods/Unify
/*
MIT License
Copyright (c) 2021 Daemon
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using Il2CppInterop.Runtime.InteropTypes;
using System.Linq.Expressions;

namespace ServerAdd
{
    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.Open))]
    public static class RegionMenuOpenPatch
    {
        private static TextBoxTMP ipField;
        private static TextBoxTMP portField;
        private static TextBoxTMP ServerNameField;
        private static GameObject isHttpsButton;
        private static GameObject isDNSButton;
        private static GameObject AddButton;
        private static GameObject ClearButton;
        private static GameObject ClearAllButton;
        private static Vector3 pos = new Vector3(3f, 2f, -100f);
        private static bool isOpen;
        private static ServerInfo[] ServerInfo;
        private static IRegionInfo[] regions;
        private static ServerManager serverManager = DestroyableSingleton<ServerManager>.Instance;
        public static void Postfix(RegionMenu __instance)
        {
            isOpen = __instance.isActiveAndEnabled;
            var template = DestroyableSingleton<JoinGameButton>.Instance;
            var joinGameButtons = GameObject.FindObjectsOfType<JoinGameButton>();
            foreach (var t in joinGameButtons)
            {  // The correct button has a background, the other 2 dont
                if (t.GameIdText != null && t.GameIdText.Background != null)
                {
                    template = t;
                    break;
                }
            }
            if (template == null || template.GameIdText == null) return;

            if (ipField == null || ipField.gameObject == null)
            {
                ipField = UnityEngine.Object.Instantiate(template.GameIdText, __instance.transform);
                ipField.gameObject.name = "IpTextBox";
                var arrow = ipField.transform.FindChild("arrowEnter");
                if (arrow == null || arrow.gameObject == null) return;
                UnityEngine.Object.DestroyImmediate(arrow.gameObject);

                ipField.transform.localPosition = pos - new Vector3(0f, 1f, 0f);
                ipField.characterLimit = 30;
                ipField.AllowSymbols = true;
                ipField.ForceUppercase = false;
                ipField.SetText(ServerAdd.Ip.Value.ToString());
                __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) =>
                {
                    ipField.outputText.SetText(ServerAdd.Ip.Value.ToString());
                    ipField.SetText(ServerAdd.Ip.Value.ToString());
                })));

                ipField.ClearOnFocus = false;
                ipField.OnEnter = ipField.OnChange = new Button.ButtonClickedEvent();
                ipField.OnFocusLost = new Button.ButtonClickedEvent();
                ipField.OnChange.AddListener((UnityAction)onEnterOrIpChange);
                ipField.gameObject.SetActive(isOpen);

                void onEnterOrIpChange()
                {
                    ServerAdd.Ip.Value = ipField.text;
                }
            }

            if (ServerNameField == null || ServerNameField.gameObject == null)
            {
                ServerNameField = UnityEngine.Object.Instantiate(template.GameIdText, __instance.transform);
                ServerNameField.gameObject.name = "ServerNameTextBox";
                var arrow = ipField.transform.FindChild("arrowEnter");
                if (arrow == null || arrow.gameObject == null) return;
                UnityEngine.Object.DestroyImmediate(arrow.gameObject);

                ServerNameField.transform.localPosition = pos - new Vector3(0f, 0f, 0f);
                ServerNameField.characterLimit = 30;
                ServerNameField.AllowSymbols = true;
                ServerNameField.ForceUppercase = false;
                ServerNameField.SetText(ServerAdd.ServerName.Value);
                __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) =>
                {
                    ServerNameField.outputText.SetText(ServerAdd.ServerName.Value);
                    ServerNameField.SetText(ServerAdd.ServerName.Value);
                })));

                ipField.ClearOnFocus = false;
                ipField.OnEnter = ipField.OnChange = new Button.ButtonClickedEvent();
                ipField.OnFocusLost = new Button.ButtonClickedEvent();
                ipField.OnChange.AddListener((UnityAction)onEnterOrIpChange);
                ipField.gameObject.SetActive(isOpen);

                void onEnterOrIpChange()
                {
                    ServerAdd.ServerName.Value = ipField.text;
                }
            }

            if (portField == null || portField.gameObject == null)
            {

                portField = UnityEngine.Object.Instantiate(template.GameIdText, __instance.transform);
                portField.gameObject.name = "PortTextBox";
                var arrow = portField.transform.FindChild("arrowEnter");
                UnityEngine.Object.DestroyImmediate(arrow.gameObject);

                portField.transform.localPosition = pos - new Vector3(0f, 2f, 0f);
                portField.characterLimit = 5;
                portField.SetText(ServerAdd.Port.Value.ToString());
                __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) =>
                {
                    portField.outputText.SetText(ServerAdd.Port.Value.ToString());
                    portField.SetText(ServerAdd.Port.Value.ToString());
                })));


                portField.ClearOnFocus = false;
                portField.OnEnter = portField.OnChange = new Button.ButtonClickedEvent();
                portField.OnFocusLost = new Button.ButtonClickedEvent();
                portField.OnChange.AddListener((UnityAction)onEnterOrPortFieldChange);
                portField.gameObject.SetActive(isOpen);

                void onEnterOrPortFieldChange()
                {
                    ushort port = 0;
                    if (ushort.TryParse(portField.text, out port))
                    {
                        ServerAdd.Port.Value = port;
                        portField.outputText.color = Color.white;
                    }
                    else
                    {
                        portField.outputText.color = Color.red;
                    }
                }
            }

            if (isHttpsButton == null || isHttpsButton.gameObject == null)
            {
                GameObject tf = GameObject.Find("NormalMenu/BackButton");

                isHttpsButton = UnityEngine.Object.Instantiate(tf, __instance.transform);
                isHttpsButton.name = "isHttpsButton";

                var text = isHttpsButton.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                PassiveButton isHttpsPassiveButton = isHttpsButton.GetComponent<PassiveButton>();
                SpriteRenderer isHttpsButtonSprite = isHttpsButton.GetComponent<SpriteRenderer>();
                isHttpsPassiveButton.OnClick = new();
                isHttpsPassiveButton.OnClick.AddListener((UnityAction)act);
                Color isHttpsColor = ServerAdd.isHttps.Value ? Palette.AcceptedGreen : Palette.White;
                isHttpsPassiveButton.OnMouseOut.AddListener((Action)(() => isHttpsButtonSprite.color = isHttpsColor));
                text.SetText("isHttps");
                __instance.StartCoroutine(Effects.Lerp(0.01f, new Action<float>((p) => text.SetText("isHttps"))));
                isHttpsButton.gameObject.SetActive(isOpen);

                void act()
                {
                    ServerAdd.isHttps.Value = ServerAdd.isDNS.Value ? false : !ServerAdd.isHttps.Value;
                    isHttpsButton.UpdateButtonColor(ServerAdd.isHttps.Value);
                }
            }

            if (isDNSButton == null || isDNSButton.gameObject == null)
            {
                GameObject tf = GameObject.Find("NormalMenu/BackButton");

                isDNSButton = UnityEngine.Object.Instantiate(tf, __instance.transform);
                isDNSButton.name = "isDNSButton";

                var text = isDNSButton.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                PassiveButton isDNSPassiveButton = isDNSButton.GetComponent<PassiveButton>();
                SpriteRenderer isDNSButtonSprite = isDNSButton.GetComponent<SpriteRenderer>();
                isDNSPassiveButton.OnClick = new();
                isDNSPassiveButton.OnClick.AddListener((UnityAction)DNSact);
                Color isDNSColor = ServerAdd.isDNS.Value ? Palette.AcceptedGreen : Palette.White;
                isDNSPassiveButton.OnMouseOut.AddListener((Action)(() => isDNSButtonSprite.color = isDNSColor));
                text.SetText("isDNS");
                __instance.StartCoroutine(Effects.Lerp(0.01f, new Action<float>((p) => text.SetText("isDNS"))));
                isDNSButton.gameObject.SetActive(isOpen);

                void DNSact()
                {
                    ServerAdd.isDNS.Value = !ServerAdd.isDNS.Value;
                    isDNSButton.UpdateButtonColor(ServerAdd.isDNS.Value);
                }
            }

            isHttpsButton.transform.position = pos - new Vector3(0f, 3f, 0f);
            isDNSButton.transform.position = pos - new Vector3(-1f, 3f, 0f);
        }

        // This is part of the Mini.RegionInstaller, Licensed under GPLv3
        // file="RegionInstallPlugin.cs" company="miniduikboot">

        public static void UpdateRegions()
        {
            string serverIp = (ServerAdd.isDNS.Value ? "" : (ServerAdd.isHttps.Value ? "https://" : "http://")) + ServerAdd.Ip.Value;
            ServerInfo serverInfo = new ServerInfo(ServerAdd.ServerName.Value, serverIp, ServerAdd.Port.Value, false);
            ServerInfo = new ServerInfo[] { serverInfo };
        }

        public static void UpdateRegionInfo()
        {
            foreach (IRegionInfo region in regions)
            {
                serverManager.AddOrUpdateRegion(region);
            }
        }

        public static void UpdateButtonColor(this GameObject objet, bool open)
        {
            var PassiveButton = objet.GetComponent<PassiveButton>();
            var ButtonSprite = objet.GetComponent<SpriteRenderer>();
            Color color = open ? Palette.AcceptedGreen : Palette.White;
            PassiveButton.OnMouseOut.AddListener((Action)(() => ButtonSprite.color = color));
        }

        private static class CastHelper<T> where T : Il2CppObjectBase
        {
            public static Func<IntPtr, T> Cast;
            static CastHelper()
            {
                var constructor = typeof(T).GetConstructor(new[] { typeof(IntPtr) });
                var ptr = Expression.Parameter(typeof(IntPtr));
                var create = Expression.New(constructor!, ptr);
                var lambda = Expression.Lambda<Func<IntPtr, T>>(create, ptr);
                Cast = lambda.Compile();
            }
        }

        public static T CastFast<T>(this Il2CppObjectBase obj) where T : Il2CppObjectBase
        {
            if (obj is T casted) return casted;
            return CastHelper<T>.Cast(obj.Pointer);
        }
    }
}

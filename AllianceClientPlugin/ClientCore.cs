using HarmonyLib;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SpaceEngineers.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VRage.GameServices;
using VRage.Plugins;
using VRageMath;

namespace AllianceClientPlugin
{
    public class ClientCore : IDisposable, IPlugin
    {
        public void Dispose()
        {
        }

        public static Boolean Ready = false;
        public void Init(object gameInstance)
        {
            var harmony = new Harmony("Crunch.AlliancePatch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            //   MyAPIGateway.Multiplayer.RegisterMessageHandler(8544, ReceivedPacket);

        }

        public static String title = "";
        public static Color AllianceColor = Color.Cyan;



        public static Boolean InAllianceChat = false;

        [HarmonyPatch(typeof(MyHudChat))]
        [HarmonyPatch("multiplayer_ScriptedChatMessageReceived")]
        class ScriptedChatMessage
        {
            static Boolean Postfix(string message, string author, string font, Color color)
            {
                if (author.Equals("AllianceChatStatus"))
                {
                    if (message.Equals("true"))
                    {
                        InAllianceChat = true;
                    }
                    else
                    {
                        InAllianceChat = false;
                    }
                }
                if (author.Equals("AllianceColorConfig"))
                {
                    try
                    {
                        int r = int.Parse(message.Split(' ')[0]);
                        int g = int.Parse(message.Split(' ')[1]);
                        int b = int.Parse(message.Split(' ')[2]);
                        AllianceColor = new Color(r, g, b);
                    }
                    catch (Exception e)
                    {

                        return false;
                    }

                    return false;
                }
                if (author.Equals("AllianceTitleConfig"))
                {
                    //  title = messageText.Split
                    title = message;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(MyHudChat))]
        [HarmonyPatch("OnMultiplayer_ChatMessageReceived")]
        class ChatMessage
        {
            static Boolean Prefix(ulong steamUserId, string messageText, ChatChannel channel, long targetId, ChatMessageCustomData? customData)
            {


                if (!InAllianceChat)
                {
                    return true;
                }
                if (channel != ChatChannel.Global)
                {
                    return true;
                }
                long identityId = MySession.Static.Players.TryGetIdentityId(steamUserId, 0);

                if (identityId == MySession.Static.LocalPlayerId)
                {
                    string str = MyMultiplayer.Static.GetMemberName(steamUserId);
                    MyHud.Chat.ShowMessage(title + str, messageText, AllianceColor, "Blue");
                    MySession.Static.ChatSystem.ChatHistory.EnqueueMessage(messageText, channel, identityId, targetId, new DateTime?(), "Blue");
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public void Update()
        {
        }


    }
}

﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using SteamKit2;
using Newtonsoft.Json;

namespace SteamChatBot_Mono.Triggers
{
    public class BaseTrigger
    {
        public TriggerType Type { get; set; }
        public string Name { get; set; }
        public TriggerOptions Options { get; set; }

        public bool ReplyEnabled = true;

        #region constructors

        public BaseTrigger(TriggerType type, string name)
        {
            Type = type;
            Name = name;
            Options = new TriggerOptions();
        }

        public BaseTrigger(TriggerType type, string name, TriggerOptions options)
        {
            Type = type;
            Name = name;
            Options = options;
        }

        #endregion

        /// <summary>
        /// If there is an error, log it easily
        /// </summary>
        /// <param name="cbn"></param>
        /// <param name="name"></param>
        /// <param name="error"></param>
        /// <returns>error string</returns>
        protected string IfError(string cbn, string name, string error)
        {
            return string.Format("{0}/{1}: Error: {2}", cbn, name, error);
        }

        #region trigger read-write

        /// <summary>
        /// Save current trigger to file
        /// </summary>
        public void SaveTrigger()
        {
            if (!Directory.Exists(Bot.username + "/triggers/"))
            {
                Directory.CreateDirectory(Bot.username + "/triggers/");
            }

            if (Options != null)
            {
                TriggerOptions options = new TriggerOptions
                {
                    Delay = Options.Delay,
                    Probability = Options.Probability,
                    Timeout = Options.Timeout,
                    Ignore = Options.Ignore,
                    User = Options.User,
                    Rooms = Options.Rooms,
                    Command = Options.Command,
                    Matches = Options.Matches,
                    Responses = Options.Responses,
                    ApiKey = Options.ApiKey
                };
                string json = JsonConvert.SerializeObject(options, Formatting.Indented);
                File.WriteAllText(Bot.username + "/triggers/" + Name + ".json", json);
            }
            else if (Options == null)
            {
                TriggerOptions options = new TriggerOptions();
                string json = JsonConvert.SerializeObject(options, Formatting.Indented);
                File.WriteAllText(Bot.username + "/triggers/" + Name + ".json", json);
            }
        }
        
        public static List<BaseTrigger> ReadTriggers()
        {
            List<BaseTrigger> temp = new List<BaseTrigger>();
            IEnumerable<string> files = Directory.EnumerateFiles(Bot.username + "/triggers/");
            foreach (string file in files)
            {
                int start = file.IndexOf("triggers/") + "triggers/".Length;
                int end = file.IndexOf(".", start);
                string _file = file.Substring(start, end - start);
                TriggerOptions options = JsonConvert.DeserializeObject<TriggerOptions>(File.ReadAllText(file));
                TriggerType type = (TriggerType)Enum.Parse(typeof(TriggerType), _file.Substring(_file.IndexOf('/') + 1));
                switch (type)
                {
                    case TriggerType.AcceptChatInviteTrigger:
                        temp.Add(new AcceptChatInviteTrigger(type, _file, options));
                        break;
                    case TriggerType.AcceptFriendRequestTrigger:
                        temp.Add(new AcceptFriendRequestTrigger(type, _file));
                        break;
                    case TriggerType.AutojoinChatTrigger:
                        temp.Add(new AutojoinChatTrigger(type, _file, options));
                        break;
                    case TriggerType.BanCheckTrigger:
                        temp.Add(new BanCheckTrigger(type, _file, options));
                        break;
                    case TriggerType.BanTrigger:
                        temp.Add(new BanTrigger(type, _file, options));
                        break;
                    case TriggerType.ChatReplyTrigger:
                        temp.Add(new ChatReplyTrigger(type, _file, options));
                        break;
                    case TriggerType.DoormatTrigger:
                        temp.Add(new DoormatTrigger(type, _file, options));
                        break;
                    case TriggerType.IsUpTrigger:
                        temp.Add(new IsUpTrigger(type, _file, options));
                        break;
                    case TriggerType.KickTrigger:
                        temp.Add(new KickTrigger(type, _file, options));
                        break;
                    case TriggerType.LeaveChatTrigger:
                        temp.Add(new LeaveChatTrigger(type, _file, options));
                        break;
                    case TriggerType.LinkNameTrigger:
                        temp.Add(new LinkNameTrigger(type, _file, options));
                        break;
                    case TriggerType.LockChatTrigger:
                        temp.Add(new LockChatTrigger(type, _file, options));
                        break;
                    case TriggerType.ModerateChatTrigger:
                        temp.Add(new ModerateChatTrigger(type, _file, options));
                        break;
                    case TriggerType.UnbanTrigger:
                        temp.Add(new UnbanTrigger(type, _file, options));
                        break;
                    case TriggerType.UnlockChatTrigger:
                        temp.Add(new UnlockChatTrigger(type, _file, options));
                        break;
                    case TriggerType.UnmoderateChatTrigger:
                        temp.Add(new UnmoderateChatTrigger(type, _file, options));
                        break;
                    case TriggerType.WeatherTrigger:
                        temp.Add(new WeatherTrigger(type, _file, options));
                        break;
                    default:
                        break;
                }
            }
            return temp;
        }

        #endregion

        #region overriden methods
        /// <summary>
        /// Return true if trigger loads properly
        /// </summary>
        /// <returns></returns>
        public virtual bool OnLoad()
        {
            try
            {
                bool ret = onLoad();
                if (!ret)
                {
                    Log.Instance.Error("{0}/{1}: Error loading trigger {2}: OnLoad returned {3}", Bot.username, Name, Name, ret);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                return false;
            }
        }

        /// <summary>
        /// Reacts to bot being logged on
        /// </summary>
        /// <returns></returns>
        public virtual bool OnLoggedOn()
        {
            try
            {
                return onLoggedOn();
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                return false;
            }
        }

        /// <summary>
        /// Reacts to bot being logged off
        /// </summary>
        /// <returns></returns>
        public virtual bool OnLoggedOff()
        {
            try
            {
                return onLoggedOff();
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                return false;
            }
        }

        /// <summary>
        /// Return true if the invite is accepted
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="roomName"></param>
        /// <param name="inviterID"></param>
        /// <returns></returns>
        public virtual bool OnChatInvite(SteamID roomID, string roomName, SteamID inviterID)
        {
            if (CheckUser(inviterID) && CheckRoom(roomID) && !CheckIgnores(roomID, inviterID))
            {
                try
                {
                    return respondToChatInvite(roomID, roomName, inviterID);
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Return true if the request is accepted
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public virtual bool OnFriendRequest(SteamID userID)
        {
            try
            {
                return respondToFriendRequest(userID);
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                return false;
            }
        }

        /// <summary>
        /// Return true if a message was sent
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="message"></param>
        /// <param name="haveSentMessage"></param>
        /// <returns></returns>
        public virtual bool OnFriendMessage(SteamID userID, string message, bool haveSentMessage)
        {
            if (ReplyEnabled && RandomRoll() && CheckUser(userID) && !CheckIgnores(userID, null))
            {
                try
                {
                    bool messageSent = respondToFriendMessage(userID, message);
                    if (messageSent)
                    {
                        Log.Instance.Silly("{0}/{1}: Sent RespondToFriendMessage - {2} - {3}", Bot.username, Name, userID, message);
                        DisableForTimeout();
                    }
                    return messageSent;
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// When someone sends a trade offer
        /// </summary>
        /// <param name="number"></param>
        /// <param name="haveEatenEvent"></param>
        /// <returns></returns>
        public virtual bool OnTradeOffer(int number, bool haveEatenEvent)
        {
            try
            {
                var eventEaten = respondToTradeOffer(number);
                if (eventEaten)
                {
                    Log.Instance.Silly("{0}/{1}: Sent RespondToTradeOffer: {2}", Bot.username, Name, number);
                }
                return eventEaten;
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                return false;
            }
        }

        /// <summary>
        /// When someone sends a trade invite
        /// </summary>
        /// <param name="tradeID"></param>
        /// <param name="userID"></param>
        /// <param name="haveEatenEvent"></param>
        /// <returns></returns>
        public virtual bool OnTradeProposed(SteamID tradeID, SteamID userID, bool haveEatenEvent)
        {
            if (CheckUser(userID) && CheckIgnores(userID, null))
            {
                try
                {
                    bool eventEaten = respondToTradeProposal(tradeID, userID);
                    if (eventEaten)
                    {
                        Log.Instance.Silly("{0}/{1}: Sent RespondToTradeProposal: {2}", Bot.username, Name, tradeID);
                    }
                    return eventEaten;
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// When someone a trade is opened
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="haveEatenEvent"></param>
        /// <returns></returns>
        public virtual bool OnTradeSession(SteamID userID, bool haveEatenEvent)
        {
            try
            {
                bool eventEaten = respondToTradeSession(userID);
                if (eventEaten)
                {
                    Log.Instance.Silly("{0}/{1}: Opened trade with: {2}", Bot.username, Name, userID);
                }
                return eventEaten;
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                return false;
            }
        }

        /// <summary>
        /// When a group makes an announcement
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="headline"></param>
        /// <param name="haveEatenEvent"></param>
        /// <returns></returns>
        public virtual bool OnAnnouncement(SteamID groupID, string headline, bool haveEatenEvent)
        {
            if (CheckIgnores(groupID, null))
            {
                try
                {
                    bool eventEaten = respondToAnnouncement(groupID, headline);
                    if (eventEaten)
                    {
                        Log.Instance.Silly("{0}/{1}: responded to {1}'s announcement: {2}", Bot.username, Name, groupID, headline);
                    }
                    return eventEaten;
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Return true if message was seen but don't want other triggers to see
        /// </summary>
        /// <param name="toID"></param>
        /// <param name="message"></param>
        /// <param name="haveSentMessage"></param>
        /// <returns></returns>
        public virtual bool OnSentMessage(SteamID toID, string message, bool haveSentMessage)
        {
            try
            {
                bool messageSeen = respondToSentMessage(toID, message);
                if (messageSeen)
                {
                    return true;
                }
                return messageSeen;
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                return false;
            }
        }

        /// <summary>
        /// Return true if a message was sent
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="chatterID"></param>
        /// <param name="message"></param>
        /// <param name="haveSentMessage"></param>
        /// <returns></returns>
        public virtual bool OnChatMessage(SteamID roomID, SteamID chatterID, string message, bool haveSentMessage)
        {
            if (ReplyEnabled && RandomRoll() && CheckUser(chatterID) && CheckRoom(roomID) && !CheckIgnores(chatterID, roomID))
            {
                try
                {
                    bool messageSent = respondToChatMessage(roomID, chatterID, message);
                    if (messageSent)
                    {
                        Log.Instance.Silly("{0}/{1}: Sent RespondToChatMessage - {2} - {3} - {4}", Bot.username, Name, chatterID, roomID, message);
                        DisableForTimeout();
                    }
                    return messageSent;
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                    return false;
                }
            }
            else
            {
                Log.Instance.Warn(ReplyEnabled.ToString() + RandomRoll().ToString() + CheckUser(chatterID) + CheckRoom(roomID) + !CheckIgnores(chatterID, roomID));
            }
            return false;
        }

        /// <summary>
        /// Return true if a message was sent
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="userID"></param>
        /// <param name="haveSentMessage"></param>
        /// <returns></returns>
        public virtual bool OnEnteredChat(SteamID roomID, SteamID userID, bool haveSentMessage)
        {
            if (ReplyEnabled && RandomRoll() && CheckRoom(roomID) && CheckUser(userID) && !CheckIgnores(userID, roomID))
            {
                try
                {
                    bool messageSent = respondToEnteredMessage(roomID, userID);
                    if(messageSent)
                    {
                        DisableForTimeout();
                    }
                    return messageSent;
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Return true if a message was sent
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="kickedID"></param>
        /// <param name="kickerID"></param>
        /// <param name="haveSentMessage"></param>
        /// <returns></returns>
        public virtual bool OnKickedChat(SteamID roomID, SteamID kickedID, SteamID kickerID, bool haveSentMessage)
        {
            if (ReplyEnabled && RandomRoll() && CheckRoom(roomID)) {
                try
                {
                    bool messageSent = respondToKick(roomID, kickedID, kickerID);
                    if(messageSent)
                    {
                        DisableForTimeout();
                    }
                    return messageSent;
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if a message was sent
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="bannedID"></param>
        /// <param name="bannerID"></param>
        /// <param name="haveSentMessage"></param>
        /// <returns></returns>
        public virtual bool OnBannedChat(SteamID roomID, SteamID bannedID, SteamID bannerID, bool haveSentMessage)
        {
            if (ReplyEnabled && RandomRoll() && CheckRoom(roomID))
            {
                try
                {
                    bool messageSent = respondToBan(roomID, bannedID, bannerID);
                    if(messageSent)
                    {
                        DisableForTimeout();
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if a message was sent
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="userID"></param>
        /// <param name="haveSentMessage"></param>
        /// <returns></returns>
        public virtual bool OnDisconnected(SteamID roomID, SteamID userID, bool haveSentMessage)
        {
            if (ReplyEnabled && RandomRoll() && CheckRoom(roomID) && CheckUser(userID) && !CheckIgnores(userID, roomID))
            {
                try
                {
                    bool messageSent = respondToDisconnect(roomID, userID);
                    if(messageSent)
                    {
                        DisableForTimeout();
                    }
                    return messageSent;
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                    return false;
                }
            }
            return false;
        }

        public virtual bool OnLeftChat(SteamID roomID, SteamID userID)
        {
            if (ReplyEnabled && RandomRoll() && CheckRoom(roomID) && CheckUser(userID) && !CheckIgnores(roomID, userID))
            {
                try
                {
                    bool messageSent = respondToEnteredMessage(roomID, userID);
                    if(messageSent)
                    {
                        DisableForTimeout();
                    }
                    return messageSent;
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(Bot.username, Name, e.StackTrace));
                    return false;
                }
            }
            return false;
        }
        #endregion

        #region subclass methods

        public virtual bool onLoad()
        {
            return true;
        }

        public virtual bool onLoggedOn()
        {
            return true;
        }

        public virtual bool onLoggedOff()
        {
            return true;
        }

        public virtual bool respondToChatInvite(SteamID roomID, string roomName, SteamID inviterId)
        {
            return false;
        }

        // Returns true if the request is accepted
        public virtual bool respondToFriendRequest(SteamID userID)
        {
            return false;
        }

        // Return true if a message was sent
        public virtual bool respondToFriendMessage(SteamID userID, string message)
        {
            return false;
        }

        // Return true if a sent message has been used and shouldn't be seen again.
        public virtual bool respondToSentMessage(SteamID toID, string message)
        {
            return false;
        }

        // Return true if a message was sent
        public virtual bool respondToChatMessage(SteamID roomID, SteamID chatterId, string message)
        {
            return false;
        }

        // Return true if the event was eaten
        public virtual bool respondToEnteredMessage(SteamID roomID, SteamID userID)
        {
            return false;
        }

        // Return true if the event was eaten
        public virtual bool respondToBan(SteamID roomID, SteamID bannedId, SteamID bannerId)
        {
            return false;
        }

        // Return true if the event was eaten
        public virtual bool respondToDisconnect(SteamID roomID, SteamID userID)
        {
            return false;
        }

        // Return true if the event was eaten
        public virtual bool respondToLeftMessage(SteamID roomID, SteamID userID)
        {
            return false;
        }

        // Return true if the event was eaten
        public virtual bool respondToKick(SteamID roomID, SteamID kickedId, SteamID kickerId)
        {
            return false;
        }

        public virtual bool respondToAnnouncement(SteamID groupID, string headline)
        {
            return false;
        }

        public virtual bool respondToTradeSession(SteamID userID)
        {
            return false;
        }

        public virtual bool respondToTradeProposal(SteamID tradeId, SteamID steamId)
        {
            return false;
        }

        public virtual bool respondToTradeOffer(int number)
        {
            return false;
        }
        #endregion

        #region helper methods


        /// <summary>
        /// Sends a message to the specified SteamID
        /// </summary>
        /// <param name="steamID"></param>
        /// <param name="message"></param>
        /// <param name="room"></param>
        protected void SendMessageAfterDelay(SteamID steamID, string message, bool room)
        {
            if (Options.Delay == null || Options.Delay.Value == 0)
            {
                Log.Instance.Silly("{0}/{1}: Sending non delayed message to {2}: {3}", Bot.username, Name, steamID, message);
                if (room)
                {
                    Bot.steamFriends.SendChatRoomMessage(steamID, EChatEntryType.ChatMsg, message);
                }
                else
                {
                    Bot.steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, message);
                }
            }
            else
            {
                Log.Instance.Silly("{0}/{1}: Sending delayed message to {2}: {3}", Bot.username, Name, steamID, message);
                System.Timers.Timer timer = new System.Timers.Timer(Options.Delay.Value / 1000);

                timer.Elapsed += (sender, e) => TimerElapsed_Message(sender, e, steamID, message, room);
            }
        }

        private void TimerElapsed_Message(object sender, System.Timers.ElapsedEventArgs e, SteamID steamID, string message, bool room)
        {
            if (room)
            {
                Bot.steamFriends.SendChatRoomMessage(steamID, EChatEntryType.ChatMsg, message);
            }
            else
            {
                Bot.steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, message);
            }
        }

        protected string[] StripCommand(string message, string command)
        {
            if (message != null && command != null && message.ToLower().IndexOf(command.ToLower()) == 0)
            {
                return message.Split(' ');
            }
            return null;
        }

        
        protected bool CheckIgnores(SteamID toID, SteamID fromID)
        {
            if (Options.Ignore != null && Options.Ignore.Count > 0)
            {
                for (int i = 0; i < Options.Ignore.Count; i++)
                {
                    SteamID ignored = Options.Ignore[i];
                    if (toID == ignored || fromID == ignored)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        protected bool CheckRoom(SteamID toID)
        {
            if (Options.Rooms == null || Options.Rooms.Count == 0)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < Options.Rooms.Count; i++)
                {
                    SteamID room = Options.Rooms[i];
                    if (toID == room)
                    {
                        return true;
                    }
                }
                return true;
            }
        }

        protected bool CheckUser(SteamID fromID)
        {
            if (Options.User != null && Options.User.Count > 0)
            {
                for (int i = 0; i < Options.User.Count; i++)
                {
                    SteamID user = Options.User[i];
                    if (fromID == user)
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        protected bool RandomRoll()
        {
            if(Options.Probability != null || Options.Probability == 1)
            {
                double rng = new Random().Next(0, 1);
                if(rng > Options.Probability)
                {
                    return false;
                }
            }
            return true;
        }

        protected void DisableForTimeout()
        {
            if(Options.Timeout != null && Options.Timeout.Value > 0)
            {
                ReplyEnabled = false;
                Log.Instance.Silly("{0}/{1}: Setting timeout ({2} ms)", Bot.username, Name, Options.Timeout);
                System.Timers.Timer timer = new System.Timers.Timer(Options.Timeout.Value);
                timer.Elapsed += (sender, e) => AfterTimer_Timeout(sender, e);
            }
        }

        private void AfterTimer_Timeout(object sender, ElapsedEventArgs e)
        {
            Log.Instance.Silly("{0}/{1}: Timeout expired", Bot.username, Name);
            ReplyEnabled = true;
        }
        #endregion

    }
}

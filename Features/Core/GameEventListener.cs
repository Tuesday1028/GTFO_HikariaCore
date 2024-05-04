﻿using Hikaria.Core.Interfaces;
using SNetwork;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;

namespace Hikaria.Core.Features.Core;

[EnableFeatureByDefault]
[DisallowInGameToggle]
[HideInModSettings]
[DoNotSaveToConfig]
internal class GameEventListener : Feature
{
    public override string Name => "Game Event Listener";

    public override FeatureGroup Group => EntryPoint.Groups.Core;

    public static GameEventListener Instance { get; private set; }

    public static new IArchiveLogger FeatureLogger { get; set; }

    [ArchivePatch(typeof(SNet_SessionHub), nameof(SNet_SessionHub.AddPlayerToSession))]
    private class SNet_SessionHub__AddPlayerToSession__Patch
    {
        private static void Postfix(SNet_Player player)
        {
            OnSessionMemberChangedM(player, SessionMemberEvent.JoinSessionHub);
        }
    }

    [ArchivePatch(typeof(SNet_SessionHub), nameof(SNet_SessionHub.RemovePlayerFromSession))]
    private class SNet_SessionHub__RemovePlayerFromSession__Patch
    {
        private static void Postfix(SNet_Player player)
        {
            OnSessionMemberChangedM(player, SessionMemberEvent.LeftSessionHub);
        }
    }

    [ArchivePatch(typeof(SNet_SessionHub), nameof(SNet_SessionHub.LeaveHub))]
    private class SNet_SessionHub__LeaveHub__Patch
    {
        private static void Postfix()
        {
            OnSessionMemberChangedM(SNet.LocalPlayer, SessionMemberEvent.LeftSessionHub);
        }
    }

    [ArchivePatch(typeof(SNet_GlobalManager), nameof(SNet_GlobalManager.Setup))]
    private class SNet_GlobalManager__Setup__Patch
    {
        private static void Postfix()
        {
            SNet_Events.OnPlayerEvent += new Action<SNet_Player, SNet_PlayerEvent, SNet_PlayerEventReason>(OnPlayerEventM);
            SNet_Events.OnRecallComplete += new Action<eBufferType>(OnRecallCompleteM);
        }
    }

    [ArchivePatch(typeof(GameStateManager), nameof(GameStateManager.DoChangeState))]
    private class GameStateManager__DoChangeState__Patch
    {
        private static void Prefix(GameStateManager __instance)
        {
            preState = __instance.m_currentStateName;
        }

        private static void Postfix(eGameStateName nextState)
        {
            foreach (var listener in GameStateChangeListeners)
            {
                try
                {
                    listener.OnGameStateChanged(preState, nextState);
                }
                catch (Exception ex)
                {
                    FeatureLogger.Exception(ex);
                }
            }
            var onGameStateChanged = OnGameStateChanged;
            if (onGameStateChanged != null)
            {
                onGameStateChanged(preState, nextState);
            }
        }
    }

    [ArchivePatch(typeof(PlayerChatManager), nameof(PlayerChatManager.DoSendChatMessage))]
    private class PlayerCharManager__DoSendChatMessage__Patch
    {
        private static void Postfix(PlayerChatManager.pChatMessage data)
        {
            if (data.fromPlayer.TryGetPlayer(out SNet_Player fromPlayer))
            {
                foreach (var listener in ChatMessageListeners)
                {
                    try
                    {
                        listener.OnReceiveChatMessage(fromPlayer, data.message.data);
                    }
                    catch (Exception ex)
                    {
                        FeatureLogger.Exception(ex);
                    }
                }
                var onReceiveChatMessage = OnReceiveChatMessage;
                if (onReceiveChatMessage != null)
                {
                    onReceiveChatMessage(fromPlayer, data.message.data);
                }
            }
        }
    }

    private static void OnRecallCompleteM(eBufferType bufferType)
    {
        foreach (var listener in OnRecallCompleteListeners)
        {
            try
            {
                listener.OnRecallComplete(bufferType);
            }
            catch (Exception ex)
            {
                FeatureLogger.Exception(ex);
            }
        }
        var onRecallComplete = OnRecallComplete;
        if (onRecallComplete != null)
        {
            onRecallComplete(bufferType);
        }
    }

    private static void OnPlayerEventM(SNet_Player player, SNet_PlayerEvent playerEvent, SNet_PlayerEventReason reason)
    {
        foreach (var Listener in PlayerEventListeners)
        {
            try
            {
                Listener.OnPlayerEvent(player, playerEvent, reason);
            }
            catch (Exception ex)
            {
                FeatureLogger.Exception(ex);
            }
        }
        var onPlayerEvent = OnPlayerEvent;
        if (onPlayerEvent != null)
        {
            onPlayerEvent(player, playerEvent, reason);
        }
    }


    private static void OnSessionMemberChangedM(SNet_Player player, SessionMemberEvent playerEvent)
    {
        FeatureLogger.Notice($"{player.NickName} [{player.Lookup}] {playerEvent}");
        foreach (var Listener in SessionMemberChangeListeners)
        {
            try
            {
                Listener.OnSessionMemberChanged(player, playerEvent);
            }
            catch (Exception ex)
            {
                FeatureLogger.Exception(ex);
            }
        }
        var onSessionMemberChanged = OnSessionMemberChanged;
        if (onSessionMemberChanged != null)
        {
            onSessionMemberChanged(player, playerEvent);
        }
    }

    private static eGameStateName preState;

    public static HashSet<IOnGameStateChanged> GameStateChangeListeners = new();

    public static HashSet<IOnReceiveChatMessage> ChatMessageListeners = new();

    public static HashSet<IOnPlayerEvent> PlayerEventListeners = new();

    public static HashSet<IOnRecallComplete> OnRecallCompleteListeners = new();

    public static HashSet<IOnSessionMemberChanged> SessionMemberChangeListeners = new();

    public static event Action OnGameDataInited;

    public static event Action<eBufferType> OnRecallComplete;

    public static event Action<eGameStateName, eGameStateName> OnGameStateChanged;

    public static event Action<SNet_Player, string> OnReceiveChatMessage;

    public static event Action<SNet_Player, SNet_PlayerEvent, SNet_PlayerEventReason> OnPlayerEvent;

    public static event Action<SNet_Player, SessionMemberEvent> OnSessionMemberChanged;
}
﻿using Hikaria.Core.Interfaces;

namespace Hikaria.Core.SNetworkExt;

public abstract class SNetExt_SyncedAction<T> : IOnSessionMemberChanged, IOnPlayerModsSynced where T : struct
{
    ~SNetExt_SyncedAction()
    {
        GameEventAPI.UnregisterListener(this);
        CoreAPI.UnregisterListener(this);
    }

    protected void Setup(string eventName, Action<ulong, T> incomingAction, Action<T> incomingActionValidation = null, Func<SNetwork.SNet_Player, bool> listenerFilter = null, SNetwork.SNet_ChannelType channelType = SNetwork.SNet_ChannelType.GameOrderCritical)
    {
        m_packet = SNetExt_Packet<T>.Create(eventName, incomingAction, incomingActionValidation, false, channelType);
        m_incomingAction = incomingAction;
        m_listenerFilter = listenerFilter;
        m_hasListenerFilter = listenerFilter != null;
        GameEventAPI.RegisterListener(this);
        CoreAPI.RegisterListener(this);
    }

    public void SyncToPlayer(SNetwork.SNet_Player player, T data)
    {
        if (!player.IsLocal)
        {
            m_packet.Send(data, player);
        }
    }

    public void SyncToPlayer(SNetwork.SNet_Player player, params T[] datas)
    {
        foreach (var data in datas)
        {
            SyncToPlayer(player, data);
        }
    }

    public void SyncToPlayer(SNetwork.SNet_Player player, IEnumerable<T> datas)
    {
        foreach (var data in datas)
        {
            SyncToPlayer(player, data);
        }
    }

    public void OnPlayerModsSynced(SNetwork.SNet_Player player, IEnumerable<pModInfo> mods)
    {
        if (!m_hasListenerFilter || !m_listenerFilter(player))
            return;
        if (player.IsInSessionHub)
        {
            Internal_AddPlayerToListeners(player);
        }
    }

    public void OnSessionMemberChanged(SNetwork.SNet_Player player, SessionMemberEvent playerEvent)
    {
        if (playerEvent == SessionMemberEvent.JoinSessionHub)
        {
            if (m_hasListenerFilter && !m_listenerFilter(player))
                return;

            Internal_AddPlayerToListeners(player);
        }
        else if (playerEvent == SessionMemberEvent.LeftSessionHub)
        {
            Internal_RemovePlayerFromListeners(player);
        }
    }

    private void Internal_AddPlayerToListeners(SNetwork.SNet_Player player)
    {
        m_listeners.RemoveAll(p => p.Lookup == player.Lookup);
        m_listeners.Add(player);
        m_listenersLookup[player.Lookup] = player;

        var onPlayerAddedToListeners = OnPlayerAddedToListeners;
        onPlayerAddedToListeners?.Invoke(player);
    }

    private void Internal_RemovePlayerFromListeners(SNetwork.SNet_Player player)
    {
        if (player.IsLocal)
        {
            var onPlayerRemovedFromListeners = OnPlayerRemovedFromListeners;
            foreach (var listener in m_listeners.ToList())
            {
                m_listeners.RemoveAll(p => p.Lookup == player.Lookup);
                m_listenersLookup.Remove(player.Lookup);
                onPlayerRemovedFromListeners?.Invoke(listener);
            }
        }
        else
        {
            m_listeners.RemoveAll(p => p.Lookup == player.Lookup);
            m_listenersLookup.Remove(player.Lookup);

            var onPlayerRemovedFromListeners = OnPlayerRemovedFromListeners;
            onPlayerRemovedFromListeners?.Invoke(player);
        }
    }

    public void AddPlayerToListeners(SNetwork.SNet_Player player)
    {
        if (m_hasListenerFilter && !m_listenerFilter(player))
            return;

        Internal_AddPlayerToListeners(player);
    }

    public void RemovePlayerFromListeners(SNetwork.SNet_Player player)
    {
        Internal_RemovePlayerFromListeners(player);
    }

    public bool IsListener(SNetwork.SNet_Player player)
    {
        if (player == null) return false;
        return m_listeners.Contains(player) || IsListener(player.Lookup);
    }

    public bool IsListener(ulong lookup)
    {
        return m_listenersLookup.ContainsKey(lookup);
    }

    public event Action<SNetwork.SNet_Player> OnPlayerAddedToListeners;

    public event Action<SNetwork.SNet_Player> OnPlayerRemovedFromListeners;

    protected SNetExt_Packet<T> m_packet;
    protected Action<ulong, T> m_incomingAction;
    protected Func<SNetwork.SNet_Player, bool> m_listenerFilter;
    protected bool m_hasListenerFilter;
    protected List<SNetwork.SNet_Player> m_listeners = new();
    protected Dictionary<ulong, SNetwork.SNet_Player> m_listenersLookup = new();
    public IEnumerable<SNetwork.SNet_Player> Listeners => m_listeners;
    public IReadOnlyDictionary<ulong, SNetwork.SNet_Player> ListenersLookup => m_listenersLookup;
}

﻿using Hikaria.Core.Entities;
using Hikaria.Core.Utility;
using SNetwork;
using Steamworks;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Features.Security;
using TheArchive.Interfaces;
using UnityEngine;

namespace Hikaria.Core.Features.Security
{
    [AutomatedFeature]
    [DoNotSaveToConfig]
    internal class GlobalBan : Feature
    {
        public override string Name => "在线全局封禁名单";

        public override FeatureGroup Group => EntryPoint.Groups.Security;

        public static new IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static GlobalBanListSettings Settings { get; set; }

        public class GlobalBanListSettings
        {
            private List<BannedPlayerEntry> _bannedPlayersEnties = new();

            [FSDisplayName("封禁玩家")]
            public List<BannedPlayerEntry> BannedPlayers
            {
                get
                {
                    for (int i = 0; i < _bannedPlayers.Count; i++)
                    {
                        if (!_bannedPlayersEnties.Any(p => p.SteamID == _bannedPlayers[i].SteamID))
                        {
                            _bannedPlayersEnties.Add(new(_bannedPlayers[i]));
                        }
                    }
                    return _bannedPlayersEnties;
                }
                set
                {
                }
            }
        }

        public class BannedPlayerEntry
        {
            public BannedPlayerEntry(BannedPlayer bannedPlayer)
            {
                SteamID = bannedPlayer.SteamID;
                Name = bannedPlayer.Name;
                Reason = bannedPlayer.Reason;
                TimeStamp = bannedPlayer.DateBanned.Ticks;
            }

            [FSSeparator]
            [FSReadOnly]
            [FSDisplayName("SteamID")]
            public ulong SteamID { get; set; }
            [FSReadOnly]
            [FSDisplayName("玩家名称")]
            public string Name { get; set; }
            [FSReadOnly]
            [FSDisplayName("封禁原因")]
            public string Reason { get; set; }
            [FSReadOnly]
            [FSTimestamp]
            [FSDisplayName("封禁时间")]
            public long TimeStamp { get; set; }
        }

        public override void OnEnable()
        {
            if (_task != null && !_task.IsCompleted && !_task.IsFaulted && !_task.IsCanceled)
            {
                return;
            }

            _task = Task.Run(GetBannedPlayers);
        }

        public override void Update()
        {
            if (_task == null || _task.IsCanceled || _task.IsFaulted)
            {
                FeatureManager.DisableAutomatedFeature(typeof(GlobalBan));
                return;
            }

            if (!_task.IsCompleted)
                return;

            _bannedPlayers = _task.Result;

            for (int i = 0; i < _bannedPlayers.Count; i++)
            {
                var player = _bannedPlayers[i];
                if (!PlayerLobbyManagement.IsPlayerBanned(player.SteamID))
                {
                    PlayerLobbyManagement.Settings.BanList.Add(new()
                    {
                        SteamID = player.SteamID,
                        Name = player.Name,
                        Timestamp = player.DateBanned.Ticks
                    });
                    FeatureLogger.Notice($"SteamID: {player.SteamID}, Name: {player.Name}, DateBanned: {player.DateBanned.ToLongDateString()}");
                }
            }
            var selfId = Steamworks.SteamUser.GetSteamID().m_SteamID;
            if (_bannedPlayers.Find(p => p.SteamID == selfId) != null)
            {
                Application.Quit();
                return;
            }
            MarkSettingsAsDirty(PlayerLobbyManagement.Settings);
            FeatureManager.DisableAutomatedFeature(typeof(GlobalBan));
        }

        private static async Task<List<BannedPlayer>> GetBannedPlayers()
        {
            return await HttpHelper.GetAsync<List<BannedPlayer>>($"{CoreGlobal.ServerUrl}/bannedplayers/GetAllBannedPlayers");
        }

        private static List<BannedPlayer> _bannedPlayers = new();
        private static Task<List<BannedPlayer>> _task;
    }
}

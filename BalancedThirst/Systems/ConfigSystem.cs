using System;
using BalancedThirst.Config;
using BalancedThirst.Util;
using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace BalancedThirst.Systems;
public class ConfigSystem : ModSystem
{
    private IClientNetworkChannel? _clientChannel;
    private IServerNetworkChannel? _serverChannel;
    private const string _channelName = "balancedthirst:config";
    private ICoreAPI? _api;
    
    public delegate void ConfigLoadedHandler();
    public static event ConfigLoadedHandler OnConfigLoaded;
    public static bool IsLoaded { get; private set; }

    public static void MarkAsLoaded()
    {
        IsLoaded = true;
        OnConfigLoaded?.Invoke();
    }

    public override double ExecuteOrder() => 0.01;
    public static ConfigServer ConfigServer { get; set; }
    public static ConfigClient ConfigClient { get; set; }
    public static SyncedConfig SyncedConfig { get; set; } = new();
    public static SyncedConfig SyncedConfigData => ConfigServer ?? SyncedConfig;

    public override void Start(ICoreAPI api)
    {
        _api = api;
    }

    #region Client
    
    
    public override void StartClientSide(ICoreClientAPI api)
    {
        ConfigClient = ModConfig.ReadConfig<ConfigClient>(api, BtConstants.ConfigClientName);
        _clientChannel = (api as ICoreClientAPI)?.Network.RegisterChannel(_channelName)
            .RegisterMessageType<SyncedConfig>()
            .SetMessageHandler<SyncedConfig>(ReloadSyncedConfig);
        api.Event.RegisterEventBusListener(AdminSendSyncedConfig, filterByEventName: EventIds.AdminSetConfig);
        MarkAsLoaded();
    }

    private void AdminSendSyncedConfig(string eventname, ref EnumHandling handling, IAttribute data)
    {
        _clientChannel?.SendPacket(SyncedConfig.Clone());
        _api?.World.RegisterCallback(_ => _api.Event.PushEvent(EventIds.ConfigReloaded), 100);
    }
    
    private void ReloadSyncedConfig(SyncedConfig packet)
    {
        BtCore.Logger.Warning("Reloading synced config");
        SyncedConfig = packet.Clone();
        _api?.Event.PushEvent(EventIds.ConfigReloaded);
    }
    #endregion

    #region Server
    
    public override void StartServerSide(ICoreServerAPI api)
    {
        ConfigServer = ModConfig.ReadConfig<ConfigServer>(api, BtConstants.ConfigServerName);
        _serverChannel = (api as ICoreServerAPI)?.Network.RegisterChannel(_channelName)
            .RegisterMessageType<SyncedConfig>()
            .SetMessageHandler<SyncedConfig>(ForceConfigFromAdmin);
        
        api.Event.PlayerJoin += SendSyncedConfig;
        api.Event.RegisterEventBusListener(SendSyncedConfig, filterByEventName: EventIds.ConfigReloaded);
        MarkAsLoaded();
    }

    private void ForceConfigFromAdmin(IServerPlayer fromplayer, SyncedConfig packet)
    {
        BtCore.Logger.Warning("Forcing config from admin");
        if (fromplayer.HasPrivilege("controlserver"))
        {
            ConfigServer.UpdateFromSyncedConfig(packet.Clone());
        }
    }

    private void SendSyncedConfig(string eventname, ref EnumHandling handling, IAttribute data)
    {
        BtCore.Logger.Warning("Config reloaded, sending to all players");
        if (_api?.World == null) return;
        foreach (var player in _api.World.AllPlayers)
        {
            if (player is not IServerPlayer serverPlayer) continue;
            SendSyncedConfig(serverPlayer);
        }
    }

    private void SendSyncedConfig(IServerPlayer byplayer)
    {
        BtCore.Logger.Warning("Sending config to player: {0}", byplayer.PlayerName);
        _serverChannel?.SendPacket(SyncedConfig.FromServerConfig(ConfigServer), byplayer);
    }
    #endregion
}
using CounterStrikeSharp.API.Core.Capabilities;
using VipCoreApi;
using Hooks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static VipCoreApi.IVipCoreApi;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Microsoft.Extensions.Logging;
using System.Numerics;


namespace VipHooks;
public class VipHooks(): BasePlugin
{
    

    public override string ModuleAuthor => "Nick Fox";
    public override string ModuleName => "[VIP] Hooks";
    public override string ModuleVersion => "1.1";

    private IVipCoreApi? _vip;
    private IHooksApi? _hooks;
    private HooksModule _viphooks;
    private PluginCapability<IVipCoreApi> PluginVip { get; } = new("vipcore:core");
    private PluginCapability<IHooksApi> PluginHooks { get; } = new("hooks:nfcore");

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _vip = PluginVip.Get();
        _hooks = PluginHooks.Get();

        if (_vip == null || _hooks == null) return;

        _vip.OnCoreReady += () =>
        {
            OnCoreLoaded();
        };

        if (hotReload)
            OnCoreLoaded();
    }


    private int[] hooksCount = new int[65];
    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        for (int i = 0; i < 65; i++)
            hooksCount[i] = 0;
        return HookResult.Continue;
    }


    [GameEventHandler]
    public HookResult OnFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        for (int i = 0; i < 65; i++)
        {
            var player = Utilities.GetPlayerFromSlot(i);

            if (IsValidPlayer(player) && _vip.IsClientVip(player) && _vip.PlayerHasFeature(player, "Hooks"))
            {
                var count = _vip.GetFeatureValue<int>(player, "Hooks");
                if(count == 0)
                    hooksCount[i] = -1;
                else
                    hooksCount[i] = count;
            }
        }
        return HookResult.Continue;
    }


    private void OnCoreLoaded()
    {
        _viphooks = new HooksModule(_vip);
        _vip.RegisterFeature(_viphooks, FeatureType.Hide);

        _hooks.AddHook((playerinfo) => {

            var slot = playerinfo.Player().Slot;
            if (hooksCount[slot] > 0)
            {
                hooksCount[slot]--;
                if (hooksCount[slot] == 0)
                    _vip.PrintToChat(playerinfo.Player(), Localizer["expired"]);
                else
                    _vip.PrintToChat(playerinfo.Player(), String.Format(Localizer["use_count"], hooksCount[slot]));
                playerinfo.Set(HookState.Enabled);
                
            }
            else 
                if(hooksCount[slot] == -1)
                    playerinfo.Set(HookState.Enabled);           

        });

    }

    private bool IsValidPlayer(CCSPlayerController player)
    {
        return player != null && player.IsValid && player.Connected == PlayerConnectedState.PlayerConnected && !player.IsBot;
    }

    public override void Unload(bool hotReload)
    {
        _vip.UnRegisterFeature(_viphooks);
    }


    public class HooksModule : VipFeatureBase
    {
        public override string Feature => "Hooks";                
                            
        public HooksModule(IVipCoreApi api) : base(api)
        {
            
        }

    }
}

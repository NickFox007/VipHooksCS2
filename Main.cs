using CounterStrikeSharp.API.Core.Capabilities;
using VipCoreApi;
using Hooks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static VipCoreApi.IVipCoreApi;
using CounterStrikeSharp.API.Modules.Entities;


namespace VipHooks;
public class VipHooks(): BasePlugin
{

    public override string ModuleAuthor => "Nick Fox";
    public override string ModuleName => "[VIP] Hooks";
    public override string ModuleVersion => "1.0";

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


    private void OnCoreLoaded()
    {
        _viphooks = new HooksModule(_vip);
        _vip.RegisterFeature(_viphooks, FeatureType.Hide);

        _hooks.AddHook((playerinfo) => {

            if (_viphooks.HasRights(playerinfo.Player()))
                playerinfo.Set(HookState.Enabled);
                
        });

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

        public bool HasRights(CCSPlayerController player)
        {
            if (IsClientVip(player) && PlayerHasFeature(player))
            {
                return true;
            }
            else return false;
        }

    }
}

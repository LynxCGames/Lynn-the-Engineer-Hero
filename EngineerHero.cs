using MelonLoader;
using BTD_Mod_Helper;
using EngineerHero;

[assembly: MelonInfo(typeof(EngineerHeroLynn), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace EngineerHero;


public class EngineerHeroLynn : BloonsTD6Mod
{

    public override void OnApplicationStart()
    {
        /*foreach (var behavior in Game.instance.model.GetTower(TowerType.SpikeFactory, 0, 5).GetAbility(0).behaviors)
        {

            ModHelper.Msg<EngineerHero>(behavior.TypeName());
        }*/
    }
}
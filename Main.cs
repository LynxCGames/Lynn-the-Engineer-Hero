using BTD_Mod_Helper.Api.Display;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.TowerFilters;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Linq;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Filters;
using EngineerHero.Projectiles;
using EngineerHero;

namespace Lynn
{
    public class Lynn : ModHero
    {
        public override string BaseTower => TowerType.EngineerMonkey;
        public override string Name => "LynnHero";
        public override int Cost => 1080;
        public override string DisplayName => "Lynn";
        public override string Title => "The Construction Worker";
        public override string Level1Description => "Lynn bashes Bloons with her wrench and creates mini worker sentries to help pop Bloons.";
        public override string Description => "Despite her serious attitude at sites, she has a very goofy and carefree personality. Her tail got cut off in an accident at the workplace.";
        public override string Icon => "Lynn-Icon";
        public override string Portrait => "Lynn-Portrait";
        public override string Square => "ButtonIcon";
        public override string Button => "Icon";
        public override string NameStyle => TowerType.Quincy;
        public override int MaxLevel => 20;
        public override float XpRatio => 1.25f;

        public override void ModifyBaseTowerModel(TowerModel towerModel)
        {
            towerModel.GetAttackModel().RemoveWeapon(towerModel.GetAttackModel().weapons[0]);

            towerModel.range *= 1.5f;
            towerModel.GetAttackModel().range = Game.instance.model.GetTower(TowerType.PatFusty).GetAttackModel().range;

            var wrench = Game.instance.model.GetTower(TowerType.PatFusty).GetAttackModel().weapons[0].Duplicate();
            wrench.projectile.GetDamageModel().immuneBloonProperties = BloonProperties.Lead | BloonProperties.Frozen;
            towerModel.GetAttackModel().AddWeapon(wrench);


            foreach (var behavior in Game.instance.model.GetTowerFromId("EngineerMonkey-200").GetAttackModels().ToArray())
            {
                if (behavior.name.Contains("Spawner"))
                {
                    var spawner = behavior.Duplicate();
                    spawner.range = towerModel.range;
                    spawner.name = "Sentry_Place";
                    spawner.weapons[0].projectile.RemoveBehavior<CreateTowerModel>();
                    spawner.weapons[0].projectile.AddBehavior(new CreateTowerModel("SentryPlace", GetTowerModel<WorkerSentry>().Duplicate(), 0f, true, false, false, true, true));

                    towerModel.AddBehavior(spawner);
                }
            }
        }
    }
    public class L2 : ModHeroLevel<Lynn>
    {
        public override string Description => "Helping Hand: All engineer monkeys in range gain 10% bonus attack speed.";
        public override int Level => 2;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            TowerFilterModel engineer = new FilterInBaseTowerIdModel("BaseTowerFilter", new string[] { TowerType.EngineerMonkey });

            towerModel.AddBehavior(new RateSupportModel("HelpingHand", 0.9f, true, "HelpingHandBuff", false, 1, new Il2CppReferenceArray<TowerFilterModel>(new TowerFilterModel[] { engineer }), "", ""));
        }
    }
    public class L3 : ModHeroLevel<Lynn>
    {
        public override string Portrait => "LynnL3-Portrait";
        public override string AbilityName => "Screw Dive ability";
        public override string AbilityDescription => "Sharp screws plummet to the ground covering the whole map.";
        public override string Description => $"{AbilityName}: {AbilityDescription}";
        public override int Level => 3;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            var abilityModel = Game.instance.model.GetTowerFromId("Ezili 3").GetAbility().Duplicate();
            abilityModel.RemoveBehavior<ActivateAttackModel>();
            abilityModel.name = "AbilityModel_ScrewDive";
            abilityModel.icon = GetSpriteReference<EngineerHeroLynn>("ScrewDive-Icon");
            abilityModel.displayName = "Screw Dive";
            abilityModel.description = "Sharp screws plummet to the ground covering the whole map.";
            abilityModel.cooldown = 35f;
            abilityModel.RemoveBehaviors<CreateSoundOnAbilityModel>();
            abilityModel.RemoveBehaviors<CreateEffectOnAbilityModel>();


            var activateAttackModel = new ActivateAttackModel("ActivateAttackModel_MissileBarrage", 5, true, new Il2CppReferenceArray<AttackModel>(1), true, false, false, false, false);
            abilityModel.AddBehavior(activateAttackModel);

            var attackModel = activateAttackModel.attacks[0] = Game.instance.model.GetTower(TowerType.MortarMonkey, 0, 5).GetAbility().GetBehavior<ActivateAttackModel>().attacks[0].Duplicate();

            attackModel.attackThroughWalls = true;
            attackModel.range = 2000;
            activateAttackModel.AddChildDependant(attackModel);
            var weaponModel = attackModel.weapons[0];
            weaponModel.rate = 0.025f;
            weaponModel.emission = new RandomTargetSpreadModel("Spread", 200, null, null);
            weaponModel.SetProjectile(Game.instance.model.GetTower(TowerType.MonkeySub, 0, 3).GetAttackModel(1).weapons[0].projectile.Duplicate());
            var projectileModel = weaponModel.projectile;
            projectileModel.display = Game.instance.model.GetTower(TowerType.DartlingGunner, 5).GetAttackModel().weapons[0].projectile.display;
            projectileModel.GetBehavior<CreateProjectileOnExpireModel>().projectile.GetDamageModel().damage = 10;
            projectileModel.GetBehavior<CreateProjectileOnExpireModel>().projectile.radius = Game.instance.model.GetTower(TowerType.BombShooter).GetAttackModel().weapons[0].projectile.GetBehavior<CreateProjectileOnContactModel>().projectile.radius;
            projectileModel.GetBehavior<Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors.CreateEffectOnExpireModel>().effectModel = Game.instance.model.GetTower(TowerType.BombShooter).GetAttackModel().weapons[0].projectile.GetBehavior<CreateEffectOnContactModel>().effectModel;
            projectileModel.GetBehavior<CreateProjectileOnExpireModel>().projectile.GetDamageModel().immuneBloonProperties = BloonProperties.None;
            towerModel.AddBehavior(abilityModel);
            projectileModel.GetBehavior<CreateProjectileOnExpireModel>().GetDescendants<FilterInvisibleModel>().ForEach(model => model.isActive = false);
        }
    }
    public class L4 : ModHeroLevel<Lynn>
    {
        public override string Description => "Lynn's wrench and sentries deal more damage";
        public override int Level => 4;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            towerModel.GetAttackModel().weapons[0].projectile.GetDamageModel().damage += 2;

            foreach (var attack in towerModel.GetAttackModels())
            {
                if (attack.name.Contains("Sentry_Place"))
                {
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].projectile.GetDamageModel().damage += 1;
                }
            }
        }
    }
    public class L5 : ModHeroLevel<Lynn>
    {
        public override string Description => "Lynn periodically creates sensor beacons that allow nearby towers to detect Camo Bloons.";
        public override int Level => 5;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var behavior in Game.instance.model.GetTowerFromId("EngineerMonkey-100").GetAttackModels().ToArray())
            {
                if (behavior.name.Contains("Spawner"))
                {
                    var spawner = behavior.Duplicate();
                    spawner.name = "Beacon_Place";
                    spawner.weapons[0].rate = 30;
                    spawner.weapons[0].projectile.RemoveBehavior<CreateTowerModel>();
                    spawner.weapons[0].projectile.AddBehavior(new CreateTowerModel("BeaconPlace", GetTowerModel<Beacon>().Duplicate(), 0f, true, false, false, true, true));

                    towerModel.AddBehavior(spawner);
                }
            }
        }
    }
    public class L6 : ModHeroLevel<Lynn>
    {
        public override string Description => "Sentries atatck faster.";
        public override int Level => 6;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var attack in towerModel.GetAttackModels())
            {
                if (attack.name.Contains("Sentry_Place"))
                {
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].rate /= 1.3f;
                }
            }
        }
    }
    public class L7 : ModHeroLevel<Lynn>
    {
        public override string Description => "Demolition: Lynn’s wrench and sentries deal bonus damage to MOAB Bloons.";
        public override int Level => 7;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            towerModel.GetAttackModel().weapons[0].projectile.hasDamageModifiers = true;
            towerModel.GetAttackModel().weapons[0].projectile.AddBehavior(new DamageModifierForTagModel("DemolitionModifier_", "Moabs", 1, 8, false, false) { name = "DemolitionModifier_" });


            foreach (var attack in towerModel.GetAttackModels())
            {
                if (attack.name.Contains("Sentry_Place"))
                {
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].projectile.hasDamageModifiers = true;
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].projectile.AddBehavior
                        (new DamageModifierForTagModel("DemolitionModifier_", "Moabs", 1, 2, false, false) { name = "DemolitionModifier_" });
                }
            }
        }
    }
    public class L8 : ModHeroLevel<Lynn>
    {
        public override string Description => "Screw Dive deals increased damage and sentries fire two shots at a time.";
        public override int Level => 8;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var attack in towerModel.GetAttackModels())
            {
                if (attack.name.Contains("Sentry_Place"))
                {
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].emission = new ArcEmissionModel("aaa", 2, 0, 10, null, false, true);
                }
            }


            towerModel.GetAbility(0).GetBehavior<ActivateAttackModel>().attacks[0].weapons[0].projectile.GetBehavior<CreateProjectileOnExpireModel>().projectile.GetDamageModel().damage *= 2;
        }
    }
    public class L9 : ModHeroLevel<Lynn>
    {
        public override string Description => "Helping Hand buff increased to 15% bonus attack speed and Lynn attacks faster.";
        public override int Level => 9;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            towerModel.GetBehavior<RateSupportModel>().multiplier = 0.85f;

            towerModel.GetAttackModel().weapons[0].rate /= 1.3f;
        }
    }
    public class L10 : ModHeroLevel<Lynn>
    {
        public override string Portrait => "LynnL10-Portrait";
        public override string AbilityName => "On Site Construction ability";
        public override string AbilityDescription => "Lynn builds temporary brick walls on the track that block and stun Bloons that hit them.";
        public override string Description => $"{AbilityName}: {AbilityDescription}";
        public override int Level => 10;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            var knockback = Game.instance.model.GetTower(TowerType.BombShooter, 5).GetAttackModel().weapons[0].projectile.GetBehavior<CreateProjectileOnContactModel>().projectile.GetBehavior<PushBackModel>();
            knockback.onlyIfDamaged = false;

            var stun = Game.instance.model.GetTower(TowerType.BombShooter, 5).GetAttackModel().weapons[0].projectile.GetBehavior<CreateProjectileOnContactModel>().projectile.GetBehavior<SlowModel>();
            stun.Lifespan = 1.5f;

            var wallAbility = Game.instance.model.GetTower(TowerType.SpikeFactory, 0, 4).GetAbility().Duplicate();
            wallAbility.name = "AbilityModel_Construction";
            wallAbility.icon = GetSpriteReference<EngineerHeroLynn>("Construction-Icon");
            wallAbility.displayName = "On Site Construction";
            wallAbility.description = "Builds temporary brick walls on the track that block and stun Bloons that hit them.";
            wallAbility.cooldown = 60f;

            foreach (var behavior in wallAbility.GetBehavior<ActivateAttackModel>().attacks)
            {
                var wall = behavior.weapons[0].projectile;

                behavior.weapons[0].rate = Game.instance.model.GetTower(TowerType.SpikeFactory, 0, 2).GetAttackModel().weapons[0].rate;
                wall.RemoveBehavior<SetSpriteFromPierceModel>();
                wall.ApplyDisplay<BrickWall>();
                wall.GetDamageModel().damage = 0;
                wall.GetDamageModel().immuneBloonProperties = BloonProperties.None;
                wall.pierce = 50;
                wall.AddBehavior(knockback);
                wall.AddBehavior(stun);
                wall.collisionPasses = new int[] { 0, -1 };
            }

            towerModel.AddBehavior(wallAbility);
        }
    }
    public class L11 : ModHeroLevel<Lynn>
    {
        public override string Description => "Lynn's wrench and sentries deal even more damage and can pop all Bloon types.";
        public override int Level => 11;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            towerModel.GetAttackModel().weapons[0].projectile.GetDamageModel().damage += 2;
            towerModel.GetAttackModel().weapons[0].projectile.GetDamageModel().immuneBloonProperties = BloonProperties.None;


            foreach (var attack in towerModel.GetAttackModels())
            {
                if (attack.name.Contains("Sentry_Place"))
                {
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].projectile.GetDamageModel().damage += 1;
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].projectile.GetDamageModel().immuneBloonProperties = BloonProperties.None;
                }
            }
        }
    }
    public class L12 : ModHeroLevel<Lynn>
    {
        public override string Description => "Sentries attack really fast.";
        public override int Level => 12;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var attack in towerModel.GetAttackModels())
            {
                if (attack.name.Contains("Sentry_Place"))
                {
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].rate /= 1.5f;
                }
            }
        }
    }
    public class L13 : ModHeroLevel<Lynn>
    {
        public override string Description => "Helping Hand buff gives engineer monkeys increased range and pierce. Bonus MOAB damage is increased.";
        public override int Level => 13;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var behavior in towerModel.GetAttackModel().weapons[0].projectile.GetBehaviors<DamageModifierForTagModel>().ToArray())
            {
                if (behavior.name.Contains("DemolitionModifier_"))
                {
                    behavior.damageAddative += 2;
                    behavior.damageMultiplier = 2;
                }
            }


            foreach (var attack in towerModel.GetAttackModels())
            {
                if (attack.name.Contains("Sentry_Place"))
                {
                    foreach (var behavior in attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].projectile.GetBehaviors<DamageModifierForTagModel>().ToArray())
                    {
                        if (behavior.name.Contains("DemolitionModifier_"))
                        {
                            behavior.damageAddative *= 2;
                        }
                    }
                }
            }


            TowerFilterModel engineer = new FilterInBaseTowerIdModel("BaseTowerFilter", new string[] { TowerType.EngineerMonkey });

            towerModel.AddBehavior(new RangeSupportModel("HelpingHandRange", true, 0.15f, 0, "HelpingHandRangeBuff", new Il2CppReferenceArray<TowerFilterModel>(new TowerFilterModel[] { engineer }), false, "", ""));
            towerModel.AddBehavior(new PierceSupportModel("HelpingHandPierce", true, 2, "HelpingHandPierceBuff", new Il2CppReferenceArray<TowerFilterModel>(new TowerFilterModel[] { engineer }), false, "", ""));
        }
    }
    public class L14 : ModHeroLevel<Lynn>
    {
        public override string Description => "Sentries now fire three shots at a time.";
        public override int Level => 14;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var attack in towerModel.GetAttackModels())
            {
                if (attack.name.Contains("Sentry_Place"))
                {
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].emission = new ArcEmissionModel("aaa", 3, 0, 15, null, false, false);
                }
            }
        }
    }
    public class L15 : ModHeroLevel<Lynn>
    {
        public override string Description => "Sensor beacons have much greater range and give towers bonus damage to camo Bloons.";
        public override int Level => 15;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var attack in towerModel.GetAttackModels())
            {
                if (attack.name.Contains("Beacon_Place"))
                {
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.range += 16;
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().range += 16;

                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.AddBehavior
                        (new DamageModifierSupportModel("", true, "CamoModifierSupportZone", null, false, new DamageModifierForTagModel("aaa", "Camo", 1, 2, false, false)));
                }
            }
        }
    }
    public class L16 : ModHeroLevel<Lynn>
    {
        public override string Description => "On Site Construction builds more walls and walls are more durable.";
        public override int Level => 16;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var behavior in towerModel.GetAbility(1).GetBehavior<ActivateAttackModel>().attacks)
            {
                behavior.weapons[0].rate /= 2f;
                behavior.weapons[0].projectile.pierce *= 1.5f;
            }
        }
    }
    public class L17 : ModHeroLevel<Lynn>
    {
        public override string Description => "Screw Dive deals even more damage and bonus MOAB damage is increased further.";
        public override int Level => 17;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            towerModel.GetAbility(0).GetBehavior<ActivateAttackModel>().attacks[0].weapons[0].projectile.GetBehavior<CreateProjectileOnExpireModel>().projectile.GetDamageModel().damage *= 2;


            foreach (var behavior in towerModel.GetAttackModel().weapons[0].projectile.GetBehaviors<DamageModifierForTagModel>().ToArray())
            {
                if (behavior.name.Contains("DemolitionModifier_"))
                {
                    behavior.damageAddative += 2;
                    behavior.damageMultiplier = 3;
                }
            }


            foreach (var attack in towerModel.GetAttackModels())
            {
                if (attack.name.Contains("Sentry_Place"))
                {
                    foreach (var behavior in attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].projectile.GetBehaviors<DamageModifierForTagModel>().ToArray())
                    {
                        if (behavior.name.Contains("DemolitionModifier_"))
                        {
                            behavior.damageMultiplier = 2;
                        }
                    }
                }
            }
        }
    }
    public class L18 : ModHeroLevel<Lynn>
    {
        public override string Description => "Helping Hand buff now gives 30% bonus attack speed and 2 additional damage.";
        public override int Level => 18;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            towerModel.GetBehavior<RateSupportModel>().multiplier = 0.70f;


            TowerFilterModel engineer = new FilterInBaseTowerIdModel("BaseTowerFilter", new string[] { TowerType.EngineerMonkey });

            towerModel.AddBehavior(new DamageSupportModel("HelpingHandDamage", true, 2, "HelpingHandDamageBuff", new Il2CppReferenceArray<TowerFilterModel>(new TowerFilterModel[] { engineer }), false, false, 0));
        }
    }
    public class L19 : ModHeroLevel<Lynn>
    {
        public override string Description => "Lynn attacks much faster and sentries fire four shots at a time.";
        public override int Level => 19;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            towerModel.GetAttackModel().weapons[0].rate /= 1.5f;


            foreach (var attack in towerModel.GetAttackModels())
            {
                if (attack.name.Contains("Sentry_Place"))
                {
                    attack.weapons[0].projectile.GetBehavior<CreateTowerModel>().tower.GetAttackModel().weapons[0].emission = new ArcEmissionModel("aaa", 4, 0, 20, null, false, false);
                }
            }
        }
    }
    public class L20 : ModHeroLevel<Lynn>
    {
        public override string Portrait => "LynnL20-Portrait";
        public override string Description => "Ability cooldowns are drastically reduced. On Site Construction builds even more walls and walls stun for longer.";
        public override int Level => 20;

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var ability in towerModel.GetAbilities())
            {
                ability.cooldown /= 1.35f;
            }

            foreach (var behavior in towerModel.GetAbility(1).GetBehavior<ActivateAttackModel>().attacks)
            {
                behavior.weapons[0].rate /= 2f;
                behavior.weapons[0].projectile.GetBehavior<SlowModel>().Lifespan += 1;
            }
        }
    }
}

public class WorkerSentry : ModTower
{
    public override string Portrait => "WorkerSentry-Portrait";
    public override string Name => "WorkerSentry";
    public override TowerSet TowerSet => TowerSet.Support;
    public override string BaseTower => TowerType.Sentry;

    public override bool DontAddToShop => true;
    public override int Cost => 0;

    public override int TopPathUpgrades => 0;
    public override int MiddlePathUpgrades => 0;
    public override int BottomPathUpgrades => 0;

    public override string DisplayName => "Construction Sentry";

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
        towerModel.GetAttackModel().weapons[0].projectile.GetDamageModel().immuneBloonProperties = BloonProperties.Lead | BloonProperties.Frozen;
        towerModel.GetAttackModel().weapons[0].projectile.GetDamageModel().damage = 2;
        towerModel.GetAttackModel().weapons[0].rate /= 1.25f;

        towerModel.isSubTower = true;
        towerModel.AddBehavior(new TowerExpireModel("ExpireModel", 20f, 5, false, false));

        towerModel.displayScale = 0.8f;
        towerModel.display = new() { guidRef = "af6bc5f76310fa84eae188f2f5381dc6" };
        towerModel.GetAttackModel().behaviors.First(a => a.name == "DisplayModel_AttackDisplay").Cast<DisplayModel>().display = new() { guidRef = "95f0e98e9602cab4fb4f1dcc6d01653a" };
    }

    public class SentryDisplay : ModTowerDisplay<WorkerSentry>
    {
        public override float Scale => 1f;
        public override string BaseDisplay => GetDisplay(TowerType.SentryCrushing);

        public override bool UseForTower(int[] tiers)
        {
            return true;
        }
        public override void ModifyDisplayNode(UnityDisplayNode node)
        {

        }
    }
}

public class Beacon : ModTower
{
    public override string Portrait => "Beacon-Portrait";
    public override string Name => "SensorBeacon";
    public override TowerSet TowerSet => TowerSet.Support;
    public override string BaseTower => TowerType.MonkeyVillage;

    public override bool DontAddToShop => true;
    public override int Cost => 0;

    public override int TopPathUpgrades => 0;
    public override int MiddlePathUpgrades => 0;
    public override int BottomPathUpgrades => 0;

    public override string DisplayName => "Sensor Beacon";

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
        towerModel.RemoveBehavior<RangeSupportModel>();
        towerModel.AddBehavior(new VisibilitySupportModel("", true, "VisionSupportZone", false, null, "", ""));

        towerModel.range = 54;
        towerModel.GetAttackModel().range = 54;
        towerModel.radius /= 2;

        towerModel.isSubTower = true;
        towerModel.AddBehavior(new TowerExpireModel("ExpireModel", 25f, 5, false, false));
    }

    public class BeaconDisplay : ModTowerDisplay<Beacon>
    {
        public override float Scale => 0.5f;
        public override string BaseDisplay => GetDisplay(TowerType.MonkeyVillage, 0, 2);

        public override bool UseForTower(int[] tiers)
        {
            return true;
        }
        public override void ModifyDisplayNode(UnityDisplayNode node)
        {

        }
    }
}
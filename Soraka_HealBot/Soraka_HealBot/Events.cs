﻿using EloBuddy;
using EloBuddy.SDK;
using Soraka_HealBot.Extensions;

namespace Soraka_HealBot
{
    internal static class Events
    {
        internal static void SavingGrace(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || args.Target == null || sender.IsMe)
            {
                return;
            }
            var enemy = sender as AIHeroClient;
            var ally = args.Target as AIHeroClient;
            if (enemy == null || !enemy.IsEnemy || ally == null || !ally.IsAlly)
            {
                return;
            }
            var enemyDmg = enemy.GetSpellDamage(ally, args.Slot);
            if (ally.Health > enemyDmg || ally.Health.Equals(ally.MaxHealth))
            {
                return;
            }
            if (Config.IsChecked(Config.AutoWMenu, "autoW") && Spells.W.CanCast() &&
                ally.Distance(Player.Instance) <= Spells.W.Range)
            {
                if (ally.Health + Spells.GetWHeal() > enemyDmg && ally.Health <= enemyDmg)
                {
                    Spells.W.Cast(ally);
                }
            }
            if (Config.IsChecked(Config.AutoRMenu, "autoR") && Spells.R.CanCast())
            {
                if (ally.Health + Spells.GetUltHeal(ally) > enemyDmg && ally.Health <= enemyDmg)
                {
                    Spells.R.Cast();
                }
            }
        }

        internal static void OnBeforeAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (args.Target.Type == GameObjectType.obj_AI_Minion &&
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) &&
                Config.IsChecked(Config.Harass, "disableAAH"))
            {
                var air = Player.Instance.CountAlliesInRange(Config.GetSliderValue(Config.Harass, "allyRangeH"));
                if (air > 1)
                {
                    args.Process = false;
                }
            }
            if ((Config.IsChecked(Config.Combo, "comboDisableAA") ||
                 (Config.IsChecked(Config.Combo, "bLvlDisableAA") &&
                  Player.Instance.Level >= Config.GetSliderValue(Config.Combo, "lvlDisableAA"))) &&
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) &&
                args.Target.Type == GameObjectType.AIHeroClient)
            {
                args.Process = false;
            }
        }
    }
}
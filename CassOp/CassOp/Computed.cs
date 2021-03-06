﻿namespace CassOp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;

    using SharpDX;

    internal static class Computed
    {
        #region Constants

        private const int Archangels = (int)ItemId.Archangels_Staff;

        private const int Archangels2 = (int)ItemId.Archangels_Staff_Crystal_Scar;

        private const int Manamune = (int)ItemId.Manamune;

        private const int Manamune2 = (int)ItemId.Manamune_Crystal_Scar;

        private const int Muramana = (int)ItemId.Muramana; // 3042

        private const int Seraph = (int)ItemId.Seraphs_Embrace; // 3040

        private const int Tear = (int)ItemId.Tear_of_the_Goddess;

        private const int Tear2 = (int)ItemId.Tear_of_the_Goddess_Crystal_Scar;

        #endregion

        #region Public Properties

        public static float TurretRange => 775;

        #endregion

        #region Properties

        private static AIHeroClient Player => EloBuddy.Player.Instance;

        #endregion

        #region Public Methods and Operators

        public static void AssistedR()
        {
            if (!Config.IsKeyPressed(Config.Misc, "assistedR") || !Spells.R.IsReady())
            {
                return;
            }

            var enemyNearMouse =
                EntityManager.Heroes.Enemies.OrderBy(entity => entity.Distance(Game.CursorPos, true))
                    .FirstOrDefault(entity => entity.IsValidTarget(Spells.R.Range));
            if (enemyNearMouse != null)
            {
                Spells.R.Cast(enemyNearMouse);
            }
        }

        public static void AutoClearE()
        {
            if (!Spells.E.IsReady() && (Orbwalker.LastHitMinion != null && Orbwalker.IsAutoAttacking))
            {
                return;
            }

            var minionToE =
                EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(
                    x =>
                    x.IsValidTarget(Spells.E.Range) && Player.GetSpellDamage(x, SpellSlot.E) >= x.Health
                    && x.HasBuffOfType(BuffType.Poison));
            if (minionToE != null)
            {
                Spells.E.Cast(minionToE);
            }
        }

        public static void AutoE()
        {
            if (!Spells.E.IsReady() && (Orbwalker.LastHitMinion != null && Orbwalker.IsAutoAttacking))
            {
                return;
            }

            var bush =
                ObjectManager.Get<GrassObject>().OrderBy(br => br.Distance(Player.ServerPosition)).FirstOrDefault();
            if (bush != null
                && (Config.IsChecked(Config.Harass, "dontAutoHarassInBush")
                    && Player.ServerPosition.IsInRange(bush, bush.BoundingRadius)))
            {
                return;
            }

            var target = TargetSelector.GetTarget(Spells.E.Range, DamageType.Magical);
            if (target == null || !target.IsValidTarget(Spells.E.Range))
            {
                return;
            }

            if (Config.IsChecked(Config.Harass, "dontAutoHarassTower") && UnderEnemyTurret(target.Position)
                && UnderEnemyTurret(Player.Position))
            {
                return;
            }

            if (Spells.E.IsReady() && target.IsValidTarget(Spells.E.Range)
                && (!Config.IsChecked(Config.Harass, "autoHarassEonP") || target.HasBuffOfType(BuffType.Poison)))
            {
                if (Config.IsChecked(Config.Harass, "humanEInAutoHarass"))
                {
                    var delay = RandomDelay(Config.GetSliderValue(Config.Misc, "humanDelay"));
                    Core.DelayAction(() => Spells.E.Cast(target), delay);
                }
                else
                {
                    Spells.E.Cast(target);
                }
            }
        }

        public static void AutoQ()
        {
            if (!Spells.Q.IsReady() && (Orbwalker.LastHitMinion != null && Orbwalker.IsAutoAttacking))
            {
                return;
            }

            var bush =
                ObjectManager.Get<GrassObject>().OrderBy(br => br.Distance(Player.ServerPosition)).FirstOrDefault();
            if (bush != null
                && (Config.IsChecked(Config.Harass, "dontAutoHarassInBush")
                    && Player.ServerPosition.IsInRange(bush, bush.BoundingRadius)))
            {
                return;
            }

            var target = TargetSelector.GetTarget(Spells.Q.Range, DamageType.Magical);
            if (target == null || !target.IsValidTarget(Spells.Q.Range))
            {
                return;
            }

            if (Config.IsChecked(Config.Harass, "dontAutoHarassTower") && UnderEnemyTurret(target.Position)
                && UnderEnemyTurret(Player.Position))
            {
                return;
            }

            if (Spells.Q.IsReady() && !target.HasBuffOfType(BuffType.Poison))
            {
                var qPred = Spells.Q.GetPrediction(target);
                if (qPred.HitChancePercent >= 95)
                {
                    Spells.Q.Cast(qPred.CastPosition);
                }
            }
        }

        public static void AutoW()
        {
            if (!Spells.W.IsReady() && (Orbwalker.LastHitMinion != null && Orbwalker.IsAutoAttacking))
            {
                return;
            }

            var bush =
                ObjectManager.Get<GrassObject>().OrderBy(br => br.Distance(Player.ServerPosition)).FirstOrDefault();
            if (bush != null
                && (Config.IsChecked(Config.Harass, "dontAutoHarassInBush")
                    && Player.ServerPosition.IsInRange(bush, bush.BoundingRadius)))
            {
                return;
            }

            var target = TargetSelector.GetTarget(Spells.W.Range, DamageType.Magical);
            if (target == null || !target.IsValidTarget(Spells.W.Range))
            {
                return;
            }

            if (Config.IsChecked(Config.Harass, "dontAutoHarassTower") && UnderEnemyTurret(target.Position)
                && UnderEnemyTurret(Player.Position))
            {
                return;
            }

            if (Spells.W.IsReady() && target.IsValidTarget(Spells.W.Range))
            {
                var wPred = Spells.W.GetPrediction(target);
                if (wPred.CastPosition.Distance(EloBuddy.Player.Instance.Position) >= 550
                    && !wPred.CastPosition.IsWall() && wPred.HitChancePercent >= 85)
                {
                    Spells.W.Cast(wPred.CastPosition);
                }
            }
        }

        public static float ComboDmg(Obj_AI_Base target)
        {
            var dmg = 0f;
            if (Spells.Q.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (Spells.W.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (Spells.E.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.E) * 2;
            }

            if (Spells.R.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.R);
            }

            dmg += Player.GetAutoAttackDamage(target);
            return dmg;
        }

        public static bool CompletedTear()
        {
            return Item.HasItem(Muramana) || Item.HasItem(Seraph);
        }

        /*
        Stolen from:
            https://github.com/mrarticuno/Elobuddy/blob/master/Cassioloira/OneForWeek/Util/Misc/Misc.cs
        */

        public static FarmLocation GetBestCircularFarmLocation(
            List<Vector2> minionPositions,
            float width,
            float range,
            int useMecMax = 9)
        {
            var result = new Vector2();
            var minionCount = 0;
            var startPos = EloBuddy.Player.Instance.ServerPosition.To2D();

            range = range * range;

            if (minionPositions.Count == 0)
            {
                return new FarmLocation(result, minionCount);
            }

            /* Use MEC to get the best positions only when there are less than 9 positions because it causes lag with more. */
            if (minionPositions.Count <= useMecMax)
            {
                var subGroups = GetCombinations(minionPositions);
                foreach (var subGroup in subGroups)
                {
                    if (subGroup.Count > 0)
                    {
                        var circle = Mec.GetMec(subGroup);

                        if (circle.Radius <= width && circle.Center.Distance(startPos, true) <= range)
                        {
                            minionCount = subGroup.Count;
                            return new FarmLocation(circle.Center, minionCount);
                        }
                    }
                }
            }
            else
            {
                foreach (var pos in minionPositions)
                {
                    if (pos.Distance(startPos, true) <= range)
                    {
                        var count = minionPositions.Count(pos2 => pos.Distance(pos2, true) <= width * width);

                        if (count >= minionCount)
                        {
                            result = pos;
                            minionCount = count;
                        }
                    }
                }
            }

            return new FarmLocation(result, minionCount);
        }

        public static bool HasTear()
        {
            return (Item.HasItem(Tear) || Item.HasItem(Tear2)) || (Item.HasItem(Manamune) || Item.HasItem(Manamune2))
                   || Item.HasItem(Archangels) || Item.HasItem(Archangels2);
        }

        public static void KillSteal(string mode)
        {
            if (mode == "E" && Spells.E.IsReady())
            {
                var entKs =
                    EntityManager.Heroes.Enemies.FirstOrDefault(
                        h => h.IsValidTarget(Spells.E.Range) && h.Health < Player.GetSpellDamage(h, SpellSlot.E));
                if (entKs != null)
                {
                    Spells.E.Cast(entKs);
                }
            }
        }

        public static void OnBeforeAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (Config.IsChecked(Config.Combo, "comboNoAA")
                && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)
                && args.Target.Type == GameObjectType.AIHeroClient)
            {
                args.Process = false;
            }
        }

        public static void OnSpellbookCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R && Config.IsChecked(Config.Misc, "antiMissR")
                && !Spells.FlashR)
            {
                var facingEns =
                    EntityManager.Heroes.Enemies.Where(h => h.IsValidTarget(Spells.R.Range) && h.IsFacing(Player));
                if (!facingEns.Any())
                {
                    args.Process = false;
                }
            }

            if (sender.Owner.IsMe)
            {
                switch (args.Slot)
                {
                    case SpellSlot.Q:
                        Spells.QCasted = Game.Time;
                        Spells.LastQPos = args.EndPosition;
                        break;
                    case SpellSlot.W:
                        Spells.WCasted = Game.Time;
                        Spells.LastWPos = args.EndPosition;
                        break;
                    case SpellSlot.E:
                        Spells.ECasted = Game.Time;
                        break;
                }
            }
        }

        public static void OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "SummonerFlash" && Spells.FlashR)
                {
                    Spells.FlashR = false;
                }
            }
        }

        public static void OnUnkillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if (target == null)
            {
                return;
            }

            var eTravelTime = target.Distance(EloBuddy.Player.Instance) / Spells.E.Handle.SData.MissileSpeed
                              + Spells.E.CastDelay + Game.Ping / 2f / 1000;
            if (Config.IsChecked(Config.Misc, "eLastHit") && Player.GetSpellDamage(target, SpellSlot.E) > target.Health
                && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)
                && Prediction.Health.GetPrediction(target, (int)eTravelTime * 1000) > 0)
            {
                Spells.E.Cast(target);
            }
        }

        public static int RandomDelay(int x)
        {
            var y = x;
            var i = Math.Abs(x);
            while (i >= 10)
            {
                i /= 10;
            }

            i = y / i;
            return Mainframe.RDelay.Next(y - i, y + i);
        }

        public static void TearStack()
        {
            if (!Spells.Q.IsReady() || Player.IsRecalling())
            {
                return;
            }

            var enemyTarget = TargetSelector.GetTarget(Spells.Q.Range + 200, DamageType.Magical);
            if (enemyTarget != null)
            {
                Spells.Q.Cast(enemyTarget);
            }
            else
            {
                var minions = EntityManager.MinionsAndMonsters.EnemyMinions;
                var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
                var qFarmLoc =
                    GetBestCircularFarmLocation(
                        objAiMinions.Where(m => m.Distance(EloBuddy.Player.Instance) <= Spells.Q.Range)
                            .Select(mx => mx.ServerPosition.To2D())
                            .ToList(),
                        Spells.Q.Width,
                        Spells.Q.Range);
                if (qFarmLoc.MinionsHit >= 1)
                {
                    Spells.Q.Cast(qFarmLoc.Position.To3D());
                }
                else
                {
                    var rndPlayerPos = Player.Position;
                    rndPlayerPos.X += Mainframe.RDelay.Next(-200, 200);
                    rndPlayerPos.Y += Mainframe.RDelay.Next(-200, 200);
                    Core.DelayAction(() => Spells.Q.Cast(rndPlayerPos), Mainframe.RDelay.Next(30, 70));
                }
            }
        }

        public static bool UnderEnemyTurret(this Vector3 position)
        {
            return EntityManager.Turrets.Enemies.Any(turret => turret.Distance(position) < TurretRange);
        }

        #endregion

        #region Methods

        private static List<List<Vector2>> GetCombinations(List<Vector2> allValues)
        {
            var collection = new List<List<Vector2>>();
            for (var counter = 0; counter < (1 << allValues.Count); ++counter)
            {
                var combination = allValues.Where((t, i) => (counter & (1 << i)) == 0).ToList();

                collection.Add(combination);
            }

            return collection;
        }

        #endregion

        public struct FarmLocation
        {
            #region Fields

            public int MinionsHit;

            public Vector2 Position;

            #endregion

            #region Constructors and Destructors

            public FarmLocation(Vector2 position, int minionsHit)
            {
                this.Position = position;
                this.MinionsHit = minionsHit;
            }

            #endregion
        }
    }
}
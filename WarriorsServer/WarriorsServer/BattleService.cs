using System;
using Utils;

namespace WarriorsServer
{
    public static class BattleService
    {
        public static void ExecuteFight(Army army1, Army army2)
        {
            // Calculate potential damage to each army, must calculate both before applying damage
            var damageToArmy1 = CalculateDamage(army2, army1);
            var damageToArmy2 = CalculateDamage(army1, army2);

            // Apply damage to both armies
            ApplyDamage(army1, damageToArmy1);
            ApplyDamage(army2, damageToArmy2);
        }

        private static (double archerDamage, double PikemanDamage, double knightDamage) CalculateDamage(Army attArmy, Army defArmy)
        {
            double totalDefenders = defArmy.ArcherCount + defArmy.PikemanCount + defArmy.KnightCount;
            double defArcherPercentage = defArmy.ArcherCount / totalDefenders;
            double defPikemanPercentage = defArmy.PikemanCount / totalDefenders;
            double defKnightPercentage = defArmy.KnightCount / totalDefenders;

            double damageFromArchers = attArmy.ArcherCount * Archer.Damage;
            double damageFromPikemans = attArmy.PikemanCount * Pikeman.Damage;
            double damageFromKnights = attArmy.KnightCount * Knight.Damage;

            double archerDamage = ComputeReceivedDamage(defArcherPercentage, damageFromArchers, damageFromPikemans * 0.5, damageFromKnights * 2);
            double PikemanDamage = ComputeReceivedDamage(defPikemanPercentage, damageFromArchers * 2, damageFromPikemans, damageFromKnights * 0.5);
            double knightDamage = ComputeReceivedDamage(defKnightPercentage, damageFromArchers * 0.5, damageFromPikemans * 2, damageFromKnights);

            return (archerDamage, PikemanDamage, knightDamage);
        }

        private static void ApplyDamage(Army army, (double archerDamage, double PikemanDamage, double knightDamage) damage)
        {
            army.SetArcherCount( Math.Max(0, army.ArcherCount - (int)Math.Round(damage.archerDamage / Archer.Health)) );
            army.SetPikemanCount( Math.Max(0, army.PikemanCount - (int)Math.Round(damage.PikemanDamage / Pikeman.Health)) );
            army.SetKnightCount( Math.Max(0, army.KnightCount - (int)Math.Round(damage.knightDamage / Knight.Health)) );
        }

        private static double ComputeReceivedDamage( double unitPercent, double archerMultiplier, double PikemanMultiplier, double knightMultiplier)
        {
            //calculate damage based on troop % and its damage among the defender three types 
            //( ex. damage to defender that are archers, with 30% (archer percent) * each of attacker units )
            return (unitPercent * archerMultiplier) + (unitPercent * PikemanMultiplier) + (unitPercent * knightMultiplier);
        }
    }
}

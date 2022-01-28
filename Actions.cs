namespace MTG_ConsoleEngine
{
    public class Actions{
        public static void DealDamageToBothPlayers(int value, Player cardOwner){
            int firstTarget = cardOwner.ID == 1?1:0; 

            Engine.Players[firstTarget].DealDamage(value);
            cardOwner.DealDamage(value);
        }
        
        public static void AddMana(int value, string colorCode, Player owner)
        {
            Console.WriteLine("adding mana:" + value + " "+colorCode);
        }
    }

}
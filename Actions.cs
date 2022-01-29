using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public class Actions{
        public static void DealDamageToBothPlayers(int value, Player cardOwner){
            int firstTarget = cardOwner.ID == 1?1:0; 

            Engine.Players[firstTarget].DealDamage(value);
            cardOwner.DealDamage(value);
        }
        public static void PlayLandCard(int value, string colorCode, Player owner)
        {
            Console.WriteLine("dodano karte many do puli ManaField`u");
            
            owner.IsLandPlayedThisTurn = true;
        }
        public static void Haste(Creature creatureCard)
        {
            // karta jest aktywna orazu po wprowadzeniu jej na pole / mozna nią odrazu atakowac lub uzyc umiejetnosci
            Console.WriteLine("Karta lądując na polu staje sie aktywna ( Untapped )");
            creatureCard.isTapped = false;
        }
        public static void Lifelink(int value, Player owner)
        {
            // leczenie po zadaniu obrażeń 
            Console.WriteLine($"Leczysz sie za {value}.");
            owner.Heal(value);
        }
        public static void AssingExtraValuesToCreature(int hp, int atk, Creature? target)
        {
            if(target != null)
            {
            // max wartość któą odzyskaja w nowej turze:
                target.BaseAttack += atk;
                target.BaseHealth += hp;
                
                Console.WriteLine($"{target.Name} po nałożeniu enchantu: atk:{target.CurrentAttack} / hp:{target.CurrentHealth}");
            }
        }
    }
}
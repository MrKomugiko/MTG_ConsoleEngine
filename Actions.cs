using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public class Actions{
        public static void DealDamageToBothPlayers(int value, PlayerBase cardOwner){
            cardOwner.Opponent.DealDamage(value);
            cardOwner.DealDamage(value);
        }
        public static void PlayLandCard(int value, string colorCode, PlayerBase owner)
        {
            //Console.WriteLine("dodano karte many do puli ManaField`u");
            
            owner.IsLandPlayedThisTurn = true;
        }
        public static void Haste(Creature creatureCard)
        {
            // karta jest aktywna orazu po wprowadzeniu jej na pole / mozna nią odrazu atakowac lub uzyc umiejetnosci
            //Console.WriteLine("Aktywowanie efektu Haste: Karta wkraca na pole bez efektu osłabienia\n(można nią zaatakować w tej samej turze)");
            creatureCard.IsTapped = false;
        }
        public static void Heal(int value, PlayerBase owner)
        {
            // leczenie po zadaniu obrażeń 
            //Console.WriteLine($"Leczysz sie za {value}.");
            owner.Heal(value);
        }
        public static void AssingExtraValuesToCreature(int hp, int atk, Creature? target)
        {
            if(target != null)
            {
            // max wartość któą odzyskaja w nowej turze:
                target.BaseAttack += atk;
                target.BaseHealth += hp;
                
                //Console.WriteLine($"{target.Name} po nałożeniu enchantu: atk:{target.CurrentAttack} / hp:{target.CurrentHealth}");
            }
        }
        public static void DamageSelectedCreature(int value, Creature target)
        {
            //Console.WriteLine($"Damage {target.Name} with {value} damage");
            target.CurrentHealth -= value;
        }
    }
}
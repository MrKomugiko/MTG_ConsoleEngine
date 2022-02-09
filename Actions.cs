using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public class Actions{
        public static void DealDamageToBothPlayers(int value, PlayerBase cardOwner){
            cardOwner.Opponent.DealDamage(value);
            cardOwner.DealDamage(value);
        }
        public static void PlayLandCard(PlayerBase owner)
        {
            //Console.WriteLine("dodano karte many do puli ManaField`u w tej truze juz nie mozesz dodac kolejnej");
           
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
               ///* DEBUG INFO */ Console.WriteLine($"\t\t{target.Name} po nałożeniu enchantu: atk:{target.CurrentAttack+atk} / hp:{target.CurrentHealth+hp}");
                target.BaseAttack += atk;
                target.BaseHealth += hp;

            }
        }
        public static void DamageSelectedCreature(int value, Creature target)
        {
           ///* DEBUG INFO */ Console.WriteLine($"\t\t{target.Name} po otrzymaniu {value} dmg   atk:{target.CurrentAttack} / hp:{target.CurrentHealth - value}");
            target.CurrentHealth -= value;
        }
    }
}
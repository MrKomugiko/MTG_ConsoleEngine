using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public class Player
    {
        public int ID { get;  internal set; } 
        public int Health { get; internal set; } = 20;
        public List<CardBase> ManaField {get; internal set; } = new();
        public List<CardBase> CombatField {get; private set; } = new();
        public List<CardBase> Hand {get; private set; } = new();
        public List<CardBase> Deck { get; private set; } = new();
        public List<CardBase> Graveyard { get; private set; } = new();
        public List<CardBase> Exiled { get; private set; } = new();
        public bool IsLandPlayedThisTurn { get; internal set; }

        public readonly ConsoleColor color;
        
        public Player(){}
        public Player( int _id, int _health) 
        {
            this.ID = _id;
            this.Health = _health;
        
            if(_id == 1) color = ConsoleColor.Blue;
            else color= ConsoleColor.Green;
        }
        public void DealDamage(int value)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            if(value > 0)
            {
                Console.Write($"Player {ID} otrzymał {value} obrażeń.");
            }  
            Health -= value;
            Console.WriteLine($"Aktualne HP: {Health}");
            Console.ResetColor();
        }
        public void Heal(int value)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            if(value > 0)
            {
                Console.Write($"Player {ID} został uleczony o {value} hp.");
            }  
            Health += value;
            Console.WriteLine($"Aktualne HP: {Health}");
            Console.ResetColor();
        }
        internal void AddToDeck(CardBase _card)
        {
            _card.Owner = this;
            Deck.Add(_card);
        }
        public void DrawCard()
        {
            Console.ForegroundColor = color;
            var newCard = Deck.First();
            Deck.Remove(newCard);
            Hand.Add(newCard);
            Console.WriteLine($"Player {ID} dobrał karte: {newCard.Name}");
            Console.ResetColor();
        }
        internal List<Creature> SelectAttackers()
        {
            Console.ForegroundColor = color;
            List<Creature> _attackersToDeclare = new();
            Console.WriteLine("Wybierz atakujących z dostępnych kreatur na polu: [ indexy oddzielaj przecinkiem: 0,1,3... ]");
            var availableTargets = CombatField.Where(x=> x.isTapped == false && x is Creature );

            Console.WriteLine("dostępne kreatury do ataku: "); 
            availableTargets.ToList().ForEach(x=> Console.Write($"[{CombatField.IndexOf(x)}] Player {x.Owner.ID} / {x.Name} (atk:{((Creature)x).CurrentAttack}/{((Creature)x).CurrentHealth})\n"));
            
            DisplayPlayerField();

            string input = Console.ReadLine()??"";
            Console.ResetColor();
            if(String.IsNullOrEmpty(input)) return new();

            List<string> attackers = input.Trim().Split(",").ToList();
            attackers.ForEach(index=>_attackersToDeclare.Add((Creature)CombatField[Int32.Parse(index)]));
            return _attackersToDeclare;
        }
        public Dictionary<Creature,Creature> SelectDeffenders()
        {
            Console.ForegroundColor = color;
           
            Console.WriteLine("Nadchodzący Atak:");
            foreach (Creature creature in Engine.DeclaredAttackers)
            {
                Console.WriteLine($"[{Engine.DeclaredAttackers.IndexOf(creature)}] - {creature.Name} ({creature.CurrentAttack}/{creature.CurrentHealth})");
            }

            Console.WriteLine("Dostępne jednostki do obrony");
            var availableTargets = CombatField.Where(x=>x.isTapped == false);
            availableTargets.ToList().ForEach(x=> Console.Write($"[{CombatField.IndexOf(x)}] Player {x.Owner.ID} / {x.Name} (atk:{((Creature)x).CurrentAttack}/{((Creature)x).CurrentHealth})\n"));

            Console.WriteLine("Wybierz obronę ataku / (enter zeby pominąć)\n[ najpierw podaj index przeciwnika potem blokujacego np. 1 - 0 ]");
            
            DisplayPlayerField();

            var deffendersToDeclare = InputHelper.Input_DefendersDeclaration(Engine.DeclaredAttackers, CombatField );
           
            Console.ResetColor();

            return deffendersToDeclare;
        }
        public void PlayCardFromHand(CardBase card, List<CardBase> landToPayCost)
        {
            bool isActionValid = false;
            int choosenIndexField = -1, choosenIndexCreature=-1;

            Console.ForegroundColor = color;
            Console.WriteLine($"Gracz 1 zagrywa kartą {card.Name}");
            switch(card)
            {
                case Creature c:  
                    if(! c.CardSpecialActions.Any(x=>x.description == "Haste"))
                    {
                        c.isTapped = true;
                    }
                    CombatField.Add(card);
                    card.UseSpecialAction(ActionType.Play);
                    break;

                case Land:
                    ManaField.Add(card);
                    card.UseSpecialAction(ActionType.Play);
                    break;

                case Enchantment e:
                    if(e.UseOn == "Creature")   
                    {
                        Console.WriteLine("karta typu enchantment: wybierz kreaturke z pola na ktora chcesz ją nałożyc\n\t(np. 1-0 czyli karta przeciwnika z indexem 0)");
                        Console.WriteLine($"[{(ID==1?0:1)}] => Your Combat field Cards:");
                        this.DisplayPlayerField();
                        Console.WriteLine($"[{(ID==2?1:0)}] => Enemies Combat field Cards:");
                        Engine.Players[ID==1?1:0].DisplayPlayerField();                
                        

                        InputPlayerMonsterNumberPair(ref isActionValid, ref choosenIndexField, ref choosenIndexCreature);

                        if(isActionValid)
                        {
                            e.AssingToCreature((Creature)(Engine.Players[choosenIndexField].CombatField[choosenIndexCreature]));
                        }
                        else return;

                    }
                    else
                    {
                        //TODO:
                        Console.WriteLine("Użycie Enchantmenta na postaci");
                        throw new NotImplementedException();
                    }
                    break;

                case Instant i:
                // TODO: rozróżnic kiedy instant potrzebuje celu a kiedy nie
                    List<object> availableTargets = i.GetValidTargets();
                    foreach(CardBase potentialtarget in availableTargets)
                    {
                        Console.WriteLine($"[{availableTargets.IndexOf(potentialtarget)}] => Player {potentialtarget.Owner.ID} /  {potentialtarget.Name}");
                    }
                    Console.WriteLine("podaj index karty na ktorej chcesz uzyc instanta");
                    int index;
                    while(true)
                    {
                        string input = Console.ReadLine()??"";
                        if(String.IsNullOrEmpty(input)) 
                        {
                            Console.WriteLine("cancel operation of card selection, << BACK");
                            isActionValid = false;
                            break;
                        }
                        if(Int32.TryParse(input, out index))
                        {
                            if(index >= 0 && index < availableTargets.Count)
                            {
                                isActionValid = true;
                                choosenIndexField = index;
                                break;
                            }
                            Console.WriteLine("niepoprawny index, podaj liczbę odpowiadajaca karcie");
                            continue;
                        }
                        Console.WriteLine("niepoprawny znak, wprowadz tylko wartosci liczbowe");
                    }

                    if(isActionValid)
                    {
                        if(availableTargets.Count != 0)
                        {
                            i.SpellSelectedTarget = availableTargets[choosenIndexField];
                        }
                        i.UseSpecialAction(ActionType.CastInstant);
                    }
                    else return;
                    break;
            }

            Hand.Remove(card);
            landToPayCost.ForEach(land=>((Land)land).isTapped = true);
            Console.ResetColor();
        }
        private void InputPlayerMonsterNumberPair(ref bool Result, ref int choosenIndexField, ref int choosenIndexCreature)
        {
            while(true)
            {
                var input = Console.ReadLine()??"";
                var inputarr = input.Split("-");
                if(String.IsNullOrEmpty(input)) 
                {
                    Console.WriteLine("cancel operation of assignation enchantment card");
                    Result = false;
                    break;
                }
                if(inputarr.Length<2) 
                {
                    Console.WriteLine("niepoprawny/nie pełny format : <side combat land number> - <index of monster from this area> ex. 1-0");
                    continue;
                }
                if(Int32.TryParse(inputarr[0], out choosenIndexField) == false) 
                {
                    Console.WriteLine("wprowadz poprawny numer strony 0 lub 1, sprobuj ponownie");
                    continue;
                }
                if(choosenIndexField > 1  || choosenIndexField < 0) 
                {
                    Console.WriteLine("niepoprawny numer strony, wprowadz 0 dla swojej czesci lub 1 dla przeciwnika, spróbuj ponownie");
                    continue;   
                }
                if(Int32.TryParse(inputarr[1], out choosenIndexCreature) == false) 
                {
                    Console.WriteLine("niepoprawny index karty z pola, sprobuj ponownie");
                    continue;
                }
                if(choosenIndexCreature > 0 && choosenIndexCreature >= Engine.Players[choosenIndexField].CombatField.Count) 
                {
                    Console.WriteLine($"niepoprawny numer karty potworka, podaj wartosc od 0 do {Engine.Players[choosenIndexField].CombatField.Count-1}, spróbuj ponownie");
                    continue;   
                }

                Result = true;
                break;
            }
        }
        public void DisplayPlayerField()
        {

            /*
╔════════════════════════════════════════════════════════════════════════════════╗
║                             'Player 1 Combat Zone'                             ║
╠══════╦═══════════╦═══════════════════════╦═══════════════╦═════════════════════╣
║ 'ID' ║ 'Status'  ║        'Name'         ║   'Stats'     ║      '.......'      ║
╠══════╬═══════════╬═══════════════════════╬═══════════════╬═════════════════════╣
║   0  ║   Ready   ║             Banehound ║ atk: 1, hp: 1 ║       .......       ║
╠══════╬═══════════╬═══════════════════════╬═══════════════╬═════════════════════╣
║   1  ║   Ready   ║             Banehound ║ atk: 3, hp: 1 ║       .......       ║
╚══════╬═══════════╩═════════════╦═════════╩═══════════════╩═════════════════════╣
       ║      Enchantments:      ║                     Perks:                    ║
       ╠═════════════════════════╬═══════════════════════════════════════════════╣
       ║ + Infernal Scarring     ║  + 11.000000_0  + 11.000000_0  + 11.000000_0  ║
╔══════╬═══════════╦═════════════╩═════════╦═══════════════╦═════════════════════╣
║   2  ║   Ready   ║             Banehound ║ atk: 3, hp: 1 ║       .......       ║
╠══════╬═══════════╬═══════════════════════╬═══════════════╬═════════════════════╣
║   3  ║   Ready   ║             Banehound ║ atk: 3, hp: 1 ║       .......       ║
╚══════╬═══════════╩═════════════╦═════════╩═══════════════╩═════════════════════╣
       ║      Enchantments:      ║                     Perks:                    ║
       ╠═════════════════════════╬═══════════════════════════════════════════════╣
       ║ + Infernal Scarring     ║  + 11.000000_0  + 11.000000_0  + 11.000000_0  ║
╔══════╬═══════════╦═════════════╩═════════╦═══════════════╦═════════════════════╣
║   4  ║   Ready   ║             Banehound ║ atk: 3, hp: 1 ║       .......       ║
╚══════╬═══════════╩═════════════╦═════════╩═══════════════╩═════════════════════╣
       ║      Enchantments:      ║                     Perks:                    ║
       ╠═════════════════════════╬═══════════════════════════════════════════════╣
       ║ + Infernal Scarring     ║  + 11.000000_0  + 11.000000_0  + 11.000000_0  ║
       ╠═════════════════════════╬═══════════════════════════════════════════════╣
       ║ + Infernal Scarring     ║  + 11.000000_0  + 11.000000_0  + 11.000000_0  ║
       ╚═════════════════════════╩═══════════════════════════════════════════════╝

║   5  ║   Ready   ║             Banehound ║ atk: 3, hp: 1 ║       .......       ║
╠══════╬═══════════╬═══════════════════════╬═══════════════╬═════════════════════╣
║   6  ║   Ready   ║        Walking Corpse ║ atk: 2, hp: 2 ║       .......       ║
╚══════╩═══════════╩═══════════════════════╩═══════════════╩═════════════════════╝
            */
            Console.ForegroundColor = color;

            Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
            Title();         // ║                             'Player 1 Combat Zone'                             ║

            if (CombatField.Count == 0)
            {
                Console.WriteLine($"╠════════════════════════════════════════════════════════════════════════════════╣");
                Console.WriteLine($"║    .     ..     ...     ....        EMPTY        ....     ...     ..     .     ║");
                Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                return;
            }

            Console.WriteLine($"╠══════╦═══════════╦═══════════════════════╦═══════════════╦═════════════════════╣");
            MainHeaders();   // ║ 'ID' ║ 'Status'  ║        'Name'         ║   'Stats'     ║      '.......'      ║
            Console.WriteLine($"╠══════╬═══════════╬═══════════════════════╬═══════════════╬═════════════════════╣");

            foreach (Creature creature in CombatField.Where(c => (c is Creature)))
            {
                
                int currentIndex = CombatField.IndexOf(creature);
                rowWithID(creature); //║   4  ║   Ready   ║             Banehound ║ atk: 3, hp: 1 ║       .......       ║

                if (creature.EnchantmentSlots.Count == 0 && creature.Perks.Count == 0)
                {
                    // sprawdzenie czy istnieje nastepna kreaturka 
                    if (currentIndex + 1 < CombatField.Count)
                    {
                        if ((((Creature)CombatField[currentIndex + 1]).EnchantmentSlots.Count > 0 || ((Creature)CombatField[currentIndex + 1]).Perks.Count > 0))
                        {
                            Console.WriteLine("╠══════╬═══════════╬═══════════════════════╬═══════════════╬═════════════════════╣");
                        }
                        else

                        if (((Creature)CombatField[currentIndex + 1]).EnchantmentSlots.Count == 0 && ((Creature)CombatField[currentIndex + 1]).Perks.Count == 0)
                        {
                            Console.WriteLine("╠══════╬═══════════╬═══════════════════════╬═══════════════╬═════════════════════╣");
                        }
                    }
                }

                if (creature.EnchantmentSlots.Count > 0 || creature.Perks.Count > 0)
                {
                    if (currentIndex <= CombatField.Count - 1)
                    {
                        Console.WriteLine($"╚══════╬═══════════╩═════════════╦═════════╩═══════════════╩═════════════════════╣");
                    }
                    EnchantmentHeaders(); //Console.WriteLine("       ║      Enchantments:      ║                     Perks:                    ║");
                    Console.WriteLine($"       ╠═════════════════════════╬═══════════════════════════════════════════════╣");


                    int enchantsCount = creature.EnchantmentSlots.Count;
                    int perksCount = creature.Perks.Count;
                    int rowsCount = enchantsCount > perksCount/3f?enchantsCount:(perksCount/3)+1;

                    for(int row = 0 ; row < rowsCount; row++)
                    {
                        string ench1 ="";
                        string perk1="", perk2="", perk3=""; 

                        if(row <= creature.EnchantmentSlots.Count - 1 )
                        {
                            ench1=$"+ {creature.EnchantmentSlots[row].Name}";
                        }
                        
                        if(row <= (creature.Perks.Count / 3) )
                        {
                            if(row < creature.Perks.Count )
                            {
                                perk1 = $"+ {creature.Perks[0]}";
                            }
                            if(row+1 < creature.Perks.Count )
                            {
                                perk2 = $"+ {creature.Perks[1]}";
                            }
                            if(row+2 < creature.Perks.Count )
                            {
                                perk3 = $"+ {creature.Perks[2]}";
                            }
                        }

                        var enchantmentsLineString = $"       ║ {ench1.PadRight(23)} ║  {perk1.PadRight(13)}  {perk2.PadRight(13)}  {perk3.PadRight(13)}  ║";

                        Console.WriteLine(enchantmentsLineString);
                        if (row < creature.EnchantmentSlots.Count - 1 || row < creature.Perks.Count/3 )
                        {
                            // przedziałka miedzy enczantami
                            Console.WriteLine("       ╠═════════════════════════╬═══════════════════════════════════════════════╣");
                        }
                        else
                        {
                            if(currentIndex < CombatField.Count - 1)
                                Console.WriteLine("╔══════╬═══════════╦═════════════╩═════════╦═══════════════╦═════════════════════╣");
                        }
                    }


        
                    // foreach (Enchantment ench in creature.EnchantmentSlots)
                    // {
                    //     Console.WriteLine($"       ║ + {ench.Name.PadRight(21)} ║  + 11.000000_0  + 11.000000_0  + 11.000000_0  ║");
                    //     if (creature.EnchantmentSlots.IndexOf(ench) < creature.EnchantmentSlots.Count - 1)
                    //     {
                    //         // przedziałka miedzy enczantami
                    //         Console.WriteLine("       ╠═════════════════════════╬═══════════════════════════════════════════════╣");
                    //     }
                    //     else
                    //     {
                    //         if(currentIndex < CombatField.Count - 1)
                    //             Console.WriteLine("╔══════╬═══════════╦═════════════╩═════════╦═══════════════╦═════════════════════╣");
                    //     }
                    // }
                }

                //sprawdzenie jakie dac zakończenie
                if (currentIndex == CombatField.Count - 1)
                {
                    if (creature.EnchantmentSlots.Count > 0 || creature.Perks.Count > 0)
                    {
                        // ma perki
                        Console.WriteLine("       ╚═════════════════════════╩═══════════════════════════════════════════════╝");
                    }
                    else
                    {
                        // bez niczego
                        Console.WriteLine("╚══════╩═══════════╩═══════════════════════╩═══════════════╩═════════════════════╝");
                    }
                }

                currentIndex++;
            }

            Console.ResetColor();

            void MainHeaders()
            {
                Console.ForegroundColor = color;
                Console.Write("║ ");
                Console.ResetColor();
                Console.Write("'ID'");
                Console.ForegroundColor = color;
                Console.Write(" ║ ");
                Console.ResetColor();
                Console.Write("'Status'");
                Console.ForegroundColor = color;
                Console.Write("  ║        ");
                Console.ResetColor();
                Console.Write("'Name'");
                Console.ForegroundColor = color;
                Console.Write("         ║   ");
                Console.ResetColor();
                Console.Write("'Stats'");
                Console.ForegroundColor = color;
                Console.Write("     ║      ");
                Console.ResetColor();
                Console.Write("'.......'");
                Console.ForegroundColor = color;
                Console.Write("      ║\n");
            }
            void Title()
            {
                Console.Write($"║                             ");
                Console.ResetColor();
                Console.Write($"'Player {ID} Combat Zone'");
                Console.ForegroundColor = color;
                Console.Write("                             ║\n");
            }
            void EnchantmentHeaders()
            {
                Console.ForegroundColor = color;
                Console.Write("       ║     ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("'Enchantments'");
                Console.ForegroundColor = color;
                Console.Write("      ║                    ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("'Perks'");
                Console.ForegroundColor = color;
                Console.Write("                    ║\n");
            }
            void rowWithID(Creature creature)
            {
                int currentIndex = CombatField.IndexOf(creature);
                string tappedValue = creature.isTapped ? "Tapped" : " Ready";
                string statsValue = $"atk:{creature.CurrentAttack.ToString().PadLeft(2)}, hp:{creature.CurrentHealth.ToString().PadLeft(2)}";
                Console.Write($"║  ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{currentIndex.ToString().PadLeft(2)}");
                Console.ForegroundColor = color;
                Console.Write($"  ║  {tappedValue.PadLeft(6)}   ║ {creature.Name.PadLeft(21)} ║ {statsValue} ║                     ║\n");
            }
        }
        public Dictionary<int,(bool result,List<CardBase> landsToTap)> DisplayPlayerHand()
        {
            string myavailableMana = String.Join(",",SumAvailableManaFromManaField().Select(x=>$"{x.Key} = {x.Value}")).ToString();
            Console.ForegroundColor = color;
            Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");

            Console.Write($"║                                ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write($"'Player {ID} Hand'");
             Console.ForegroundColor = color; 
             Console.Write("                                 ║\n");
            Console.WriteLine($"╠═══════════════════════════╦════════════════════════════════════════════════════╣");

            Console.Write($"║          ");
             Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"HP:{Health.ToString().PadLeft(3)}");
            Console.ForegroundColor = color; 
            Console.Write("           ║  ");
             Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Mana: {myavailableMana.PadRight(43)}");
            Console.ForegroundColor = color; 
            Console.Write(" ║\n");

            Console.WriteLine($"╠═══════╦════╦══════════════╬═══════════════════════╦════════════╦═══════════════╣");
            Console.Write($"║ ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("'Use'");
            Console.ForegroundColor = color; 
            Console.Write(" ║");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("'ID'");
            Console.ForegroundColor = color; 
            Console.Write("║   ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("'Type'");
            Console.ForegroundColor = color; 
            Console.Write("     ║        ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("'Name'");
            Console.ForegroundColor = color; 
            Console.Write("         ║   ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("'Cost'");
            Console.ForegroundColor = color; 
            Console.Write("   ║   ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("'Stats'");
            Console.ForegroundColor = color; 
            Console.Write("     ║\n");

            Console.WriteLine($"╠═══════╬════╬══════════════╬═══════════════════════╬════════════╬═══════════════╣");

            Dictionary<int ,(bool result,List<CardBase> landsToTap)> validpickwithcostcast = new();
            for(int index = 0;index < Hand.Count; index++)
            {   
                if(Hand[index].CardType == "Instant") 
                    Console.Write("");

                validpickwithcostcast.Add(index,Hand[index].CheckAvailability());
                Console.ForegroundColor = color; 
                Console.Write($"║ {validpickwithcostcast[index].result.ToString().PadLeft(5)} ║ ");
                Console.ForegroundColor = ConsoleColor.White; 
                Console.Write($"{index.ToString().PadLeft(2)}");
                Console.ForegroundColor = color; 
                Console.Write($" ║ {Hand[index].GetCardString().PadLeft(49)} ║\n");
                if(index < Hand.Count-1)
                {
                    Console.WriteLine("╠═══════╬════╬══════════════╬═══════════════════════╬════════════╬═══════════════╣");
                }
            }
            Console.WriteLine("╚═══════╩════╩══════════════╩═══════════════════════╩════════════╩═══════════════╝");
            Console.ResetColor();
            return validpickwithcostcast;
        }
        public Dictionary<string, int> SumAvailableManaFromManaField()
        {
            Dictionary<string, int> SumManaOwnedAndAvailable = new();
            //sumowanie dostepnej many
            foreach (Land manaCard in ManaField.Where(c => c is Land && c.isTapped == false))
            {
                foreach (var mana in manaCard.manaValue)
                {
                    if (SumManaOwnedAndAvailable.ContainsKey(mana.Key))
                    {
                        SumManaOwnedAndAvailable[mana.Key] += mana.Value;
                    }
                    else
                    {
                        SumManaOwnedAndAvailable.Add(mana.Key, mana.Value);
                    }
                }
            }

            return SumManaOwnedAndAvailable;
        }

        internal void ShuffleDeck()
        {
            var rand = new Random();
            for (var i = Deck.Count - 1; i > 0; i--)
            {
                var temp = Deck[i];
                var index = rand.Next(0, i + 1);
                Deck[i] = Deck[index];
                Deck[index] = temp;
            }
        }
    }
}
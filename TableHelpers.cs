using MTG_ConsoleEngine.Card_Category;

namespace MTG_ConsoleEngine
{
    public static class TableHelpers
    {
        internal static void DisplayFieldTable(EngineBase _gameEngine, ConsoleColor _color, List<CardBase> _combatField, int _playerNumberID )
        {
            Console.ForegroundColor = _color;

            Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");
            Title();         

            if (_combatField.Count == 0)
            {
                Console.WriteLine($"╠════════════════════════════════════════════════════════════════════════════════╣");
                Console.WriteLine($"║    .     ..     ...     ....        EMPTY        ....     ...     ..     .     ║");
                Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                return;
            }

            Console.WriteLine($"╠══════╦═══════════╦═══════════════════════╦═══════════════╦═════════════════════╣");
            MainHeaders();   // ║ indx ║ 'Status'  ║        'Name'         ║   'Stats'     ║      '.......'      ║
            Console.WriteLine($"╠══════╬═══════════╬═══════════════════════╬═══════════════╬═════════════════════╣");
            string alertInfo = "";
            foreach (Creature creature in _combatField.Where(c => c is Creature))
            {
                int currentIndex = _combatField.IndexOf(creature);

                    if(_gameEngine.GetAttackerDeclaration() != null && _gameEngine.GetAttackerDeclaration().Contains(creature) == true)
                    {
                        alertInfo = " ATTACK INCOMMING! ";
                    }
                else if(_gameEngine.GetDeffendersDeclaration() != null &&_gameEngine.GetDeffendersDeclaration().ContainsKey(creature) == true)
                {
                    alertInfo = "   ON DEFFENDING.  ";
                }
                else
                {
                    alertInfo = "";
                }
                rowWithID(creature); //║   4  ║   Ready   ║             Banehound ║ atk: 3, hp: 1 ║       .......       ║

                if (creature.EnchantmentSlots.Count == 0 && creature.Perks.Count == 0)
                {
                    // sprawdzenie czy istnieje nastepna kreaturka 
                    if (currentIndex + 1 < _combatField.Count)
                    {
                        if ((((Creature)_combatField[currentIndex + 1]).EnchantmentSlots.Count > 0 || ((Creature)_combatField[currentIndex + 1]).Perks.Count > 0))
                        {
                            Console.WriteLine("╠══════╬═══════════╬═══════════════════════╬═══════════════╬═════════════════════╣");
                        }
                        else

                        if (((Creature)_combatField[currentIndex + 1]).EnchantmentSlots.Count == 0 && ((Creature)_combatField[currentIndex + 1]).Perks.Count == 0)
                        {
                            Console.WriteLine("╠══════╬═══════════╬═══════════════════════╬═══════════════╬═════════════════════╣");
                        }
                    }
                }

                if (creature.EnchantmentSlots.Count > 0 || creature.Perks.Count > 0)
                {
                    if (currentIndex <= _combatField.Count - 1)
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
                            if((3*row) < creature.Perks.Count )
                            {
                                perk1 = $"+ {creature.Perks[(3*row)]}";
                            }
                            if((3*row)+1 < creature.Perks.Count )
                            {
                                perk2 = $"+ {creature.Perks[(3*row)+1]}";
                            }
                            if((3*row)+2 < creature.Perks.Count )
                            {
                                perk3 = $"+ {creature.Perks[(3*row)+2]}";
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
                            if(currentIndex < _combatField.Count - 1)
                                Console.WriteLine("╔══════╬═══════════╦═════════════╩═════════╦═══════════════╦═════════════════════╣");
                        }
                    }
                }

                //sprawdzenie jakie dac zakończenie
                if (currentIndex == _combatField.Count - 1)
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
            }

            Console.ResetColor();

            void MainHeaders()
            {
                Console.ForegroundColor = _color;
                Console.Write("║ ");
                Console.ResetColor();
                Console.Write("indx");
                Console.ForegroundColor = _color;
                Console.Write(" ║ ");
                Console.ResetColor();
                Console.Write("'Status'");
                Console.ForegroundColor = _color;
                Console.Write("  ║        ");
                Console.ResetColor();
                Console.Write("'Name'");
                Console.ForegroundColor = _color;
                Console.Write("         ║   ");
                Console.ResetColor();
                Console.Write("'Stats'");
                Console.ForegroundColor = _color;
                Console.Write("     ║      ");
                Console.ResetColor();
                Console.Write("         ");
                Console.ForegroundColor = _color;
                Console.Write("      ║\n");
            }
            void Title()
            {
                Console.Write($"║                             ");
                Console.ResetColor();
                Console.Write($"'Player {_playerNumberID} Combat Zone'");
                Console.ForegroundColor = _color;
                Console.Write("                             ║\n");
            }
            void EnchantmentHeaders()
            {
                Console.ForegroundColor = _color;
                Console.Write("       ║     ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("'Enchantments'");
                Console.ForegroundColor = _color;
                Console.Write("      ║                    ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("'Perks'");
                Console.ForegroundColor = _color;
                Console.Write("                    ║\n");
            }
            void rowWithID(Creature creature)
            {
                int currentIndex = _combatField.IndexOf(creature);
                string tappedValue = creature.IsTapped ? "Tapped" : " Ready";
                string statsValue = $"atk:{creature.CurrentAttack.ToString().PadLeft(2)}, hp:{creature.CurrentHealth.ToString().PadLeft(2)}";
                Console.Write($"║  ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{currentIndex.ToString().PadLeft(2)}");
                Console.ForegroundColor = _color;                                                         // ║  ATTACK INCOMMING ! ║            
                Console.Write($"  ║  {tappedValue.PadLeft(6)}   ║ {creature.Name.PadLeft(21)} ║ {statsValue} ║ {alertInfo.PadLeft(19)} ║\n");
            }
        }
        internal static void DisplayHandTable(ConsoleColor _color, int[] _currentManaArr, int _playerNumberID, int _playerHEalth, List<CardBase> _hand )
        {
            Console.ForegroundColor = _color;
            string manaString = $"{(_currentManaArr[0] == 0 ? "" : $"[ ] {_currentManaArr[0]}, ")}";
            manaString += $"{(_currentManaArr[1] == 0 ? "" : $"[B] {_currentManaArr[1]}, ")}";
            manaString += $"{(_currentManaArr[2] == 0 ? "" : $"[W] {_currentManaArr[2]}, ")}";
            manaString += $"{(_currentManaArr[3] == 0 ? "" : $"[R] {_currentManaArr[3]}, ")}";
            manaString += $"{(_currentManaArr[4] == 0 ? "" : $"[G] {_currentManaArr[4]}, ")}";
            manaString += $"{(_currentManaArr[5] == 0 ? "" : $"[U] {_currentManaArr[5]}")}";

            string myavailableMana = manaString;
       
            Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════╗");

            Console.Write($"║                                ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write($"'Player {_playerNumberID} Hand'");
             Console.ForegroundColor = _color; 
             Console.Write("                                 ║\n");
            Console.WriteLine($"╠═══════════════════════════╦════════════════════════════════════════════════════╣");

            Console.Write($"║          ");
             Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"HP:{_playerHEalth.ToString().PadLeft(3)}");
            Console.ForegroundColor = _color; 
            Console.Write("           ║  ");
             Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Mana: {myavailableMana.PadRight(43)}");
            Console.ForegroundColor = _color; 
            Console.Write(" ║\n");

            Console.WriteLine($"╠═══════╦════╦══════════════╬═══════════════════════╦════════════╦═══════════════╣");
            Console.Write($"║ ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("'Use'");
            Console.ForegroundColor = _color; 
            Console.Write(" ║");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("indx");
            Console.ForegroundColor = _color; 
            Console.Write("║   ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("'Type'");
            Console.ForegroundColor = _color; 
            Console.Write("     ║        ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("'Name'");
            Console.ForegroundColor = _color; 
            Console.Write("         ║   ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("'Cost'");
            Console.ForegroundColor = _color; 
            Console.Write("   ║   ");
            Console.ForegroundColor = ConsoleColor.White; 
            Console.Write("'Stats'");
            Console.ForegroundColor = _color; 
            Console.Write("     ║\n");

            Console.WriteLine($"╠═══════╬════╬══════════════╬═══════════════════════╬════════════╬═══════════════╣");

            Dictionary<int ,(bool result,List<CardBase> landsToTap)> validpickwithcostcast = new();
            for(int index = 0;index < _hand.Count; index++)
            {   
                if(_hand[index].CardType == "Instant") 
                    Console.Write("");

                Console.ForegroundColor = _color; 
                Console.Write($"║ {_hand[index].IsAbleToPlay.ToString().PadLeft(5)} ║ ");
                Console.ForegroundColor = ConsoleColor.White; 
                Console.Write($"{index.ToString().PadLeft(2)}");
                Console.ForegroundColor = _color; 
                Console.Write($" ║ {_hand[index].GetCardString().PadLeft(49)} ║\n");
                if(index < _hand.Count-1)
                {
                    Console.WriteLine("╠═══════╬════╬══════════════╬═══════════════════════╬════════════╬═══════════════╣");
                }
            }
            Console.WriteLine("╚═══════╩════╩══════════════╩═══════════════════════╩════════════╩═══════════════╝");
            Console.ResetColor();
        }
        internal static (bool status, int playerIndex, int creatureIndex) Input_SinglePairPlayerMonster(Engine _gameEngine)
        {
            Console.ResetColor();
            while (true)
            {
                var input = Console.ReadLine() ?? "";
                var inputarr = input.Split("-");
                if (String.IsNullOrEmpty(input))
                {
                    Console.WriteLine("cancel operation of assignation enchantment card");
                    return (false, -1, -1);
                }
                if (inputarr.Length < 2)
                {
                    Console.WriteLine("niepoprawny/nie pełny format : <side combat land number> - <index of monster from this area> ex. 1-0");
                    continue;
                }
                if (Int32.TryParse(inputarr[0], out int choosenIndexField) == false)
                {
                    Console.WriteLine("wprowadz poprawny numer strony 0 lub 1, sprobuj ponownie");
                    continue;
                }
                if (choosenIndexField > 1 || choosenIndexField < 0)
                {
                    Console.WriteLine("niepoprawny numer strony, wprowadz 0 dla swojej czesci lub 1 dla przeciwnika, spróbuj ponownie");
                    continue;
                }
                if (Int32.TryParse(inputarr[1], out int choosenIndexCreature) == false)
                {
                    Console.WriteLine("niepoprawny index karty z pola, sprobuj ponownie");
                    continue;
                }
                if (choosenIndexCreature > 0 && choosenIndexCreature >= _gameEngine.Players[choosenIndexField].CombatField.Count)
                {
                    Console.WriteLine($"niepoprawny numer karty potworka, podaj wartosc od 0 do {_gameEngine.Players[choosenIndexField].CombatField.Count - 1}, spróbuj ponownie");
                    continue;
                }

                return (true, choosenIndexField, choosenIndexCreature);
            }
        }

    }
}
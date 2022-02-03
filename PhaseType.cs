namespace MTG_ConsoleEngine
{
    public partial class Engine
    {
        public enum PhaseType {
            StartNewGame = 0,
            Beginning_Phase = 1,
            First_Main_Phase = 2,
            Combat_Phase_Begining = 3,
            Combat_Phase_AttackersDeclaration = 4,
            Combat_Phase_DefendersDeclaration = 5,
            Combat_Phase_AttackerInstants = 6,
            Combat_Phase_DeffenderInstants = 7,
            Combat_Phase_Combat = 8,
            Combat_Phase_End = 9,
            Second_Main_Phase = 10,
            End_Phase = 11
        }
    }
}
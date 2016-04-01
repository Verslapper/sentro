namespace Sentro.Enums
{
    public enum Mode
    {
        /// <summary>
        /// Standard betting
        /// </summary>
        Matchmaking = 1,
        /// <summary>
        /// Start in the mines, but preference all-ins when going for a win, or until a certain range
        /// </summary>
        Tournament = 2,
        /// <summary>
        /// Unpredictable trap mode. Money to be made if you ain't got much, but go conservative otherwise
        /// </summary>
        Exhibitions = 3
    }
}

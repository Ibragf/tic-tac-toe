namespace tic_tac_toe.Services
{
    public interface IDateProvider
    {
        DateTime GetCurrentDate();
    }

    public class DateProvider : IDateProvider
    {
        public DateTime GetCurrentDate()
        {
            return DateTime.UtcNow;
        }
    }
}

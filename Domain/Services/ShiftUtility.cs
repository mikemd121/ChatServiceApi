namespace ChatServiceApi
{
    public record ShiftSchedule(ShiftType Type, TimeSpan Start, TimeSpan End);
   public class ShiftUtility
    {
        public static ShiftType GetCurrentShift(List<ShiftSchedule> shifts,TimeSpan now)
        {
 

            foreach (var shift in shifts)
            {
                if (shift.Start < shift.End)
                {
                    if (now >= shift.Start && now < shift.End)
                        return shift.Type;
                }
                else
                {
                    if (now >= shift.Start || now < shift.End)
                        return shift.Type;
                }
            }
            throw new Exception("No active shift found");
        }
    }
}

using ChatServiceApi;
using Xunit;

namespace Tests
{
    public class ShiftUtilityTests
    {
        private readonly List<ShiftSchedule> _shiftTimings = new()
    {
        new(ShiftType.Morning, new TimeSpan(6, 0, 0), new TimeSpan(14, 0, 0)),
        new(ShiftType.Evening, new TimeSpan(14, 0, 0), new TimeSpan(22, 0, 0)),
        new(ShiftType.Night, new TimeSpan(22, 0, 0), new TimeSpan(6, 0, 0)) 
    };

        [Theory]
        [InlineData(7, 0, ShiftType.Morning)]
        [InlineData(13, 59, ShiftType.Morning)]
        [InlineData(14, 0, ShiftType.Evening)]
        [InlineData(21, 59, ShiftType.Evening)]
        [InlineData(22, 0, ShiftType.Night)]
        [InlineData(2, 0, ShiftType.Night)]
        [InlineData(5, 59, ShiftType.Night)]
        public void GetCurrentShift_ReturnsCorrectShift(int hour, int minute, ShiftType expected)
        {
            var now = new TimeSpan(hour, minute, 0);
            var result = ShiftUtility.GetCurrentShift(_shiftTimings, now);
            Assert.Equal(expected, result);
        }
    }
}

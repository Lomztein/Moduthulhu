using Lomztein.Moduthulhu.Modules.Clock.ActivityMonitor;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static Lomztein.Moduthulhu.Modules.Clock.ActivityMonitor.UserActivityMonitorPlugin;

namespace Tests
{
    public class UserActivityMonitorTests
    {
        [Theory]
        [InlineData (-1, 0)]
        [InlineData (0, 0)]
        [InlineData (1, 0)]
        [InlineData (8, 1)]
        [InlineData (9, 1)]
        [InlineData (13, 1)]
        [InlineData (30, 2)]
        [InlineData (31, 2)]
        [InlineData (60, 2)]
        public void TestGetRole (int daysOffset, ulong expectedRole)
        {
            List<ActivityRole> roles = new List<ActivityRole>
            {
                new ActivityRole (0, 7),
                new ActivityRole (1, 14),
                new ActivityRole (2, 30),
            };

            ulong role = GetRole(roles, DateTime.Now.AddDays(-daysOffset));
            Assert.Equal(role, expectedRole);
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(8, 1)]
        [InlineData(9, 1)]
        [InlineData(13, 1)]
        [InlineData(30, 1)]
        [InlineData(31, 1)]
        [InlineData(60, 1)]
        public void TestGetRoleMonsterMash (int daysOffset, ulong expectedRole)
        {
            List<ActivityRole> roles = new List<ActivityRole>
            {
                new ActivityRole (0, 7),
                new ActivityRole (1, 30),
            };

            ulong role = GetRole(roles, DateTime.Now.AddDays(-daysOffset));
            Assert.Equal(role, expectedRole);
        }

    }
}

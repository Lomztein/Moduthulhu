
using System;
using Xunit;
using Lomztein.Moduthulhu.Modules.Clock.Birthday;
using Lomztein.Moduthulhu.UnitTests.FakeClasses;

namespace Lomztein.Moduthulhu.UnitTests.Modules {

    public class BirthdayTests {

        [Theory]
        [InlineData (20, "'th")]
        [InlineData (21, "'st")]
        [InlineData (22, "'nd")]
        [InlineData (23, "'rd")]
        [InlineData (24, "'th")]
        [InlineData (25, "'th")]
        [InlineData (26, "'th")]
        [InlineData (27, "'th")]
        [InlineData (28, "'th")]
        [InlineData (29, "'th")]
        public void BirthdaySuffixTest(int age, string expectedOutput) {

            BirthdayModule.BirthdayDate birthday = new BirthdayModule.BirthdayDate (DateTime.Now.AddYears (-20));
            string output = birthday.GetAgeSuffix (age);

            Assert.Equal (expectedOutput, output);

        }
        
        [Theory]
        [InlineData (1996, 12, 12, 21)]
        [InlineData (1971, 12, 12, 46)]
        [InlineData (1995, 12, 12, 22)]
        [InlineData (2010, 12, 12, 7)]
        [InlineData (1983, 12, 12, 34)]
        [InlineData (2017, 12, 12, 0)]
        [InlineData (1975, 12, 12, 42)]
        public void BirthdayAgeTest(int year, int month, int day, int expectedAge) {

            DateTime fakeDay = new DateTime (2018, 5, 10); // Litteraly the day these tests are written.

            FakeBirthdayDate fakeBirthday = new FakeBirthdayDate (new DateTime (year, month, day), fakeDay);
            int outputAge = fakeBirthday.GetAge ();

            Assert.Equal (expectedAge, outputAge);

        }

    }

}

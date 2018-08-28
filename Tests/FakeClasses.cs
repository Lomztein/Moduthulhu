using Lomztein.Moduthulhu.Modules.Clock.Birthday;
using System;

namespace Lomztein.Moduthulhu.UnitTests.FakeClasses {

    public class FakeBirthdayDate : BirthdayModule.BirthdayDate {

            public DateTime fakeNow;

            public FakeBirthdayDate(DateTime _date, DateTime _fakeNow) : base (_date) {
                fakeNow = _fakeNow;
            }

            public override DateTime GetNow() => fakeNow;

        }
}
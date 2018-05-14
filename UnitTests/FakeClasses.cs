using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Modules.Clock.Birthday;

namespace Lomztein.Moduthulhu.UnitTests.FakeClasses
{
        public class FakeBirthdayDate : BirthdayModule.BirthdayDate {

            public DateTime fakeNow;

            public FakeBirthdayDate(DateTime _date, DateTime _fakeNow) : base (_date) {
                fakeNow = _fakeNow;
            }

            public override DateTime GetNow() => fakeNow;

        }
}

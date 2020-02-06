using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests
{
    public class PluginMessengerTests
    {
        private PluginMessenger _persistant = new PluginMessenger();

        [Theory]
        [InlineData ("Target", "Name", true)] // Test that any input works.
        [InlineData ("Target", "Name", false)]
        [InlineData ("", "", false)] // Test that any combination of Target and Name works.
        [InlineData ("Target", "", false)]
        [InlineData ("", "Name", false)]
        [InlineData (null, "", false)]
        [InlineData (null, null, false)]
        public void TestRegisterParameterizedAction (string target, string name, bool expectedValue)
        {
            var messenger = new PluginMessenger(); 
            bool value = false;
            messenger.Register(target, name, (x) => value = (bool)x[0]);
            messenger.SendMessage(target, name, new object[] { expectedValue });
            Assert.True(value == expectedValue);
        }

        [Theory]
        [InlineData (2, "2")]
        [InlineData ("Assmunch", "Assmunch")]
        public void TestRegisterParameterizedFunction (object input, string toString)
        {
            var messenger = new PluginMessenger();
            messenger.Register("Target", "Name", x => input.ToString());
            Assert.True(messenger.SendMessage<string>("Target", "Name", new object[] { input }) == toString);
        }

        [Theory]
        [InlineData ("Target", "Name", "Target", "Name", true)] // Baseline check that ordinary identification works.
        [InlineData ("Target", "Name", "", "Name", false)] // Check that a target name is required if a specific target is registered.
        [InlineData ("", "Name", "Target", "Name", false)] // Check that a non-specific target cannot be contacted by a specific target.
        [InlineData ("", "Name", "", "Name", true)] // Check that an non-specific target can be targeted by supplying a non-specific target. what.
        public void TestInvalidIdentifierMatches (string registerTarget, string registerName, string sendTarget, string sendName, bool expectedMatch)
        {
            var messenger = new PluginMessenger();
            bool value = false;
            messenger.Register(registerTarget, registerName, x => value = true);
            messenger.SendMessage(sendTarget, sendName);
            Assert.True(value == expectedMatch);
        }

        [Fact]
        public void TestRegisterAction ()
        {
            var messenger = new PluginMessenger();
            bool value = false;
            messenger.Register("Target", "Name", x => value = true);
            messenger.SendMessage("Target", "Name");
            Assert.True(value);
        }

        [Fact]
        public void TestRegisterFunction()
        {
            var messenger = new PluginMessenger();
            messenger.Register("Target", "Name", x => 2 + 2);
            Assert.True((int)messenger.SendMessage ("Target", "Name") == 4);
        }

        [Fact]
        public void TestRegisterGenericFunction()
        {
            var messenger = new PluginMessenger();
            messenger.Register("Target", "Name", x => 2 + 2);
            Assert.True(messenger.SendMessage<int>("Target", "Name") == 4);
        }
    }
}

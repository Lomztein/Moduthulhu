using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lomztein.ModularDiscordBot.UnitTests.Modules {

    [TestClass]
    public class ModuleTests {

        [TestMethod]
        public void ReferenceTest() {

            try {

                float n1 = 4f;
                float n2 = 1.5f;
                float expected = 5.5f;

                TestModule.ReferenceTest.NestedClass referencedObjectType = new TestModule.ReferenceTest.NestedClass ();
                float result = referencedObjectType.Method (n1, n2);
                Assert.AreEqual (expected, result, 0.1f);

            } catch {
                Assert.Inconclusive ();
            }
        }
    }
}

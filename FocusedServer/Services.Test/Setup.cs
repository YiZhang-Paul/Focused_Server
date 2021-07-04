using NUnit.Framework;
using WebApi.AppStart;

namespace Services.Test
{
    [SetUpFixture]
    public class Setup
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            CustomBsonSerializers.Register();
        }
    }
}

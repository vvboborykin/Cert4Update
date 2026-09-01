using Cert4Update.Core;

namespace CoreTest
{
    public class CollectorServiceTest
    {
        [Fact]
        public async Task Test()
        {
            var vCertDir = @"c:\temp";
            var vService = new CollectorService();
            var result = await vService.CollectCertificatesForUpdate(new(vCertDir, new ProgressCmd(), 60), CancellationToken.None);
            Assert.True(result != null);
        }
    }
}

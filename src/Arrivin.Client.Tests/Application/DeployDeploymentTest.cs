using Arrivin.Client.Application;
using Arrivin.Client.Domain;
using Arrivin.Domain;
using FluentAssertions;
using FluentAssertions.Execution;
using LanguageExt.Sys.Live;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static LanguageExt.Prelude;

namespace Arrivin.Client.Tests.Application;

[TestClass]
public class DeployDeploymentTest
{
    private readonly TestEnvironment<Runtime> testEnvironment = new();

    private ServiceProvider serviceProvider = default!;

    [TestCleanup]
    public void TestCleanup()
    {
        serviceProvider.Dispose();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        var services = new ServiceCollection();
        services.AddApplicationServices<Runtime>();

        services.AddSingleton(testEnvironment.ApiClient.Object);
        services.AddSingleton(testEnvironment.Nix.Object);
        services.AddSingleton(testEnvironment.Cli.Object);
        services.AddSingleton(testEnvironment.FileSystem.Object);

        serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task WithOutdatedDerivationAndOutputLinks()
    {
        // Arrange
        var serverUrl = ServerUrl.From(new Uri("http://server/"));
        var name = DeploymentName.From("test");
        var extraBuildArgs = NixArgs.From([]);
        var dataDirectory = FilePath.From("/var/lib/arrivin");
        var subject = serviceProvider.GetRequiredService<DeployDeployment<Runtime>>();
        var runtime = Runtime.New();

        testEnvironment.ApiClient
            .Setup(x => x.GetDeployment(It.IsAny<ApiUrl>(), It.IsAny<DeploymentName>()))
            .Returns(SuccessAff(Some(new DeploymentInfo(StorePath.From("/nix/store/test.drv"), None))));

        testEnvironment.Nix
            .Setup(x => x.Build(It.IsAny<StorePath>(), It.IsAny<NixArgs>()))
            .Returns(SuccessAff(StorePath.From("/nix/store/test")));

        testEnvironment.FileSystemEntries[FilePath.From("/var/lib/arrivin/gcroots/deployments/test.drv")] =
            FileEntry.Symlink(FilePath.From("/nix/store/test-old.drv"));
        testEnvironment.FileSystemEntries[FilePath.From("/var/lib/arrivin/gcroots/deployments/test.out")] =
            FileEntry.Symlink(FilePath.From("/nix/store/test-old"));

        // Act
        await subject.With(serverUrl, dataDirectory, name, extraBuildArgs).RunUnit(runtime);

        // Assert
        using (new AssertionScope())
        {
            testEnvironment.FileSystemEntries.Should()
                .Contain(FilePath.From("/var/lib/arrivin/gcroots/deployments/test.drv"),
                    FileEntry.Symlink(FilePath.From("/nix/store/test.drv")));
            testEnvironment.FileSystemEntries.Should()
                .Contain(FilePath.From("/var/lib/arrivin/gcroots/deployments/test.out"),
                    FileEntry.Symlink(FilePath.From("/nix/store/test")));
            testEnvironment.FileSystemEntries.Should()
                .NotContainKey(FilePath.From("/var/lib/arrivin/gcroots/deployments/test-current.drv"));
            testEnvironment.FileSystemEntries.Should()
                .NotContainKey(FilePath.From("/var/lib/arrivin/gcroots/deployments/test-current.out"));
            testEnvironment.Cli
                .Verify(x => x.Call(StorePath.From("/nix/store/test/arrivin-activate")));
        }
    }

    [TestMethod]
    public async Task WithoutDerivationAndOutputLinks()
    {
        // Arrange
        var serverUrl = ServerUrl.From(new Uri("http://server/"));
        var name = DeploymentName.From("test");
        var extraBuildArgs = NixArgs.From([]);
        var dataDirectory = FilePath.From("/var/lib/arrivin");
        var subject = serviceProvider.GetRequiredService<DeployDeployment<Runtime>>();
        var runtime = Runtime.New();

        testEnvironment.ApiClient
            .Setup(x => x.GetDeployment(It.IsAny<ApiUrl>(), It.IsAny<DeploymentName>()))
            .Returns(SuccessAff(Some(new DeploymentInfo(StorePath.From("/nix/store/test.drv"), None))));

        testEnvironment.Nix
            .Setup(x => x.Build(It.IsAny<StorePath>(), It.IsAny<NixArgs>()))
            .Returns(SuccessAff(StorePath.From("/nix/store/test")));

        // Act
        await subject.With(serverUrl, dataDirectory, name, extraBuildArgs).RunUnit(runtime);

        // Assert
        using (new AssertionScope())
        {
            testEnvironment.FileSystemEntries.Should()
                .Contain(FilePath.From("/var/lib/arrivin/gcroots/deployments/test.drv"),
                    FileEntry.Symlink(FilePath.From("/nix/store/test.drv")));
            testEnvironment.FileSystemEntries.Should()
                .Contain(FilePath.From("/var/lib/arrivin/gcroots/deployments/test.out"),
                    FileEntry.Symlink(FilePath.From("/nix/store/test")));
            testEnvironment.FileSystemEntries.Should()
                .NotContainKey(FilePath.From("/var/lib/arrivin/gcroots/deployments/test-current.drv"));
            testEnvironment.FileSystemEntries.Should()
                .NotContainKey(FilePath.From("/var/lib/arrivin/gcroots/deployments/test-current.out"));
            testEnvironment.Cli
                .Verify(x => x.Call(StorePath.From("/nix/store/test/arrivin-activate")));
        }
    }

    [TestMethod]
    public async Task WithSameDerivationLink()
    {
        // Arrange
        var serverUrl = ServerUrl.From(new Uri("http://server/"));
        var name = DeploymentName.From("test");
        var extraBuildArgs = NixArgs.From([]);
        var dataDirectory = FilePath.From("/var/lib/arrivin");
        var subject = serviceProvider.GetRequiredService<DeployDeployment<Runtime>>();
        var runtime = Runtime.New();

        testEnvironment.ApiClient
            .Setup(x => x.GetDeployment(It.IsAny<ApiUrl>(), It.IsAny<DeploymentName>()))
            .Returns(SuccessAff(Some(new DeploymentInfo(StorePath.From("/nix/store/test.drv"), None))));

        testEnvironment.Nix
            .Setup(x => x.Build(It.IsAny<StorePath>(), It.IsAny<NixArgs>()))
            .Returns(SuccessAff(StorePath.From("/nix/store/test")));

        testEnvironment.FileSystemEntries[FilePath.From("/var/lib/arrivin/gcroots/deployments/test.drv")] =
            FileEntry.Symlink(FilePath.From("/nix/store/test.drv"));
        testEnvironment.FileSystemEntries[FilePath.From("/var/lib/arrivin/gcroots/deployments/test.out")] =
            FileEntry.Symlink(FilePath.From("/nix/store/test"));

        // Act
        await subject.With(serverUrl, dataDirectory, name, extraBuildArgs).RunUnit(runtime);

        // Assert
        using (new AssertionScope())
        {
            testEnvironment.FileSystemEntries.Should()
                .Contain(FilePath.From("/var/lib/arrivin/gcroots/deployments/test.drv"),
                    FileEntry.Symlink(FilePath.From("/nix/store/test.drv")));
            testEnvironment.FileSystemEntries.Should()
                .Contain(FilePath.From("/var/lib/arrivin/gcroots/deployments/test.out"),
                    FileEntry.Symlink(FilePath.From("/nix/store/test")));
            testEnvironment.Nix
                .Verify(x => x.Build(It.IsAny<StorePath>(), It.IsAny<NixArgs>()), Times.Never);
            testEnvironment.Cli
                .Verify(x => x.Call(StorePath.From("/nix/store/test/arrivin-activate")), Times.Never);
        }
    }
}

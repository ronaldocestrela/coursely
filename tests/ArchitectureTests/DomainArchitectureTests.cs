using Domain;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace ArchitectureTests;

public sealed class DomainArchitectureTests
{
    [Fact]
    public void Domain_should_not_depend_on_other_layers_or_frameworks()
    {
        var domain = typeof(AssemblyMarker).Assembly;

        var result = Types.InAssembly(domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Application",
                "Infrastructure",
                "Api",
                "MediatR",
                "FluentValidation",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Microsoft.AspNetCore.Identity")
            .GetResult();

        var failing = result.FailingTypes ?? [];

        result.IsSuccessful.Should().BeTrue(
            "domain violated dependency rules: {0}",
            string.Join(", ", failing.Select(t => t.FullName)));
    }
}

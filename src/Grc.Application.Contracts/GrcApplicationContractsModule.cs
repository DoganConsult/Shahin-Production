using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace Grc.Application.Contracts;

[DependsOn(typeof(AbpAuthorizationModule))]
public class GrcApplicationContractsModule : AbpModule
{
}

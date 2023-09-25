using Pulumi;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;
using Pulumi.AzureNative.Resources;

namespace Equinor.ProCoSys.Completion.IaC.Resources;

public class KeyVault : Vault
{
    public KeyVault(string name, ResourceGroup resourceGroup, Config config)
        : base("Vault", new VaultArgs
        {
            VaultName = name,
            ResourceGroupName = resourceGroup.Name,
            Properties = new VaultPropertiesArgs
            {
                TenantId = config.Require("EquinorTenantId"),
                EnabledForDeployment = false,
                EnabledForDiskEncryption = false,
                EnabledForTemplateDeployment = true,
                AccessPolicies = new[]
                {
                    new AccessPolicyEntryArgs
                    {
                        TenantId = config.Require("EquinorTenantId"),
                        ObjectId = config.Require("PcsDeveloperObjectId"),
                        Permissions = new PermissionsArgs
                        {
                            Secrets = new InputList<Union<string, SecretPermissions>> { "all" },
                        }
                    }
                },
                NetworkAcls = new NetworkRuleSetArgs
                {
                    Bypass = "AzureServices",
                    DefaultAction = "Deny",
                    IpRules = new[]
                    {
                        new IPRuleArgs
                        {
                            Value = config.Require("BouvetStavangerIp")
                        },
                        new IPRuleArgs
                        {
                            Value = config.Require("EquinorIp")
                        },
                        new IPRuleArgs
                        {
                            Value = config.Require("BouvetVpnIpRange")
                        }
                    }
                },
                Sku = new SkuArgs
                {
                    Family = "A",
                    Name = SkuName.Standard
                }
            }
        })
    { }
}

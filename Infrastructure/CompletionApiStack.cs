using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;

namespace Equinor.ProCoSys.Completion.IaC;

public class CompletionApiStack : Stack
{
    public CompletionApiStack()
    {
        var stack = Pulumi.Deployment.Instance.StackName;
        
        //Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("ResourceGroup", new ResourceGroupArgs
        {
             ResourceGroupName = $"pcs-completion-{stack}-rg"
        });

        const string bouvetStavangerIp = "213.236.148.45";
        const string equinorIp = "143.97.2.35";
        const string bouvetVpnIpRange = "8.29.230.8/31";
        const string equinorTenantId = "3aa4a235-b6e2-48d5-9195-7fcf05b459b0";
        const string pcsDeveloperObjectId = "ff10241c-9a6c-47c3-aebd-5d8bd75ae9de";
        
        
        var keyVault = new Vault("Vault", new VaultArgs
        {
            VaultName = $"pcs-completion-{stack}-kv",
            ResourceGroupName = resourceGroup.Name,
            Properties = new VaultPropertiesArgs
            {
                
                TenantId = equinorTenantId,
                EnabledForDeployment = false,
                EnabledForDiskEncryption = false,
                EnabledForTemplateDeployment = true,
                AccessPolicies = new []
                {
                    new AccessPolicyEntryArgs
                    {
                        TenantId = equinorTenantId,
                        ObjectId = pcsDeveloperObjectId,
                        Permissions = new PermissionsArgs()
                        {
                            Secrets = new InputList<Union<string, SecretPermissions>>{"all"},
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
                            Value = bouvetStavangerIp
                        },
                        new IPRuleArgs
                        {
                            Value = equinorIp
                        },
                        new IPRuleArgs
                        {
                             Value= bouvetVpnIpRange
                        }
                    }
                },
                Sku = new SkuArgs
                {
                    Family = "A",
                    Name = SkuName.Standard
                }
            }
        });
    }
}
using Pulumi;
using Pulumi.AzureNative.AppConfiguration;
using Pulumi.AzureNative.AppConfiguration.Inputs;
using Pulumi.AzureNative.Resources;

namespace Equinor.ProCoSys.Completion.IaC.Resources;

public class AzureAppConfiguration : ConfigurationStore
{
    private readonly Input<string> _resourceGroupName;
    private readonly Input<string> _configStoreName;

    public AzureAppConfiguration(string name, ResourceGroup resourceGroup) 
        : base("AzureAppConfiguration", new ConfigurationStoreArgs
        {
            ResourceGroupName = resourceGroup.Name,
            ConfigStoreName = name,
            Sku = new SkuArgs
            {
                Name = "Standard"
            }
        })
    {
        _resourceGroupName = resourceGroup.Name;
        _configStoreName = name;
    }

    public AzureAppConfiguration AddKeyValue(string key, Input<string> value)
    {
        var keyValue = new KeyValue(key, new KeyValueArgs
        {
            ConfigStoreName = _configStoreName,
            KeyValueName = key,
            ResourceGroupName = _resourceGroupName,
            Value = value
        });
        return this;
    }
}
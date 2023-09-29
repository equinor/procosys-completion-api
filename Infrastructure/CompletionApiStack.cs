using Equinor.ProCoSys.Completion.IaC.Resources;
using Pulumi;
using Pulumi.AzureNative.Resources;

namespace Equinor.ProCoSys.Completion.IaC;

public class CompletionApiStack : Stack
{
    public CompletionApiStack()
    {
        var stack = Pulumi.Deployment.Instance.StackName;
        var config = new Config();
        //Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("ResourceGroup", new ResourceGroupArgs
        {
             ResourceGroupName = $"pcs-completion-{stack}-rg"
        });
        
        //Add key vault
        var keyVault = new KeyVault( $"pcs-completion-{stack}-kv", resourceGroup, config);
        
        //Add app insights
        var appInsight = new ApplicationInsight($"pcs-completion-{stack}-ai", resourceGroup);
        
        var appConfig = new AzureAppConfiguration($"pcs-completion-{stack}-config", resourceGroup)
            .AddKeyValue("ApplicationInsights:ConnectionString", appInsight.ConnectionString)
            .AddKeyValue("Sentinel","0");
        
        
        //Create database
        
        //add connectionstring to keyvault
        //TODO discuss how we want to do this
        
        

        //App reg
        //BlobStorage
        
        
        //ServiceBus??
        //Database?

    }
}
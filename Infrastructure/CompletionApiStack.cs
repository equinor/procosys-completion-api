using Equinor.ProCoSys.Completion.IaC.Resources;
using Pulumi;
using Pulumi.AzureNative.Resources;

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

        var keyVault = new KeyVaultComponent("Vault", resourceGroup);
        
        
        //ServiceBus
        
        //Azure App Configuration?
        
        //Database?

        
        
        
    }
}
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Resources;

namespace Equinor.ProCoSys.Completion.IaC.Resources;

public class ApplicationInsight : Component
{
    public ApplicationInsight(string name, ResourceGroup resourceGroup) 
        : base("ApplicationInsight", new ComponentArgs
        {
            ResourceName = name,
            ApplicationType = "web",
            Kind = "web",
            ResourceGroupName = resourceGroup.Name
        }){}
}
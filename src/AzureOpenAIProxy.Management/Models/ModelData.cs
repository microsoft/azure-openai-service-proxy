namespace AzureOpenAIProxy.Management.Models;

public class ModelData
{
    public List<ModelCounts> ModelCounts { get; set; } = [];
    public List<ChartData> ChartData { get; set; } = [];
}

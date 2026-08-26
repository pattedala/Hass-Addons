
namespace MQTTGatewayClient.Services;

public static class MqttTopicMatcher
{
    public static bool IsMatch(string topic, string topicFilter)
    {
        if(string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(topicFilter))
        {
            return false;
        }

        var topicLevels = topic.Split('/');
        var filterLevels = topicFilter.Split('/');
        
        for(int i = 0; i < filterLevels.Length; i++)
        {
            var filterLevel = filterLevels[i];

            if(filterLevel == "#")
            {
                return i == filterLevels.Length - 1;
            }

            if(i >= topicLevels.Length)
            {
                return false;
            }

            if(filterLevel == "+")
            {
                continue;
            }

            if(!string.Equals(
            filterLevel,
            topicLevels[i],
            StringComparison.Ordinal))
            {
                return false;
            }
        }
        return topicLevels.Length == filterLevels.Length;
    }
}
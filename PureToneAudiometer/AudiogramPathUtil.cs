namespace PureToneAudiometer
{
    public static class AudiogramPathUtil
    {
        public static string GetSvgFilePath(string dataFilePath)
        {
            return dataFilePath.Substring(0, dataFilePath.LastIndexOf('.')) + "_svg.html";
        }
    }
}
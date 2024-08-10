namespace FutureTime.StaticData
{
    public static class FTStaticData
    {
        public static List<StaticData> data { get; set; }
    }

    public enum STATIC_DATA_TYPE
    {
        RASHI,
        USER_TYPE,
        CATEGORY_TYPE
    }

    public class StaticData
    {
        public STATIC_DATA_TYPE type { get; set; }
        public List<StaticDataList> list { get; set; }
    }

    public class StaticDataList
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}

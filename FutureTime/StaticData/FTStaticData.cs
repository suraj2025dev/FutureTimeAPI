namespace FutureTime.StaticData
{
    public static class FTStaticData
    {
        public static List<StaticData> data { get; set; }

        public static string GetName(STATIC_DATA_TYPE type, string id)
        {
            var name = FTStaticData.data.Where(w => w.type == type).Select(s => s.list).ToList().First().Where(w1=>w1.id == id).Select(s1=>s1.name).FirstOrDefault();
            return name ?? "";
        }
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

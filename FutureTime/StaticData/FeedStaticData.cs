namespace FutureTime.StaticData
{
    public static class FeedStaticData
    {
        public static void Feed()
        {
            FTStaticData.data = new List<StaticData>();
            #region RASHI
            FTStaticData.data.Add(new StaticData { 
                type=STATIC_DATA_TYPE.RASHI,
                list=new List<StaticDataList>
                {
                    new StaticDataList
                {
                    id = "1",
                    name = "Aries"
                },
                new StaticDataList
                {
                    id = "2",
                    name = "Taurus"
                },
                new StaticDataList
                {
                    id = "3",
                    name = "Gemini"
                },
                new StaticDataList
                {
                    id = "4",
                    name = "Cancer"
                },
                new StaticDataList
                {
                    id = "5",
                    name = "Leo"
                },
                new StaticDataList
                {
                    id = "6",
                    name = "Virgo"
                },
                new StaticDataList
                {
                    id = "7",
                    name = "Libra"
                },
                new StaticDataList
                {
                    id = "8",
                    name = "Scorpio"
                },
                new StaticDataList
                {
                    id = "9",
                    name = "Sagittarius"
                },
                new StaticDataList
                {
                    id = "10",
                    name = "Capricorn"
                },
                new StaticDataList
                {
                    id = "11",
                    name = "Aquarius"
                },
                new StaticDataList
                {
                    id = "12",
                    name = "Pisces"
                }
                }
            });
            #endregion
        }
    }
}

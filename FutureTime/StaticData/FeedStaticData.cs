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

            #region USER TYPE
            FTStaticData.data.Add(new StaticData
            {
                type = STATIC_DATA_TYPE.USER_TYPE,
                list = new List<StaticDataList>
                {
                    new StaticDataList
                {
                    id = "1",
                    name = "Admin"
                },
                new StaticDataList
                {
                    id = "2",
                    name = "Support"
                },
                new StaticDataList
                {
                    id = "3",
                    name = "Expert"
                },
                new StaticDataList
                {
                    id = "4",
                    name = "Translator"
                },
                new StaticDataList
                {
                    id = "5",
                    name = "Reviewer"
                }
                }
            });
            #endregion

            #region CATEGORY TYPE
            FTStaticData.data.Add(new StaticData
            {
                type = STATIC_DATA_TYPE.CATEGORY_TYPE,
                list = new List<StaticDataList>
                {
                    new StaticDataList
                    {
                        id = "1",
                        name = "Horosope"
                    },
                    new StaticDataList
                    {
                        id = "2",
                        name = "Compatibility"
                    },
                    new StaticDataList
                    {
                        id = "3",
                        name = "Auspicious Time"
                    },
                    new StaticDataList
                    {
                        id = "4",
                        name = "Kundali"
                    },
                    new StaticDataList
                    {
                        id = "5",
                        name = "Support"
                    },
                    new StaticDataList
                    {
                        id = "6",
                        name = "Question"
                    },
                }
            });
            #endregion

            #region VEDIC API TYPE
            FTStaticData.data.Add(new StaticData
            {
                type = STATIC_DATA_TYPE.VEDIC_API_TYPE,
                list = new List<StaticDataList>
                {
                    new StaticDataList
                    {
                        id = "1",
                        name = "Horoscope Planet Detail (Current)"
                    },
                    new StaticDataList
                    {
                        id = "2",
                        name = "Horoscope Planet Detail (Birth)"
                    },
                    new StaticDataList
                    {
                        id = "3",
                        name = "Matching North Match With Astro"
                    },
                    new StaticDataList
                    {
                        id = "4",
                        name = "Panchang"
                    },
                    new StaticDataList
                    {
                        id = "5",
                        name = "Dasha Current Maha Dasha Full"
                    }
                }
            });
            #endregion

            #region GMT TZ
            FTStaticData.data.Add(new StaticData
            {
                type = STATIC_DATA_TYPE.GMT_TZ,
                list = new List<StaticDataList>
                {
                    new StaticDataList { id = "-12", name = "GMT-12:00" },
                    new StaticDataList { id = "-11", name = "GMT-11:00" },
                    new StaticDataList { id = "-10", name = "GMT-10:00" },
                    new StaticDataList { id = "-9.5", name = "GMT-9:30" },
                    new StaticDataList { id = "-9", name = "GMT-9:00" },
                    new StaticDataList { id = "-8", name = "GMT-8:00" },
                    new StaticDataList { id = "-7", name = "GMT-7:00" },
                    new StaticDataList { id = "-6", name = "GMT-6:00" },
                    new StaticDataList { id = "-5", name = "GMT-5:00" },
                    new StaticDataList { id = "-4", name = "GMT-4:00" },
                    new StaticDataList { id = "-3.5", name = "GMT-3:30" },
                    new StaticDataList { id = "-3", name = "GMT-3:00" },
                    new StaticDataList { id = "-2", name = "GMT-2:00" },
                    new StaticDataList { id = "-1", name = "GMT-1:00" },
                    new StaticDataList { id = "0", name = "GMT+0:00" },
                    new StaticDataList { id = "1", name = "GMT+1:00" },
                    new StaticDataList { id = "2", name = "GMT+2:00" },
                    new StaticDataList { id = "3", name = "GMT+3:00" },
                    new StaticDataList { id = "3.5", name = "GMT+3:30" },
                    new StaticDataList { id = "4", name = "GMT+4:00" },
                    new StaticDataList { id = "4.5", name = "GMT+4:30" },
                    new StaticDataList { id = "5", name = "GMT+5:00" },
                    new StaticDataList { id = "5.5", name = "GMT+5:30" },
                    new StaticDataList { id = "5.75", name = "GMT+5:45" },
                    new StaticDataList { id = "6", name = "GMT+6:00" },
                    new StaticDataList { id = "6.5", name = "GMT+6:30" },
                    new StaticDataList { id = "7", name = "GMT+7:00" },
                    new StaticDataList { id = "8", name = "GMT+8:00" },
                    new StaticDataList { id = "8.75", name = "GMT+8:45" },
                    new StaticDataList { id = "9", name = "GMT+9:00" },
                    new StaticDataList { id = "9.5", name = "GMT+9:30" },
                    new StaticDataList { id = "10", name = "GMT+10:00" },
                    new StaticDataList { id = "10.5", name = "GMT+10:30" },
                    new StaticDataList { id = "11", name = "GMT+11:00" },
                    new StaticDataList { id = "12", name = "GMT+12:00" },
                    new StaticDataList { id = "12.75", name = "GMT+12:45" },
                    new StaticDataList { id = "13", name = "GMT+13:00" },
                    new StaticDataList { id = "14", name = "GMT+14:00" }
                }
            });
            #endregion
        }
    }
}

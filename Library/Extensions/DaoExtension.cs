using Library.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Library.Extensions
{
    public static class DaoExtension
    {
        /// <summary>
        /// Cast the dictionary value to the desired type of column.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <returns></returns>

        public static Dictionary<string, object> CastParam<T>(this Dictionary<string, object> keyValuePairs,string key)
        {
            if (keyValuePairs.ContainsKey(key))
            {
                
                keyValuePairs[key] = (T)keyValuePairs[key];
            }
            return keyValuePairs;
        }

        public static string ToJson(this DataSet ds)
        {
            List<List<Dictionary<string, object>>> mainList = new List<List<Dictionary<string, object>>>();
            List<Dictionary<string, object>> list;
            foreach (DataTable table in ds.Tables)
            {
                list = new List<Dictionary<string, object>>();
                foreach (DataRow row in table.Rows)
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    list.Add(dict);
                }
                mainList.Add(list);
            }
            return JsonConvert.SerializeObject(mainList);
        }

        public static string ToJson(this DataTable table)
        {
            List<Dictionary<string, object>> list;
            list = new List<Dictionary<string, object>>();
            foreach (DataRow row in table.Rows)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }
            return JsonConvert.SerializeObject(list);
        }

        public static string ToJson(this DataRow row)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
        
            foreach (DataColumn col in row.Table.Columns)
            {
                dict[col.ColumnName] = row[col];
            }
            return JsonConvert.SerializeObject(dict);
        }

        /// <summary>
        /// It will convert table into dictionary. It when converted to json becomes:
        /// [{
        ///     "0":COL_VALUE[0],
        ///     "1":COL_VALUE[1]
        /// },
        /// {
        ///     "0":COL_VALUE[0],
        ///     "1":COL_VALUE[1]
        /// }]
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToIndexDictionary(this DataTable table)
        {
            List<Dictionary<string, object>> list;
            list = new List<Dictionary<string, object>>();
            foreach (DataRow row in table.Rows)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                int index = 0;
                foreach (DataColumn col in table.Columns)
                {
                    // If null is returned from db, it becomes {}, if it returns to react it is treated like json object. So, if {} is returned then it is converted into string
                    // {}.toString() is equal to "", so if the value returned is "" then it is returned as "" else it is returned as it is, i.e number/decimal/bool/date etc.
                    dict[index.ToString()] = (row[col].ToString()==""? row[col].ToString(): row[col]);
                    index++;
                }
                list.Add(dict);
            }
            return list;
        }



       

        public static List<T1> TableDataListMapper<T1>(this DataTable dt, int rowIndex = 0, int colIndex = 0)
        {

            List<T1> objects = new List<T1>();

            string json = dt.Rows[rowIndex][colIndex].ToString();
            objects=(JsonConvert.DeserializeObject<List<T1>> (json));

            return objects;
        }

       

        public static T1 TableDataMapper<T1>(this DataTable dt, int rowIndex = 0, int colIndex = 0)
        {

            T1 obj;

            string json = dt.Rows[rowIndex][colIndex].ToString();
            obj = (JsonConvert.DeserializeObject<T1>(json));

            return obj;
        }

        public static T1 TableDataMapper<T1>(this DataTable dt, DataRow dtRow = null, DataColumn dtCol = null)
        {

            T1 obj;

            string json = dtRow[dtCol].ToString();
            obj = (JsonConvert.DeserializeObject<T1>(json));

            return obj;
        }


    }
}

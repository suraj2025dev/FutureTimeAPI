using Library.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Data
{
    public class ActivityLogData
    {
        public string activity_detail { get; set; }
        public string activity_user { get; set; }
        public string remarks { get; set; }
        public string reference_json { get; set; }
        public DateTime activity_date { get; set; }
        public Npgsql.NpgsqlConnection connection { get; set; }

        public void Save(){
            var data=new ActivityLogData();

            string sql = "";

            sql = @"
INSERT INTO public.tbl_activity_log(
	activity_detail, activity_user, reference_json,activity_date,remarks)
	VALUES (@activity_detail, @activity_user,@reference_json,@activity_date,@remarks);
";
            Dao.Execute(sql, new
            {
                activity_detail = data.activity_detail,
                activity_user = data.activity_user,
                reference_json = new JsonbParameter(data.reference_json),
                activity_date = (data.activity_date),
                remarks = (data.remarks == null ? "" : data.remarks)
            }, data.connection);

        }
    }

    public static class ActivityLogExtension
    {
        public static void RecordLog(this ActivityLogData data)
        {
            data.Save();
        }
    }
}

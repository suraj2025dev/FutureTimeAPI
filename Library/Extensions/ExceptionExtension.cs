using Library.Exceptions;
using Library.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Library.Parameters;
using MongoDB.Driver;

namespace Library.Extensions
{
    public static class ExceptionExtension
    {
        public static ApplicationResponse GenerateResponse(this Exception ex)
        {
            Guid guid = Guid.NewGuid();
            ApplicationResponse response = new ApplicationResponse("500");
            response.error_code = "1";
            if (ex.GetType() == typeof(ErrorException))
            {
                var ex_parsed = (ErrorException)ex;
                response.error_code = ((int)ex_parsed.exception_result).ToString();
                response.data = ex_parsed.data;
                response.message = ex.Message;
                response.status_code = "200";
            }
            else if (ex.GetType() == typeof(DaoException))
            {
                var ex_parsed = (DaoException)ex;
                response.error_code = ((int)ex_parsed.exception_result).ToString();
                response.data = ex_parsed.data;
                response.message = ex.Message;
                response.status_code = "200";
            }
            else
            {
                var unknownException = true;
                //If the exception is of type Postgres
                if (ex.GetType()== typeof(Npgsql.PostgresException) || 
                    (ex.InnerException!=null && ex.InnerException.GetType()== typeof(Npgsql.PostgresException))
                    )
                {
                    Npgsql.PostgresException pgEx;
                    if (ex.InnerException != null && ex.InnerException.GetType() == typeof(Npgsql.PostgresException))
                    {
                        pgEx = (Npgsql.PostgresException)ex.InnerException;
                    }
                    else
                    {
                        pgEx = (Npgsql.PostgresException)ex;
                    }
                    if (pgEx.SqlState.Equals("40001"))
                        response.message = "Some other user processed same transaction concurrently. Please try again.";
                    else if (pgEx.SqlState.Equals("55P03"))
                        response.message = "Some other user changed the data you want to work on. Please refresh the page and continue. For more information: " + guid.ToString();
                    else if (pgEx.SqlState.Equals("P0001"))
                    {
                        response.message = pgEx.MessageText;
                        response.status_code = "200";
                        unknownException = false;//Set to false. It prevents recording of this exception. This exception is known and thrown from pgsql.
                    }
                    else
                    {
                        response.message = "Unexpected Error. For more information: " + guid.ToString();
                    }
                }else if (
                    ex.GetType() == typeof(Npgsql.NpgsqlException)
                        &&
                    ex.InnerException != null && ex.InnerException.GetType() == typeof(System.IO.IOException)
                        &&
                    ex.InnerException.InnerException != null && ex.InnerException.InnerException.GetType() == typeof(System.Net.Sockets.SocketException)
                    )
                {
                    //If it is Connection Time Out Exception.
                    response.message = "Looks like the server is taking too long to respond, please try again.";

                }
                else
                {
                    response.message = "Unexpected Error. For more information: " + guid.ToString();
                }

                if (unknownException)
                {
                    response.description = ex.Message;
                    //If not a DaoException then write in tbl_error_log.
                    ex.SaveApplicationException(guid.ToString());
                }
            }
            

            //var result = JsonConvert.SerializeObject(response);

            return response;
        }

        private static void SaveApplicationException(this Exception ex, string guid)
        {
            string sql = @"
            INSERT INTO public.tbl_error_log(guid,exception)
            VALUES(@guid,@exception::jsonb)
";
            try
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new IPAddressConverter());

                
                Dao.Execute(sql, new
                {
                    guid = guid.ToString(),
                    exception = JsonConvert.SerializeObject(ex, settings)
                });
                
            }
            catch (Exception ex1)
            {

            }
        }

        public static void WriteInLog(this Exception ex)
        {
            //var path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //path = Path.Combine(path, "errlg");
            //System.IO.Directory.CreateDirectory(path);

            ////Clean all file that is created 5 minutes earlier
            //DirectoryInfo info = new DirectoryInfo(path);
            //FileInfo[] files = info.GetFiles().Where(p => p.CreationTime<DateTime.Now.AddMinutes(-5)).ToArray();
            //foreach (FileInfo file in files)
            //{
            //    try {
            //        File.Delete(file.FullName);
            //    } catch { }
            //}

            //Guid guid = Guid.NewGuid();

            //path = Path.Combine(path, guid+".txt");

            //// Create a new file     
            //using (FileStream fs = File.Create(path))
            //{
            //    Byte[] title = new UTF8Encoding(true).GetBytes(JsonConvert.SerializeObject(ex));
            //    fs.Write(title, 0, title.Length);
            //}

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(ex, Formatting.Indented));

        }
    }
}

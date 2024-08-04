using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Library;
using Library.Data;
using User.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Library.Extensions;
using Library.Exceptions;

namespace User
{
    public class UserRepo : IUserRepo
    {
        ApplicationResponse response;
        public UserRepo()
        {
            response = new ApplicationResponse();
        }


        //for the generation of the otp
        public static class BetterRandom
        {
            private static readonly ThreadLocal<System.Security.Cryptography.RandomNumberGenerator> crng = new ThreadLocal<System.Security.Cryptography.RandomNumberGenerator>(System.Security.Cryptography.RandomNumberGenerator.Create);
            private static readonly ThreadLocal<byte[]> bytes = new ThreadLocal<byte[]>(() => new byte[sizeof(int)]);
            public static int NextInt()
            {
                crng.Value.GetBytes(bytes.Value);
                return BitConverter.ToInt32(bytes.Value, 0) & int.MaxValue;
            }
            public static double NextDouble()
            {
                while (true)
                {
                    long x = NextInt() & 0x001FFFFF;
                    x <<= 31;
                    x |= (long)NextInt();
                    double n = x;
                    const double d = 1L << 52;
                    double q = n / d;
                    if (q != 1.0)
                        return q;
                }
            }
        }

        //generated_otp
        string generated_otp = (BetterRandom.NextInt() % 1000000).ToString("000000");

        /// <summary>
        /// Handles the signup functionality with unique username ans email
        /// </summary>
        /// <param name="request">PulseRequest containing signup data with unique email and username.</param>
        /// <returns>A PulseResponse indicating the result of the signup operation </returns>
        public ApplicationResponse Signup(ApplicationRequest request)
        {
            ApplicationResponse response = new ApplicationResponse();

            try
            {
                using (var tran = Dao.CreateTransactionScope(System.Transactions.IsolationLevel.ReadCommitted))
                {
                    var created_date = DateTime.Now;
                    var data = (UserData)request.data["data"];
                    data.created_date = created_date;
                    data.created_by = request.user_id;

                    using (var conn = Dao.CreateConnection())
                    {

                        //comparing the password and confirm password before insertion into the database
                        if (data.password != data.confirm_password)
                        {
                            throw new DaoException("Password and confirm password do not match.");
                        }


                        //for validation of the email if request email and in db table if the same email exist it throw the validation
                        //email should be unique
                        var get_email = @"
                            SELECT email
                            FROM pms.tbl_user
                        ";
                        if (Dao.Query<string>(get_email, null, conn).Any(email => email.Equals(data.email, StringComparison.OrdinalIgnoreCase)))
                        {
                            throw new DaoException("Email already exists");
                        }


                        ////for username validation.if the request user name and in the db table if the same username exists it throw validation
                        ////user name also should be unique
                        //var get_user = @"
                        //    SELECT full_name
                        //    FROM pms.tbl_user
                        //";
                        //if (Dao.Query<string>(get_user, null, conn).Any(full_name => full_name.Equals(data.full_name, StringComparison.OrdinalIgnoreCase)))
                        //{
                        //    throw new DaoException("Username is already taken");
                        //}

                        //insert query
                        var sql_insert = @"
                                INSERT INTO pms.tbl_user (
	                                full_name,
	                                email,
	                                password,
	                                is_active,
	                                created_by,
	                                created_date
	                                )
                                VALUES(
	                                @full_name,
	                                @email,
	                                @password,
	                                true,
	                                'admin',     --todo
	                                @created_date
                                ) RETURNING id,uid,created_date;

                        ";
                        var dynamic_inserted = (List<dynamic>)Dao.Query<dynamic>(sql_insert, data, conn);

                        //insertion in log table
                        Dao.Execute("INSERT INTO pms.tbl_user_log SELECT * FROM pms.tbl_user WHERE id=@id AND created_date=@created_date;", dynamic_inserted[0], conn);

                    }
                    response.message = "Signed Up successfully.";
                    tran.Complete();
                }
            }
            catch (Exception ex)
            {
                return ex.GenerateResponse();
            }
            return response;
        }


        /// <summary>
        /// Handles the login functionality
        /// </summary>
        /// <param name="request">PulseRequest containing login credentials.</param>
        /// <returns>A PulseResponse indicating the result of the login operation.</returns>
        public ApplicationResponse Login(ApplicationRequest request)
        {
            ApplicationResponse response = new ApplicationResponse();

            try
            {
                using (var tran = Dao.CreateTransactionScope(System.Transactions.IsolationLevel.ReadCommitted))
                {
                    var created_date = DateTime.Now;
                    var data = (UserData)request.data["data"];
                    data.created_date = created_date;
                    data.created_by = request.user_id;
                    data.email = data.email.ToLower();
                    data.password = data.password;

                    var sql_user = @"
                SELECT
                    email,
                    password,
                    is_verified,
                    is_locked,
                    is_blocked,
                    is_active,
                    is_deleted
                FROM
                    pms.tbl_user
                WHERE
                    email = @email
                    AND password = @password
                    AND is_verified = true
                    AND is_locked = false
                    AND is_active = true
                    AND is_deleted = false";

                    using (var conn = Dao.CreateConnection())
                    {
                        


                        var get_email = @"
                            SELECT email
                            FROM pms.tbl_user
                        ";
                        if (!Dao.Query<string>(get_email, null, conn).Any(email => email.Equals(data.email, StringComparison.OrdinalIgnoreCase)))
                        {
                            throw new DaoException("Invalid Username or Password");
                        }


                        
                       
                         var get_email_password = @"
                            SELECT password
                            FROM pms.tbl_user
                            WHERE email = @email
                        ";

                         // Check if the email exists
                         var dbPassword = Dao.Query<string>(get_email_password, new { email = data.email }, conn).FirstOrDefault();
                        if (!dbPassword.Equals(data.password, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new DaoException("Invalid Username or Password");
                        }

                    }
                    response.message = "Logged in Succesfully";
                    tran.Complete();
                }
            }
            catch (Exception ex)
            {
                return ex.GenerateResponse();

            }

            return response;
        }

       
        /// <summary>
        /// Handles the  verification of OTP(one time password)
        /// </summary>
        /// <param name="request">PulseRequest containing OTP and user information</param>
        /// <returns>A PulseResponse indicating the result of the OTP verification.</returns>
        public ApplicationResponse VerifyOTPResetPassword(ApplicationRequest request)
        {
            ApplicationResponse response = new ApplicationResponse();

            try
            {
                using (var tran = Dao.CreateTransactionScope(System.Transactions.IsolationLevel.ReadCommitted))
                {
                    var created_date = DateTime.Now;
                    var data = (UserData)request.data["data"];
                    data.created_date = created_date;
                    data.created_by = request.user_id;
                    data.forget_password_otp = data.forget_password_otp;


                    using (var conn = Dao.CreateConnection())
                    {
                        var sql_user = @"
                            SELECT
                                id,
                                email,
                                forget_password_otp
                            FROM
                                pms.tbl_user
                            WHERE
                                email = @email
                                AND forget_password_otp = @forget_password_otp    
                                AND forget_password_otp_valid_till >= now()
                                AND is_verified = true
                                AND is_locked = false
                                AND is_active = true
                                AND is_deleted = false;
                        ";


                        var user = Dao.Query<UserData>(sql_user, new { email = data.email, forget_password_otp = data.forget_password_otp }, conn).FirstOrDefault();

                        if (user == null)
                        {
                            throw new DaoException("Invalid Email or OTP");
                        }

                        Dao.Execute(@"
with cte as(
    update pms.tbl_user set 
        password=@password,
        forget_password_otp=null,
        forget_password_otp_valid_till=null
    where id=@id
    returning *
)
insert into pms.tbl_user_log
select * from cte;
", new { 
                            id = user.id,
                            data.password
                        },conn);
                    }
                    response.message = "Password updated successfully.";
                    tran.Complete();
                }
            }
            catch (Exception ex)
            {
                return ex.GenerateResponse();
            }
            return response;
        }



        /// <summary>
        /// Handles the forget password functionality with the OTP generation
        /// </summary>
        /// <param name="request">PulseRequest containing user information.</param>
        /// <returns>A PulseResponse indicating the result of the forget password operation.</returns>
        public ApplicationResponse ForgetPassword(ApplicationRequest request)
        {
            ApplicationResponse response = new ApplicationResponse();

            try
            {
                using (var tran = Dao.CreateTransactionScope(System.Transactions.IsolationLevel.ReadCommitted))
                {
                    var created_date = DateTime.Now;
                    var data = (UserData)request.data["data"];
                    data.created_date = created_date;
                    data.created_by = request.user_id;
                    data.forget_password_otp = generated_otp;
                    data.email = data.email;
                    data.password = data.password;
                    data.forget_password_otp_valid_till = DateTime.Now.AddMinutes(5);
                    string user_name = "";

                    using (var conn = Dao.CreateConnection())
                    {

                        var configurationJson = File.ReadAllText("Configuration.json");
                        var appSettings = JObject.Parse(configurationJson)["App"];
                        //var emailSettingsJson = appSettings["EmailSettings"].ToString();

                        //var emailSettings = JsonConvert.DeserializeObject<EmailSettings>(emailSettingsJson);
                       
                        
                        var get_email = @"
                            SELECT email
                            FROM pms.tbl_user
                            WHERE email = @email AND is_active = true;
                        ";

                        var emailResult = Dao.ExecuteScalar<string>(get_email, new { email = data.email}, conn);

                        if (emailResult == null)
                        {
                            throw new DaoException("Invalid email address");
                        }

                        var get_users = @"
                            SELECT full_name
                            FROM pms.tbl_user
                            WHERE email = @email AND is_active = true;
                        ";

                        user_name = Dao.ExecuteScalar<string>(get_users, new { email = data.email }, conn);

                       
                        
                        //string fullName = email.full_name;

                        var sql_update = @"
                        UPDATE pms.tbl_user 
                        SET uid = public.fn_new_uuid(),
                            forget_password_otp = @forget_password_otp,
                            forget_password_otp_valid_till = @forget_password_otp_valid_till
                        WHERE email = @email AND is_deleted = false
                        RETURNING id, uid, created_date;";
                        var dynamic_inserted = (List<dynamic>)Dao.Query<dynamic>(sql_update, data, conn);


                        Dao.Execute("INSERT INTO pms.tbl_user_log SELECT * FROM pms.tbl_user WHERE id=@id AND created_date=@created_date;", dynamic_inserted[0], conn);


                        var emailBody = @$"
<p>OTP: {generated_otp} </p>
<p>User: {user_name}</p>
<p> Valid Till 5 minutes </p>

";
                            //OTPBodyTemplate.GetOneTimePasswordEmailTemplate(user_name, "Aegis Software", generated_otp, "24 HR"); // You can adjust the dynamic data here

                        var mailDto = new MailDTO
                        {
                            EmailTo = new List<string> { data.email },
                            Subject = "Reset Password OTP",
                            Body = emailBody,
                            DisplayName = AppStatic.CONFIG.App.Email.SenderName
                        };
                         EmailManagement.SendMail(mailDto);
                    }
                    response.message = "OTP Generated successfully.";
                    tran.Complete();
                }
            }
            catch (Exception ex)
            {
                return ex.GenerateResponse();
            }
            return response;
        }

        public ApplicationResponse GetUser()
        {
            return response;
        }



    }
}

using FutureTime.Filters;
using Auth;
using Library;
using Library.Data;
using Microsoft.AspNetCore.Mvc;
using User;
using User.Data;
using static System.Net.WebRequestMethods;
using MongoDB.Driver;
using static Dapper.SqlMapper;
using FutureTime.MongoDB.Model;
using FutureTime.MongoDB;
using Library.Extensions;
using Library.Exceptions;
using FutureTime.StaticData;
using MongoDB.Bson;
using Microsoft.VisualBasic;
using FutureTime.Helper;
using FutureTime.MongoDB.Data;
using MongoDB.Bson.Serialization;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;

namespace FutureTime.Controllers.Backend
{
    [Route("backend/[controller]")]
    public class FileUploadController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public FileUploadController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);
            if (!new List<int> { 1 }.Contains(request.user_type_id))//Only Admin & support
                throw new ErrorException("Not allowed");
        }

        /// <summary>
        /// RESET COUNTRY DATASET
        /// </summary>
        /// <remarks>
        /// CSV FORMAT: 
        /// 
        ///              city_ascii,
        ///              lat,
        ///              lng,
        ///              country,
        ///              iso2,
        ///              iso3,
        ///              id
        /// 
        /// </remarks>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("UploadCity")]
        public async Task<IActionResult> UploadCity(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<dynamic>().Select(record => new CityListModal
                {
                    city_ascii=record.city_ascii,
                    lat=record.lat,
                    lng=record.lng,
                    country=record.country,
                    iso2=record.iso2,
                    iso3=record.iso3,
                    city_id = record.id,
                }).ToList();

                if (records.Count > 0)
                {
                    var col = MongoDBService.ConnectCollection<CityListModal>(MongoDBService.COLLECTION_NAME.CityListModal);
                    col.DeleteMany(FilterDefinition<CityListModal>.Empty);//Delete All
                    col.InsertMany(records);//Insert New

                    // Ensure text index is created
                    var indexKeysDefinition = Builders<CityListModal>.IndexKeys
                        .Ascending("city_ascii")
                        .Ascending("country")
                        .Ascending("iso2")
                        .Ascending("iso3");

                    var indexOptions = new CreateIndexOptions
                    {
                        Collation = new Collation("en", strength: CollationStrength.Secondary) // Case-insensitive
                    };

                    var indexModel = new CreateIndexModel<CityListModal>(indexKeysDefinition, indexOptions);

                    col.Indexes.CreateOne(indexModel);
                }

                
            }

            return Ok("City Uploaded.");
        }



    }
}

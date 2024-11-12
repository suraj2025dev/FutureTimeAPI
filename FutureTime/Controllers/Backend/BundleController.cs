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
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.ObjectModel;
using SharpCompress.Common;
using Amazon.Runtime.Internal.Transform;

namespace FutureTime.Controllers.Backend
{
    [Route("backend/[controller]")]
    public class BundleController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public BundleController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillSessionDetail(request);
            if (!new List<int> { 1, 2 }.Contains(request.user_type_id))//Only Admin & support
                throw new ErrorException("Not allowed");
        }

        
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> InsertAsync([FromBody] BundleModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<BundleModel>(MongoDBService.COLLECTION_NAME.BundleModel);

                data._id = null;

                data = await ValidateData(data);

                data.created_by = request.user_id;
                data.created_date = DateTime.Now;
                data.updated_by = request.user_id;
                data.updated_date = DateTime.Now;

                //Check if question already exists.
                var filter = Builders<BundleModel>.Filter.Regex("name", new BsonRegularExpression(data.name.ToLower(), "i"));
                var exists = col.Find(filter).Any();

                if (exists)
                {
                    throw new ErrorException("Bundle with same name already exists.");
                }

                

                var result = col.InsertOneAsync(data);
                _ = MongoLogRecorder.RecordLogAsync<BundleModel>(MongoDBService.COLLECTION_NAME.BundleModel, data._id, request.user_id);
                response.message = "Bundle created successfully.";
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }

            return Ok(response);

        }

        
        [HttpGet]
        [Route("loadbasedata")]
        public async Task<IActionResult> LoadBaseDataAsync()
        {
            try
            {

                var category_list = await MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel)
                                        .Find(Builders<QuestionCategoryModel>.Filter.And(
                                                Builders<QuestionCategoryModel>.Filter.Eq("category_type_id", 3)
                                            )).ToListAsync();

                var object_ids = category_list.Select(c => ObjectId.Parse(c._id)).ToList();

                var question_list = await MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel)
                                        .Find(Builders<QuestionModel>.Filter.And(
                                                Builders<QuestionModel>.Filter.In("question_category_id", object_ids)
                                            )).ToListAsync();

                
                response.data.Add("question_for_auspicious_time", question_list.ToList().Select(s => new
                {
                    question_category_id = s._id,
                    question_category = s.question,
                    s.price
                }).ToList());
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
           
            return Ok(response);

        }

        
        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> UpdateAsync([FromBody] BundleModel data)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<BundleModel>(MongoDBService.COLLECTION_NAME.BundleModel);

                data = await ValidateData(data);

                #region Check question Name Exists in other id
                var c_filter = Builders<BundleModel>.Filter.Regex("name", new BsonRegularExpression(data.name.ToLower(), "i"));
                var idFilter = Builders<BundleModel>.Filter.Ne("_id", data._id);
                var combinedFilter = Builders<BundleModel>.Filter.And(c_filter, idFilter);
                var exists = col.Find(combinedFilter).Any();

                if (exists)
                {
                    throw new ErrorException("Bundle with same name already exists.");
                }
                #endregion



                
                var id = new ObjectId(data._id);
                var filter = Builders<BundleModel>.Filter.Eq("_id", id);

                var updateDefinitionBuilder = Builders<BundleModel>.Update;

                var updateDefinition = new List<UpdateDefinition<BundleModel>>();

                var updateFields = new Dictionary<string, object>
                {
                    { "name", data.name },
                    { "description", data.description },
                    //{ "image_blob", data.image_blob },
                    { "effective_from", data.effective_from },
                    { "effective_to", data.effective_to },
                    { "active", data.active },
                    { "price", data.price },
                    { "horoscope_question_count", data.horoscope_question_count },
                    { "compatibility_question_count", data.compatibility_question_count },
                    { "auspicious_question_id", data.auspicious_question_id },
                    { "updated_by", request.user_id},
                    { "updated_date",DateTime.Now}
            };

                if (data.image_blob != null && data.image_blob!="")
                    updateFields.Add("image_blob", data.image_blob);

                foreach (var field in updateFields)
                {
                    updateDefinition.Add(updateDefinitionBuilder.Set(field.Key, field.Value));
                }

                var combinedUpdate = updateDefinitionBuilder.Combine(updateDefinition);

                var updateResult = await col.UpdateOneAsync(filter, combinedUpdate);

                if (updateResult.ModifiedCount == 0)
                {
                    throw new ErrorException("Bundle could not be updated.");
                }
                _ = MongoLogRecorder.RecordLogAsync<BundleModel>(MongoDBService.COLLECTION_NAME.BundleModel, data._id, request.user_id);

                //col.InsertOne(data);
                response.message = "Bundle updated successfuly.";
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }

            return Ok(response);

        }

        
        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAllList()
        {
            try
            {
                var col = MongoDBService.ConnectCollection<BundleModel>(MongoDBService.COLLECTION_NAME.BundleModel);


                var items = await col.Find(new BsonDocument()).ToListAsync();

                response.data.Add("list", items);
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        
        [HttpGet]
        [Route("Get")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var col = MongoDBService.ConnectCollection<BundleModel>(MongoDBService.COLLECTION_NAME.BundleModel);

                var obj_id = new ObjectId(id);

                var filter = Builders<BundleModel>.Filter.Eq("_id", obj_id);
                var item = await col.Find(filter).FirstOrDefaultAsync();

                response.data.Add("item", item);
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        private async Task<BundleModel> ValidateData(BundleModel data)
        {
            if (data.name == null || data.name.Trim() == "")
            {
                throw new ErrorException("Please provide name");
            }
            else
            {
                data.name = data.name.Trim();
            }

            if (data.description == null || data.description.Trim() == "")
            {
                throw new ErrorException("Please provide description");
            }
            else
            {
                data.description = data.description.Trim();
            }

            if (data.image_blob == null || data.image_blob.Trim() == "")
            {
                //throw new ErrorException("Please provide image blob");
            }
            else
            {
                //data.image_blob = data.image_blob.Trim();

                string[] validImageFormats = {
                    "data:image/jpeg;base64,",
                    "data:image/png;base64,",
                    "data:image/gif;base64,",
                    "data:image/bmp;base64,",
                    "data:image/tiff;base64,",
                    "data:image/webp;base64,",
                    "data:image/x-icon;base64,"
                };

                try
                {
                    // Check if the base64 string has a data URL prefix and remove it
                    //string base64Data = data.image_blob.Split(',')[1];

                    // Convert the Base64 string to a byte array
                    byte[] fileBytes = Convert.FromBase64String(data.image_blob);

                }
                catch (Exception ex)
                {
                    throw new ErrorException("File invalid.");
                }
            }

            if (data.effective_from == DateTime.MinValue)
            {
                throw new ErrorException("Please provide effective from date");
            }

            if (data.effective_to == DateTime.MinValue)
            {
                throw new ErrorException("Please provide effective to date");
            }

            if (data.effective_from >= data.effective_to)
            {
                throw new ErrorException("Effective from date must be earlier than effective to date");
            }

            if (data.price <= 0)
            {
                throw new ErrorException("Please provide a valid price");
            }

            if (data.horoscope_question_count < 0)
            {
                throw new ErrorException("Please provide a valid horoscope question count");
            }

            if (data.compatibility_question_count < 0)
            {
                throw new ErrorException("Please provide a valid compatibility question count");
            }

            if (data.auspicious_question_id == null || data.auspicious_question_id.Trim() == "")
            {
                //User can choose 0 aus qsn.
            }
            else
            {
                var qsn_col = MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel);

                //var qsn_id = new ObjectId(data.auspicious_question_id);

                if(!ObjectId.TryParse(data.auspicious_question_id,out ObjectId qsn_id))
                {
                    throw new ErrorException("Please choose valid auspicious question.");
                }

                var qsn_filter = Builders<QuestionModel>.Filter.Eq("_id", qsn_id);
                var qsn_item = await qsn_col.Find(qsn_filter).FirstOrDefaultAsync();
                /////////////
                if (qsn_item == null)
                {
                    throw new ErrorException("Please choose valid auspicious question.");
                }
                var qsn_cat_col = MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel);

                var qsn_cat_id = new ObjectId(qsn_item.question_category_id);

                var qsn_cat_filter1 = Builders<QuestionCategoryModel>.Filter.Eq("_id", qsn_cat_id);
                var qsn_cat_filter2 = Builders<QuestionCategoryModel>.Filter.Eq("category_type_id", 3);
                var qsn_cat_filter3 = Builders<QuestionCategoryModel>.Filter.Eq("active", true);
                var qsn_cat_item = await qsn_cat_col.Find(Builders<QuestionCategoryModel>.Filter.And(qsn_cat_filter1, qsn_cat_filter2, qsn_cat_filter3)).FirstOrDefaultAsync();
                ///////////
                if (qsn_cat_item == null)
                {
                    throw new ErrorException("Please pick valid questions from Auspicious Time.");
                }

            }
            return data;
        }
    }
}

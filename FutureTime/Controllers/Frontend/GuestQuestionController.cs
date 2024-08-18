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
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace FutureTime.Controllers.Backend
{
    [Route("frontend/[controller]")]
    public class GuestQuestionController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public GuestQuestionController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillGuestSessionAsync(request);

        }

       
        

        [GuestAuthFilter]
        [HttpGet]
        [Route("GetQuestionCategory")]
        public async Task<IActionResult> GetQuestionCategory(int type_id)
        {
            if (request.guest_id == null)
            {
                response = new ApplicationResponse("401");

                return StatusCode(401, response);
            }
            try
            {
               
                var category_list = await MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel)
                        .Find(Builders<QuestionCategoryModel>.Filter.And(
                                (
                                    type_id == 0 ?
                                        Builders<QuestionCategoryModel>.Filter.Empty
                                    :
                                        Builders<QuestionCategoryModel>.Filter.Eq("category_type_id", type_id)
                                ),
                                Builders<QuestionCategoryModel>.Filter.Eq("active", true)
                            )).ToListAsync();
                response.data.Add("question_category", category_list.OrderBy(o => o.order_id).ToList());
                
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }

        [GuestAuthFilter]
        [HttpGet]
        [Route("GetQuestion")]
        public async Task<IActionResult> GetQuestion(int type_id, string question_category_id)
        {
            if (request.guest_id == null)
            {
                response = new ApplicationResponse("401");

                return StatusCode(401, response);
            }
            try
            {
                if (type_id != 0)
                {
                    var category_list = await MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel)
                            .Find(Builders<QuestionCategoryModel>.Filter.And(
                                    (
                                        type_id == 0 ?
                                            Builders<QuestionCategoryModel>.Filter.Empty
                                        :
                                            Builders<QuestionCategoryModel>.Filter.Eq("category_type_id", type_id)
                                    ),
                                    Builders<QuestionCategoryModel>.Filter.Eq("active", true)
                                )).ToListAsync();
                    
                    var question_list = await MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel)
                            .Find(Builders<QuestionModel>.Filter.And(
                                    Builders<QuestionModel>.Filter.In("question_category_id", category_list.Select(s => ObjectId.Parse(s._id))),
                                    Builders<QuestionModel>.Filter.Eq("active", true)
                                )).ToListAsync();

                    var questions = new List<QuestionModel>();
                    category_list.OrderBy(o => o.order_id).ToList().ForEach(f => { 
                        questions.AddRange(question_list.Where(w=>w.question_category_id == f._id).OrderBy(o=>o.order_id).ToList());
                    });

                    response.data.Add("questions", questions);
                }
                else
                {
                    if (!ObjectId.TryParse(question_category_id, out ObjectId obj_id))
                    {
                        throw new ErrorException("Invalid category");
                    }
                    var question_list = await MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel)
                            .Find(Builders<QuestionModel>.Filter.And(
                                    Builders<QuestionModel>.Filter.Eq("question_category_id", obj_id),
                                    Builders<QuestionModel>.Filter.Eq("active", true)
                                )).ToListAsync();

                    
                    response.data.Add("questions", question_list.OrderBy(o=>o.order_id));
                }
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            //response.message = "Daily Rashi Updates saved for the day.";
            return Ok(response);

        }


    }
}

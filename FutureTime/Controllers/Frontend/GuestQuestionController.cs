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
using System.Globalization;

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
        public async Task<IActionResult> GetQuestion(int type_id, string question_category_id, bool is_bundle = false)
        {
            if (request.guest_id == null)
            {
                response = new ApplicationResponse("401");

                return StatusCode(401, response);
            }

            var questions = new List<QuestionModel>();

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

            try
            {

                if (type_id != 0)
                {
                    
                    var question_list = await MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel)
                            .Find(Builders<QuestionModel>.Filter.And(
                                    Builders<QuestionModel>.Filter.In("question_category_id", category_list.Select(s => ObjectId.Parse(s._id))),
                                    Builders<QuestionModel>.Filter.Eq("active", true),
                                    Builders<QuestionModel>.Filter.Eq("is_bundle", is_bundle)
                                )).ToListAsync();

                    
                    category_list.OrderBy(o => o.order_id).ToList().ForEach(f => { 
                        questions.AddRange(question_list.Where(w=>w.question_category_id == f._id).OrderBy(o=>o.order_id).ToList());
                    });

                    questions = questions;
                }
                else
                {
                    if (ObjectId.TryParse(question_category_id, out ObjectId obj_id))
                    {
                        var question_list = await MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel)
                            .Find(Builders<QuestionModel>.Filter.And(
                                    Builders<QuestionModel>.Filter.Eq("question_category_id", obj_id),
                                    Builders<QuestionModel>.Filter.Eq("active", true),
                                    Builders<QuestionModel>.Filter.Eq("is_bundle", is_bundle)
                                )).ToListAsync();


                        questions = question_list.OrderBy(o => o.order_id).ToList();
                    }
                    else
                    {
                        var question_list = await MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel)
                                                    .Find(Builders<QuestionModel>.Filter.And(
                                                            Builders<QuestionModel>.Filter.Eq("active", true),
                                                            Builders<QuestionModel>.Filter.Eq("is_bundle", is_bundle)
                                                        )).ToListAsync();


                        questions = question_list.OrderBy(o => o.order_id).ToList();
                    }
                    
                }
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }

            DateTime serverDate = DateTime.UtcNow;

            var filtered_questions = questions.Where(item =>
            {
                DateTime? fromDate = string.IsNullOrEmpty(item.effective_from) ? null : DateTime.ParseExact(item.effective_from, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime? toDate = string.IsNullOrEmpty(item.effective_to) ? null : DateTime.ParseExact(item.effective_to, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                return (!fromDate.HasValue || fromDate.Value <= serverDate) &&
                       (!toDate.HasValue || toDate.Value >= serverDate);
            }).Select(s=> new { 
                created_by=s.created_by,
                active=s.active,
                created_date=s.created_date,
                discount_amount=s.discount_amount,
                effective_from=s.effective_from,
                effective_to=s.effective_to,
                image_blob = s.image_blob,
                is_bundle = s.is_bundle,
                is_initial_offerings = s.is_initial_offerings,
                order_id=s.order_id,
                price=s.price,
                price_before_discount=s.price_before_discount,
                question=s.question,
                question_category_id=s.question_category_id,
                updated_by=s.updated_by,
                updated_date=s.updated_date,
                _id = s._id,
                category_type_id = category_list.Where(w=>w._id == s.question_category_id).FirstOrDefault().category_type_id
            }).ToList();

            response.data.Add("questions", filtered_questions);
            return Ok(response);

        }


    }
}

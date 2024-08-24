

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
using FutureTime.MongoDB.Data;

namespace FutureTime.Controllers.Backend
{
    [Route("frontend/[controller]")]
    public class GuestInquiryController : ControllerBase
    {
        ApplicationResponse response;
        ApplicationRequest request;

        public GuestInquiryController(IHttpContextAccessor httpContextAccessor)
        {
            response = new ApplicationResponse();
            request = new ApplicationRequest();
            request = httpContextAccessor.FillGuestSessionAsync(request);

        }

       
        

        [GuestAuthFilter]
        [HttpPost]
        [Route("StartInquiryProcess")]
        public async Task<IActionResult> StartInquiryProcess([FromBody] StartInquiryProcessDTO dto)
        {
            if (request.guest_id == null)
            {
                response = new ApplicationResponse("401");

                return StatusCode(401, response);
            }
            try
            {
                var current_date = DateTime.Now;
                //Validation & Data Filling
                var new_inquiry = new StartInquiryProcessModel { 
                    assignee_id=null,
                    created_by=request.guest_id,
                    created_date= current_date,
                    guest_id=request.guest_id,
                    inquiry_payment_status = INQUIRY_PAYMENT_STATUS.Pending,
                    inquiry_status = INQUIRY_STATUS.New,
                    inquiry_type = dto.inquiry_type,
                    inquiry_bundle = null,
                    inquiry_regular = null,
                    inquiry_number =  Guid.NewGuid().ToString().Replace("-",""),
                };

                if(new_inquiry.inquiry_type == INQUIRY_TYPE.Regular)
                {
                    var qsn_detail = MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel)
                                    .Find(Builders<QuestionModel>.Filter.And(
                                                Builders<QuestionModel>.Filter.Eq("_id", dto.inquiry_regular.question_id),
                                                Builders<QuestionModel>.Filter.Eq("active", true)
                                            )).FirstOrDefault();
                    //var qsn_category_detail = MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel)
                    //                            .Find(Builders<QuestionCategoryModel>.Filter.And(
                    //                                Builders<QuestionCategoryModel>.Filter.Eq("_id", qsn_detail.question_category_id),
                    //                                Builders<QuestionCategoryModel>.Filter.Eq("active", true)
                    //                            )).FirstOrDefault();

                    if (qsn_detail == null)
                    {
                        throw new ErrorException("Question choosen in invalid");
                    }

                    //if (qsn_category_detail == null)
                    //{
                    //    throw new ErrorException("Question Category is not valid.");
                    //}

                    new_inquiry.inquiry_regular = new InquiryRegular { 
                        question_id = dto.inquiry_regular.question_id,
                        price=qsn_detail.price,
                        question=qsn_detail.question,
                        //question_category=qsn_category_detail.category,
                        //question_category_id=qsn_category_detail._id,
                        reading_activity = null
                    };
                }
                else
                {
                    var all_qsn_id = new List<string?>();
                    if (dto.inquiry_bundle.horoscope_question == null)
                    {
                        dto.inquiry_bundle.horoscope_question = new List<InquiryRegularDTO>();
                    }
                    if (dto.inquiry_bundle.compatibility_question == null)
                    {
                        dto.inquiry_bundle.compatibility_question = new List<InquiryRegularDTO>();
                    }

                    var bundle_data = MongoDBService.ConnectCollection<BundleModel>(MongoDBService.COLLECTION_NAME.BundleModel)
                                   .Find(Builders<BundleModel>.Filter.And(
                                               Builders<BundleModel>.Filter.Eq("_id", dto.inquiry_bundle.bundle_id),
                                               Builders<BundleModel>.Filter.Eq("active", true)
                                           )).FirstOrDefault();
                    if (bundle_data == null)
                    {
                        throw new ErrorException("Bundle is not valid.");
                    }


                    all_qsn_id = dto.inquiry_bundle.horoscope_question.Select(s => s.question_id).ToList();
                    all_qsn_id.AddRange(dto.inquiry_bundle.compatibility_question.Select(s => s.question_id).ToList());
                    if(bundle_data.auspicious_question_id!=null)
                        all_qsn_id.Add(bundle_data.auspicious_question_id);

                    var qsn_list_detail = MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel)
                                    .Find(Builders<QuestionModel>.Filter.And(
                                                Builders<QuestionModel>.Filter.In("_id", all_qsn_id),
                                                Builders<QuestionModel>.Filter.Eq("active", true)
                                            )).ToList();


                    var auspicious_question = new List<InquiryRegular>();
                    var aus_qsn = qsn_list_detail.Where(w => w._id == bundle_data.auspicious_question_id).FirstOrDefault();
                    if (aus_qsn == null)
                    {
                        throw new ErrorException("Invalid Auspicious Question Choosen.");
                    }
                    auspicious_question.Add(new InquiryRegular
                    {
                        price = aus_qsn.price,
                        question_id = aus_qsn._id,
                        question = aus_qsn.question,
                        reading_activity = null
                    });

                    var horoscope_question = new List<InquiryRegular>();
                    dto.inquiry_bundle.horoscope_question.ForEach(f => {
                        var qsn = qsn_list_detail.Where(w => w._id == f.question_id).FirstOrDefault();
                        if (qsn == null)
                        {
                            throw new ErrorException("Invalid Horoscope Question Choosen.");
                        }
                        horoscope_question.Add(new InquiryRegular
                        {
                            price = qsn.price,
                            question_id = qsn._id,
                            question = qsn.question,
                            reading_activity = null
                        });
                    });

                    var compatibility_question = new List<InquiryRegular>();
                    dto.inquiry_bundle.compatibility_question.ForEach(f => {
                        var qsn = qsn_list_detail.Where(w => w._id == f.question_id).FirstOrDefault();
                        if (qsn == null)
                        {
                            throw new ErrorException("Invalid Horoscope Question Choosen.");
                        }
                        compatibility_question.Add(new InquiryRegular
                        {
                            price = qsn.price,
                            question_id = qsn._id,
                            question = qsn.question,
                            reading_activity = null
                        });
                    });

                    new_inquiry.inquiry_bundle = new InquiryBundle
                    {
                      auspicious_question = auspicious_question,
                      compatibility_question=  compatibility_question,
                      horoscope_question= horoscope_question,
                      bundle_id=dto.inquiry_bundle.bundle_id,
                      bundle_name=bundle_data.name,
                      description=bundle_data.description,
                      price=bundle_data.price
                    };

                    //Validation
                    if(DateTime.Now.Date >= bundle_data.effective_from.Date && DateTime.Now.Date <= bundle_data.effective_to)
                    {

                    }
                    else
                    {
                        throw new ErrorException("This bundle is expired.");
                    }

                    if(bundle_data.horoscope_question_count != new_inquiry.inquiry_bundle.horoscope_question.Count)
                    {
                        throw new ErrorException("Total question in horoscope must be "+bundle_data.horoscope_question_count);
                    }

                    if (bundle_data.compatibility_question_count != new_inquiry.inquiry_bundle.compatibility_question.Count)
                    {
                        throw new ErrorException("Total question in compatibility must be " + bundle_data.compatibility_question_count);
                    }




                }

                var col = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel);
                await col.InsertOneAsync(new_inquiry);

                _ = MongoLogRecorder.RecordLogAsync<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel, new_inquiry._id, request.user_id);

                response.message = "Please complete the payment process.";
                response.data.Add("inquiry_number", new_inquiry.inquiry_number);

            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            return Ok(response);

        }


    }
}

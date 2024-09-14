

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
using DbUp;
using System.Xml.Linq;

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

                # region Validate Profile1
                if(dto.profile1 == null)
                {
                    throw new ErrorException("Profile 1 data is required.");
                }
                if (dto.profile1.name == "" || dto.profile1.name == null)
                {
                    throw new ErrorException("Profile 1 name is required.");
                }

                if (dto.profile1.dob == "" || dto.profile1.dob == null)
                {
                    throw new ErrorException("Profile 1 dob is required.");
                }

                if (!DateTime.TryParse(dto.profile1.dob, out DateTime _dob))
                {
                    throw new ErrorException("Please provide valid dob in format like 2024-01-01");
                }

                if (dto.profile1.tob == "" || dto.profile1.tob == null)
                {
                    throw new ErrorException("Profile 1 tob is required.");
                }

                string time_pattern = @"^([01]\d|2[0-3]):([0-5]\d)$";
                Regex regex = new Regex(time_pattern);
                if (!regex.IsMatch(dto.profile1.tob))
                {
                    throw new ErrorException("Please provide valid tob in HH:MM format from 00:00 to 23:59");
                }

                if (dto.profile1.city_id == "" || dto.profile1.city_id == null)
                {
                    throw new ErrorException("Profile 1 city is required.");
                }

                #endregion

                var current_date = DateTime.Now;
                //Validation & Data Filling
                var new_inquiry = new StartInquiryProcessModel { 
                    assignee_id=null,
                    created_by=request.guest_id,
                    created_date= current_date,
                    guest_id=request.guest_id,
                    inquiry_payment_status = INQUIRY_PAYMENT_STATUS.Paid,
                    inquiry_status = INQUIRY_STATUS.Pending,
                    inquiry_state = INQUIRY_STATE.New,
                    inquiry_type = dto.inquiry_type,
                    inquiry_bundle = null,
                    inquiry_regular = null,
                    inquiry_number =  Guid.NewGuid().ToString().Replace("-",""),
                    is_read = false,
                    profile1=dto.profile1,
                    profile2=dto.profile2,
                    active=true,
                    updated_by=request.guest_id,
                    updated_date= current_date
                };

                if(new_inquiry.inquiry_type == INQUIRY_TYPE.Regular)
                {
                    var qsn_detail = MongoDBService.ConnectCollection<QuestionModel>(MongoDBService.COLLECTION_NAME.QuestionModel)
                                    .Find(Builders<QuestionModel>.Filter.And(
                                                Builders<QuestionModel>.Filter.Eq("_id", dto.inquiry_regular.question_id),
                                                Builders<QuestionModel>.Filter.Eq("active", true)
                                            )).FirstOrDefault();
                    
                    if (qsn_detail == null)
                    {
                        throw new ErrorException("Question choosen in invalid");
                    }

                    var qsn_cat_detail = MongoDBService.ConnectCollection<QuestionCategoryModel>(MongoDBService.COLLECTION_NAME.QuestionCategoryModel)
                                    .Find(Builders<QuestionCategoryModel>.Filter.And(
                                                Builders<QuestionCategoryModel>.Filter.Eq("_id", qsn_detail.question_category_id)
                                                //Builders<QuestionCategoryModel>.Filter.Eq("active", true)
                                            )).FirstOrDefault();

                    if(qsn_cat_detail == null)
                    {
                        throw new ErrorException("Question Category in invalid");
                    }

                    if(qsn_cat_detail.category_type_id == 2)
                    {
                        //Compatibility Verification
                        #region Validate Profile2
                        if (dto.profile2 == null)
                        {
                            throw new ErrorException("Profile 2 data is required.");
                        }
                        if (dto.profile2.name == "" || dto.profile2.name == null)
                        {
                            throw new ErrorException("Profile 2 name is required.");
                        }

                        if (dto.profile2.dob == "" || dto.profile2.dob == null)
                        {
                            throw new ErrorException("Profile 2 dob is required.");
                        }

                        if (!DateTime.TryParse(dto.profile2.dob, out DateTime _dob2))
                        {
                            throw new ErrorException("Please provide valid dob in format like 2024-01-01");
                        }

                        if (dto.profile2.tob == "" || dto.profile2.tob == null)
                        {
                            throw new ErrorException("Profile 2 tob is required.");
                        }

                        string time_pattern2 = @"^([01]\d|2[0-3]):([0-5]\d)$";
                        Regex regex2 = new Regex(time_pattern2);
                        if (!regex2.IsMatch(dto.profile2.tob))
                        {
                            throw new ErrorException("Please provide valid tob in HH:MM format from 00:00 to 23:59");
                        }

                        if (dto.profile2.city_id == "" || dto.profile2.city_id == null)
                        {
                            throw new ErrorException("Profile 2 city is required.");
                        }

                        #endregion

                    }

                    //Aus Time
                    DateTime auspicious_from_date1 = DateTime.MinValue;
                    if (qsn_cat_detail.category_type_id == 3)
                    {
                        if(dto.auspicious_from_date == "" || dto.auspicious_from_date == null)
                        {
                            throw new ErrorException("Please provide auspicious time prediction start date (auspicious_from_date).");
                        }
                        
                        if (!DateTime.TryParse(dto.auspicious_from_date, out auspicious_from_date1))
                        {
                            throw new ErrorException("Please provide valid auspicious_from_date in format like 2024-01-01");
                        }
                    }


                    new_inquiry.inquiry_regular = new InquiryRegular { 
                        question_id = dto.inquiry_regular.question_id,
                        price=qsn_detail.price,
                        question=qsn_detail.question,
                        reading_activity = null,
                        auspicious_from_date = qsn_cat_detail.category_type_id == 3 ? auspicious_from_date1 : null,
                        category_type_id=qsn_cat_detail.category_type_id
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

                _ = MongoLogRecorder.RecordLogAsync<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel, new_inquiry._id, request.user_id);

                //response.message = "Please complete the payment process.";
                response.message = "Purchase successfull.";
                response.data.Add("inquiry_number", new_inquiry.inquiry_number);

            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            return Ok(response);

        }

        [GuestAuthFilter]
        [HttpGet]
        [Route("MyInquiries")]
        public async Task<IActionResult> MyInquiries(string subsribed_on_from=null, 
                                                        string subsribed_on_to=null, 
                                                        string inquiry_number = null, 
                                                        decimal? price_from = null,
                                                        decimal? price_to = null
                                                    )
        {
            if (request.guest_id == null)
            {
                response = new ApplicationResponse("401");

                return StatusCode(401, response);
            }
            try
            {
                if (subsribed_on_from!=null && !DateTime.TryParse(subsribed_on_from, out DateTime _subsribed_on_from))
                {
                    throw new ErrorException("Please provide valid subsribed_on_from in format like 2024-01-01");
                }
                else
                {
                    _subsribed_on_from = DateTime.Now;

                }

                if (subsribed_on_to != null && !DateTime.TryParse(subsribed_on_to, out DateTime _subsribed_on_to))
                {
                    throw new ErrorException("Please provide valid subsribed_on_to in format like 2024-01-01");
                }
                else
                {
                    _subsribed_on_to = DateTime.Now;

                }

                var filters = Builders<StartInquiryProcessModel>.Filter.And(
                                    Builders<StartInquiryProcessModel>.Filter.Eq("guest_id", request.guest_id),
                                    Builders<StartInquiryProcessModel>.Filter.Eq("active", true)
                                );

                if (_subsribed_on_from != null && subsribed_on_to != null)
                {
                    // Add date filter if both dates are provided
                    var dateFilter = Builders<StartInquiryProcessModel>.Filter.And(
                        Builders<StartInquiryProcessModel>.Filter.Gte(x => x.created_date.Date, _subsribed_on_from),
                        Builders<StartInquiryProcessModel>.Filter.Lt(x => x.created_date.Date, _subsribed_on_to)
                    );
                    filters = Builders<StartInquiryProcessModel>.Filter.And(filters, dateFilter);
                }

                if (inquiry_number != null)
                {
                    filters = Builders<StartInquiryProcessModel>.Filter.And(filters,
                                Builders<StartInquiryProcessModel>.Filter.And(
                                    Builders<StartInquiryProcessModel>.Filter.Regex("inquiry_number", new BsonRegularExpression($"^{inquiry_number}$", "i"))
                                )
                        );
                }

                if (price_from != null && price_to != null)
                {
                    // Add date filter if both dates are provided
                    var pricefilter = Builders<StartInquiryProcessModel>.Filter.And(
                        Builders<StartInquiryProcessModel>.Filter.Gte(x => x.inquiry_regular.price, price_from),
                        Builders<StartInquiryProcessModel>.Filter.Lt(x => x.inquiry_regular.price, price_to)
                    );
                    filters = Builders<StartInquiryProcessModel>.Filter.And(filters, pricefilter);
                }

                var inquiries = MongoDBService.ConnectCollection<StartInquiryProcessModel>(MongoDBService.COLLECTION_NAME.StartInquiryProcessModel)
                                    .Find(filters).ToList()
                                    .Select(s=>new 
                                    {
                                        s.inquiry_regular.question,
                                        s.inquiry_regular.price,
                                        s.inquiry_number,
                                        payment_successfull = s.inquiry_payment_status==INQUIRY_PAYMENT_STATUS.Paid?true:false,
                                        purchased_on = s.created_date,
                                        s.profile1,
                                        s.profile2,
                                        s.inquiry_regular.auspicious_from_date,
                                        s.inquiry_regular.category_type_id,
                                        is_replied = s.inquiry_state == INQUIRY_STATE.Published?true:false,  
                                        s.is_read
                                    }).OrderByDescending(o=>o.purchased_on).ToList();

                response.data.Add("inquiries", inquiries);
            }
            catch (Exception ex)
            {
                response = ex.GenerateResponse();
            }
            return Ok(response);

        }


    }
}

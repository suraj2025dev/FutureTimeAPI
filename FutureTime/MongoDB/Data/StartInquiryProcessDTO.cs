
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using FutureTime.MongoDB.Model;

namespace FutureTime.MongoDB.Data
{
   
    public class StartInquiryProcessDTO 
    {
        public INQUIRY_TYPE inquiry_type { get; set; }
        //public INQUIRY_STATUS InquiryStatus { get; set; }
        //public INQUIRY_PAYMENT_STATUS InquiryPaymentStatus { get; set; }
        //public string? guest_id { get; set; }//Person who paid for the service
        public InquiryRegularDTO inquiry_regular { get; set; }
        public InquiryBundleDTO inquiry_bundle { get; set; }
        public InqurityGuestProfile profile1 { get; set; }
        public InqurityGuestProfile profile2 { get; set; }
        public string auspicious_from_date { get; set; }

    }

    
    public class InquiryRegularDTO
    {
        public string question_id { get; set; }//FK
    }

    public class InquiryBundleDTO
    {
        public string? bundle_id { get; set; }
        //Validate Count
        public List<InquiryRegularDTO> horoscope_question { get; set; }
        //Validate Count
        public List<InquiryRegularDTO> compatibility_question { get; set; }
    }

}

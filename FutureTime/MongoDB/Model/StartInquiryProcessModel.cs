
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Model
{
    public enum INQUIRY_TYPE
    {
        Regular = 0,
        Bundle = 1
    }

    public enum INQUIRY_STATUS
    {
        Pending = 1,
        Completed=2
        //Cancelled=3
    }

    public enum INQUIRY_PAYMENT_STATUS
    {
        Pending = 0,
        Paid = 1,
        Failed = 2
    }

    public enum INQUIRY_STATE
    {
        New = 0,
        Expert = 1,
        Translator = 2,
        Reviewer = 3,
        Published = 4,
        Cancelled = 5
    }

    public class StartInquiryProcessModel : MasterModel
    {
        //[BsonElement("items")]
        //[JsonPropertyName("items")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string inquiry_number { get; set; }//auto generated, for tracking
        public INQUIRY_TYPE inquiry_type { get; set; }
        public INQUIRY_STATUS inquiry_status { get; set; }
        public INQUIRY_PAYMENT_STATUS inquiry_payment_status { get; set; }
        public INQUIRY_STATE inquiry_state { get; set; }
        public string? guest_id { get; set; }//Person who paid for the service
        public string? assignee_id { get; set; }//Id of person who is assigned. Only Assignee person can update description
        public string? comment_for_assignee { get; set; }//Id of person who is assigned. Only Assignee person can update description
        public InquiryRegular inquiry_regular { get; set; }
        public InquiryBundle inquiry_bundle { get; set; }
        public InqurityGuestProfile profile1 { get; set; }
        public InqurityGuestProfile profile2 { get; set; }
        public List<VedicAPIResponse> vedic_api_response_list { get; set; }
        public bool is_read { get; set; }
        public string final_reading { get; set; }
        public int rating { get; set; }
        public bool active { get; set; }
    }

    public class InquiryReading
    {
        public string assignee_id { get; set; }
        public string description { get; set; }
        public DateTime updated_on { get; set; }
    }

    public class InquiryRegular
    {
        public int category_type_id { get; set; }
        public string? question_id { get; set; }//FK
        public string question { get; set; }//To Be filled from backend
        public decimal price { get; set; }//To Be filled from backend
        public DateTime? auspicious_from_date { get; set; }
        public DateTime? horoscope_from_date { get; set; }
        public string publish_content { get; set; }
        //Validate Active
        public List<InquiryReading> reading_activity { get; set; }// Only Assignee person can update
    }

    public class InquiryBundle
    {
        public string? bundle_id { get; set; }
        public string bundle_name { get; set; }//To Be Filled From Backend
        public string description { get; set; }//To Be Filled From Backend
        //Validate Effective From & To, Active
        public decimal price { get; set; }
        //Validate Count
        public List<InquiryRegular> horoscope_question { get; set; }
        //Validate Count
        public List<InquiryRegular> compatibility_question { get; set; }
        //Validate Count
        public List<InquiryRegular> auspicious_question { get; set; }
    }

    public class InqurityGuestProfile
    {
        public string name { get; set; }
        public string city_id { get; set; }
        public string dob { get; set; }
        public string tob { get; set; }
        public decimal tz { get; set; }
        public CityListModal city { get; set; }
    }

    public class VedicAPIResponse
    {
        public string vedic_api_type_id { get; set; }
        public string vedic_api_response { get; set; }
    }

}

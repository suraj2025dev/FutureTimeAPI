namespace FutureTime.MongoDB.Data
{
    public class ChangeInquiryAssigneeDTO
    {
        public string comment { get; set; }
        public string assignee_id { get; set; }
        public string inquiry_id { get; set; }
    }

    public class RejectInquiryDTO
    {
        public string comment { get; set; }
        public string inquiry_id { get; set; }
    }
}

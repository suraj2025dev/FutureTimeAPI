namespace FutureTime.MongoDB.Data
{
    public class GetAllQuestionsData
    {
        public string? question { get; set; }
        public string? question_category_id { get; set; }
        public bool? active { get; set; }
        public decimal? price { get; set; }//net price after discount
        public decimal? price_before_discount { get; set; }//After discount & bundle concept added.
        public bool? is_initial_offerings { get; set; }
        public bool? is_bundle { get; set; }
        public string? effective_from { get; set; }
        public string? effective_to { get; set; }
        public decimal? discount_amount { get; set; }
        public int? page_number { get; set; }
        public int? page_size { get; set; }
    }
}

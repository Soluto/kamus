using System;
namespace CustomResourceDescriptorController.Models
{
    public class ConversionReview
    {
        public string Kind { get; set; }
        public string ApiVersion { get; set; }
        public ConversionReviewRequest Request { get; set; }
        public ConversionReviewResponse Response { get; set; }
    }
}

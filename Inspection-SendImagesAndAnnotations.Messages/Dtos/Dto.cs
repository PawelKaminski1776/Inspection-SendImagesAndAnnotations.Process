using NServiceBus;

namespace InspectionSendImagesAndAnnotations.Messages.Dtos
{
    public class InspectionRequest : IMessage
    {
        public List<ImageAnnotationDto> data { get; set; } = new();
        public string ModelUrl { get; set; }

        public int numofimgs { get; set; }
        public string inspectionname { get; set; }
        public string county { get; set; }
    }

    

    public class ImageAnnotationDto
    {
        public string image { get; set; }
        public string image_name { get; set; }
        public Dictionary<string, List<AnnotationDto>> annotations { get; set; } = new();
    }

    public class AnnotationDto
    {
        public string id { get; set; } 
        public string class_name { get; set; }
        public List<double> bounding_box { get; set; } = new();
        public double score { get; set; }
    }

    public class ImageTrainingRequest : IMessage
    {
        public List<ImageAnnotationDto> data { get; set; } = new();
        public string ModelUrl { get; set; }
    }

    public class ImageTrainingResponse : IMessage
    {
        public string id { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string overallLoss { get; set; }
    }

    public class InspectionResponse : IMessage
    {
        public string Message { get; set; }
    }

}

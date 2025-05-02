using System.Text.Json;
using System.Text.Json.Serialization;
using InspectionSendImagesAndAnnotations.Channel;
using InspectionSendImagesAndAnnotations.Channel.Services;
using InspectionSendImagesAndAnnotations.Messages.Dtos;
using Newtonsoft.Json;

namespace InspectionSendImagesAndAnnotations.Handlers
{
    public class MyHandler : IHandleMessages<InspectionRequest>
    {
        private PythonAPI _pythonApi;
        private InspectionService inspectionService;
        private string numofimgs;
        private string inspectionname;
        private string county;
        public MyHandler(PythonAPI pythonApi, InspectionService _inspectionService)
        {
            this._pythonApi = pythonApi;
            this.inspectionService = _inspectionService;

        }

        public async Task Handle(InspectionRequest message, IMessageHandlerContext context)
        {
            try
            {
                this.numofimgs = message.numofimgs.ToString();
                this.inspectionname = message.inspectionname;
                this.county = message.county;


                ImageTrainingRequest request = new ImageTrainingRequest
                {
                    data = message.data,
                    ModelUrl = message.ModelUrl
                };

                string apiResponse = _pythonApi.SendToImageTrainingAPI("/SaveSemiAutomaticTraining", request).Result;
                Console.WriteLine(apiResponse);

                ImageTrainingResponse response;
                InspectionResponse frontendresponse = new InspectionResponse { Message = "Failed" };
                try
                {

                    response = JsonConvert.DeserializeObject<ImageTrainingResponse>(apiResponse);
                    
                    frontendresponse.Message = await inspectionService.InsertOnFinishedImageScanning(response, this.inspectionname, this.numofimgs, this.county);

                }
                catch (Newtonsoft.Json.JsonException jsonEx)
                {
                    Console.WriteLine($"Error deserializing API response: {jsonEx.Message}");
                    throw;
                }



                await context.Reply(frontendresponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while processing the message: {ex.ToString()}");
                throw;
            }
        }
    }
}

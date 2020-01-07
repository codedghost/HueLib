using HueLib.ResponseModels.ChildModels;

namespace HueLib.ResponseModels
{
    public class GetUsernameResponse
    {
        public Error error { get; set; }
        public Success success { get; set; }
    }
}
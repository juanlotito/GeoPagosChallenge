using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization; 

namespace PublicApi.Models.Authorization
{
    public class AuthorizationRequest
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }

        [SwaggerSchema(ReadOnly = true, WriteOnly = true)]
        [JsonIgnore]
        public int Type { get; set; }

        [SwaggerSchema(ReadOnly = true, WriteOnly = true)]
        [JsonIgnore]
        public bool RequiresConfirmation { get; set; }
    }

}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace Project.Core.Api.V1.ResponseModels
{
    public class ErrorResponseModel
    {
        public IEnumerable<string> Errors { get; set; }

        public ErrorResponseModel(string error)
        {
            Errors = new List<string> { error };
        }

        public ErrorResponseModel(IEnumerable<string> errors)
        {
            Errors = errors;
        }

        public ErrorResponseModel (string code, string description, ModelStateDictionary modelState)
        {
            modelState.TryAddModelError(code, description);
            Errors = getErrorsFromModelState(modelState);
        }

        public ErrorResponseModel (ModelStateDictionary modelState)
        {
            Errors = getErrorsFromModelState(modelState);
        }

        public ErrorResponseModel(IdentityResult identityResult, ModelStateDictionary modelState)
        {
            foreach (IdentityError e in identityResult.Errors)
            {
                modelState.TryAddModelError(e.Code ,e.Description);
            }
            Errors = getErrorsFromModelState(modelState);
        }

        private IEnumerable<string> getErrorsFromModelState(ModelStateDictionary modelState)
        {
            return modelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentValidation;
using LocateApp.DataTransferObjects;

namespace LocateApp.Validators
{
    public class InsertPatientValidator:AbstractValidator<ModifyInfoPatientRequest>
    {
        public InsertPatientValidator()
        {
            RuleFor(request => request.Matricule).NotNull().NotEmpty().Length(8);
        }
    }

    public class GetCorrectionPatientValidator : AbstractValidator<GetModificationPatientRequest>
    {
        public GetCorrectionPatientValidator()
        {
            RuleFor(request => request.Matricule).NotNull().NotEmpty().Length(8);
        }
    }
}
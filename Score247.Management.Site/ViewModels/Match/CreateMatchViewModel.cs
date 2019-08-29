using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Score247.Management.Site.Repositories.Matches.Models;

namespace Score247.Management.Site.ViewModels.Match
{
    public class CreateMatchViewModel
    {
        public MatchEntity Match { get; set; }
        public IEnumerable<Score247Management.Core.Leagues.Models.League> Tournaments { get; set; }
        public IEnumerable<Score247Management.Core.Teams.Models.Team> Teams { get; set; }

        [TeamDifference("AwayTeam", ErrorMessage = "Not valid")]
        public Score247Management.Core.Teams.Models.Team HomeTeam { get; set; }

        [TeamDifference("HomeTeam", ErrorMessage = "Not valid")]
        public Score247Management.Core.Teams.Models.Team AwayTeam { get; set; }
    }

    public class TeamDifferenceAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly string _comparisonProperty;

        public TeamDifferenceAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            var currentValue = (Score247Management.Core.Teams.Models.Team)value;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                throw new ArgumentException("Property with this name not found");

            var comparisonValue = (Score247Management.Core.Teams.Models.Team)property.GetValue(validationContext.ObjectInstance);

            if (currentValue.Id == comparisonValue.Id)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            var error = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-error", error);
        }
    }
}
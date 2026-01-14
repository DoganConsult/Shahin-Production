using System;
using System.Collections.Generic;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using Xunit;

namespace GrcMvc.Tests.Services
{
    /// <summary>
    /// Unit tests for OnboardingFieldValueProvider
    /// Tests field value extraction from OnboardingWizard entity
    /// </summary>
    public class OnboardingFieldValueProviderTests
    {
        [Fact]
        public void GetFieldValue_WithOrganizationName_ReturnsValue()
        {
            // Arrange
            var wizard = new OnboardingWizard
            {
                OrganizationLegalNameEn = "Test Organization"
            };
            var provider = new OnboardingFieldValueProvider(wizard);

            // Act
            var value = provider.GetFieldValue("SF.S1.organization_name");

            // Assert
            Assert.NotNull(value);
            Assert.Equal("Test Organization", value.ToString());
        }

        [Fact]
        public void HasFieldValue_WithPresentField_ReturnsTrue()
        {
            // Arrange
            var wizard = new OnboardingWizard
            {
                OrganizationLegalNameEn = "Test Organization"
            };
            var provider = new OnboardingFieldValueProvider(wizard);

            // Act
            var hasValue = provider.HasFieldValue("SF.S1.organization_name");

            // Assert
            Assert.True(hasValue);
        }

        [Fact]
        public void HasFieldValue_WithMissingField_ReturnsFalse()
        {
            // Arrange
            var wizard = new OnboardingWizard
            {
                OrganizationLegalNameEn = null
            };
            var provider = new OnboardingFieldValueProvider(wizard);

            // Act
            var hasValue = provider.HasFieldValue("SF.S1.organization_name");

            // Assert
            Assert.False(hasValue);
        }

        [Fact]
        public void GetFieldValue_WithJsonField_ReturnsDeserializedValue()
        {
            // Arrange
            var wizard = new OnboardingWizard
            {
                DataTypesProcessedJson = "[\"PII\", \"Financial\", \"Health\"]"
            };
            var provider = new OnboardingFieldValueProvider(wizard);

            // Act
            var value = provider.GetFieldValue("W.E.1.data_types_processed");

            // Assert
            Assert.NotNull(value);
            // The provider returns List<string> for JSON arrays, verify it's a collection
            Assert.IsAssignableFrom<System.Collections.IEnumerable>(value);
            var list = value as IEnumerable<string>;
            Assert.NotNull(list);
            Assert.Contains("PII", list);
            Assert.Contains("Financial", list);
            Assert.Contains("Health", list);
        }

        [Fact]
        public void GetFieldValue_WithBooleanField_ReturnsBoolean()
        {
            // Arrange
            var wizard = new OnboardingWizard
            {
                HasPaymentCardData = true
            };
            var provider = new OnboardingFieldValueProvider(wizard);

            // Act
            var value = provider.GetFieldValue("W.E.2.payment_card_data");

            // Assert
            Assert.NotNull(value);
            Assert.True((bool)value);
        }

        [Fact]
        public void GetFieldValue_WithIntegerField_ReturnsInteger()
        {
            // Arrange
            var wizard = new OnboardingWizard
            {
                EvidenceSlaSubmitDays = 30
            };
            var provider = new OnboardingFieldValueProvider(wizard);

            // Act
            var value = provider.GetFieldValue("W.I.8.evidence_sla_submit_days");

            // Assert
            Assert.NotNull(value);
            Assert.Equal(30, (int)value);
        }

        [Fact]
        public void GetCollectedFieldIds_WithCompleteWizard_ReturnsAllFields()
        {
            // Arrange
            var wizard = new OnboardingWizard
            {
                OrganizationLegalNameEn = "Test Org",
                CountryOfIncorporation = "Saudi Arabia",
                PrimaryDriver = "RegulatorExam",
                HasPaymentCardData = true,
                EvidenceSlaSubmitDays = 30
            };
            var provider = new OnboardingFieldValueProvider(wizard);

            // Act
            var collectedIds = provider.GetCollectedFieldIds();

            // Assert
            Assert.NotNull(collectedIds);
            Assert.Contains("SF.S1.organization_name", collectedIds);
            Assert.Contains("W.E.2.payment_card_data", collectedIds);
            Assert.Contains("W.I.8.evidence_sla_submit_days", collectedIds);
        }

        [Fact]
        public void GetFieldValue_WithUnknownField_ReturnsNull()
        {
            // Arrange
            var wizard = new OnboardingWizard();
            var provider = new OnboardingFieldValueProvider(wizard);

            // Act
            var value = provider.GetFieldValue("UNKNOWN.FIELD.id");

            // Assert
            Assert.Null(value);
        }

        [Fact]
        public void HasFieldValue_WithEmptyString_ReturnsFalse()
        {
            // Arrange
            var wizard = new OnboardingWizard
            {
                OrganizationLegalNameEn = ""
            };
            var provider = new OnboardingFieldValueProvider(wizard);

            // Act
            var hasValue = provider.HasFieldValue("SF.S1.organization_name");

            // Assert
            Assert.False(hasValue);
        }

        [Theory]
        [InlineData("SF.S1.organization_name", "OrganizationLegalNameEn")]
        [InlineData("W.A.1.legal_name_en", "OrganizationLegalNameEn")]
        [InlineData("W.A.4.country_of_incorporation", "CountryOfIncorporation")]
        [InlineData("W.B.1.primary_driver", "PrimaryDriver")]
        [InlineData("W.E.2.payment_card_data", "HasPaymentCardData")]
        [InlineData("W.G.1.ownership_approach", "ControlOwnershipApproach")]
        [InlineData("W.K.1.adopt_default_baseline", "AdoptDefaultBaseline")]
        public void GetFieldValue_WithVariousFields_ReturnsCorrectValues(string fieldId, string propertyName)
        {
            // Arrange
            var wizard = new OnboardingWizard
            {
                OrganizationLegalNameEn = "Test Org",
                CountryOfIncorporation = "Saudi Arabia",
                PrimaryDriver = "RegulatorExam",
                HasPaymentCardData = true,
                ControlOwnershipApproach = "Centralized",
                AdoptDefaultBaseline = true
            };
            var provider = new OnboardingFieldValueProvider(wizard);

            // Act
            var value = provider.GetFieldValue(fieldId);

            // Assert
            Assert.NotNull(value);
        }
    }
}

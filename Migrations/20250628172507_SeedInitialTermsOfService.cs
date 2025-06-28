using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuminsterBackend.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialTermsOfService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var tosContent = @"# Terms of Service

## 1. Acceptance of Terms
By accessing or using Ruminster (""the Service""), you agree to be bound by these Terms of Service (""Terms""). If you do not agree to these Terms, you may not use the Service.

## 2. Age Requirements
You must be at least 13 years of age to use this Service. If you are between 13 and 18 years of age, you may only use the Service with the consent and supervision of a parent or legal guardian who agrees to be bound by these Terms. Users under 13 years of age are prohibited from using the Service. By using the Service, you represent and warrant that you meet these age requirements.

## 3. Description of Service
Ruminster is a platform that allows users to create, organize, and share their thoughts and ideas through ruminations. The Service includes web-based tools and features to help users manage their personal reflections.

## 4. User Accounts
To use certain features of the Service, you must create an account. You are responsible for:
- Providing accurate and complete registration information
- Maintaining the security of your account credentials
- All activities that occur under your account
- Notifying us immediately of any unauthorized use

## 5. Acceptable Use
You agree not to use the Service to:
- Violate any applicable laws or regulations
- Infringe on the rights of others
- Upload malicious code or engage in harmful activities
- Spam, harass, or abuse other users
- Attempt to gain unauthorized access to the Service

## 6. Privacy and Data
Your privacy is important to us. Our collection and use of your information is governed by our Privacy Policy, which is incorporated into these Terms by reference.

## 7. Content Ownership
You retain ownership of the content you create using the Service. By using the Service, you grant us a limited license to store, process, and display your content as necessary to provide the Service.

## 8. Service Availability
We strive to maintain the availability of the Service, but we do not guarantee uninterrupted access. We may modify, suspend, or discontinue the Service at any time with appropriate notice.

## 9. Limitation of Liability
The Service is provided ""as is"" without warranties of any kind. We shall not be liable for any indirect, incidental, special, or consequential damages arising from your use of the Service.

## 10. Changes to Terms
We may update these Terms from time to time. When we do, we will notify existing users and require acceptance of the updated Terms before continued use of the Service.

## 11. Contact Information
If you have any questions about these Terms, please contact us at support@ruminster.com.";

            migrationBuilder.InsertData(
                table: "terms_of_service",
                columns: new[] { "version", "content", "created_at", "is_active" },
                values: new object[] { "1.0", tosContent, DateTime.UtcNow, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "terms_of_service",
                keyColumn: "version",
                keyValue: "1.0");
        }
    }
}
